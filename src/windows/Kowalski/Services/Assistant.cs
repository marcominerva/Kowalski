using Kowalski.Common;
using Kowalski.Models;
using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.Networking.Sockets;
using Windows.Storage;

namespace Kowalski.Services
{
    public static class Assistant
    {
        private static SpeechRecognizer assistantInvokerSpeechRecognizer;
        private static SpeechRecognizer commandSpeechRecognizer;

        private static DirectLineClient directLineClient;
        private static Conversation conversation;
        private static MessageWebSocket webSocketClient;

        public static event EventHandler OnStartRecognition;
        public static event EventHandler OnCommandReceived;
        public static event EventHandler<BotEventArgs> OnResponseReceived;

        private static bool isRunning;

        public static async void StartService()
        {
            if (isRunning)
            {
                // If the service is already running, exits.
                return;
            }

            isRunning = true;

            if (Settings.Instance == null)
            {
                await LoadSettingsAsync();
            }

            if (directLineClient == null)
            {
                // Obtain a token using the Direct Line secret
                var tokenResponse = await new DirectLineClient(Settings.Instance.DirectLineSecret).Tokens.GenerateTokenForNewConversationAsync();

                // Use token to create conversation
                directLineClient = new DirectLineClient(tokenResponse.Token);
                conversation = await directLineClient.Conversations.StartConversationAsync();

                // Connect using a WebSocket.
                webSocketClient = new MessageWebSocket();
                webSocketClient.MessageReceived += WebSocketClient_MessageReceived;
                await webSocketClient.ConnectAsync(new Uri(conversation.StreamUrl));
            }

            if (assistantInvokerSpeechRecognizer == null)
            {
                // Create an instance of SpeechRecognizer.
                assistantInvokerSpeechRecognizer = new SpeechRecognizer(new Language(Settings.Instance.Culture));
                assistantInvokerSpeechRecognizer.Timeouts.InitialSilenceTimeout = TimeSpan.MaxValue;
                assistantInvokerSpeechRecognizer.Timeouts.BabbleTimeout = TimeSpan.MaxValue;

                // Add a list constraint to the recognizer.
                var listConstraint = new SpeechRecognitionListConstraint(new string[] { Settings.Instance.AssistantName }, "assistant");
                assistantInvokerSpeechRecognizer.Constraints.Add(listConstraint);
                await assistantInvokerSpeechRecognizer.CompileConstraintsAsync();
            }

            if (commandSpeechRecognizer == null)
            {
                commandSpeechRecognizer = new SpeechRecognizer(new Language(Settings.Instance.Culture));

                // Apply the dictation topic constraint to optimize for dictated freeform speech.
                var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.WebSearch, "dictation");
                commandSpeechRecognizer.Constraints.Add(dictationConstraint);
                await commandSpeechRecognizer.CompileConstraintsAsync();
            }

            // The assistant is ready to receive input.
            SoundPlayer.Instance.Play(Sounds.SpeechActive);

            while (isRunning)
            {
                try
                {
                    var assistantInvocationResult = await assistantInvokerSpeechRecognizer.RecognizeAsync();
                    if (assistantInvocationResult.Status == SpeechRecognitionResultStatus.Success && assistantInvocationResult.Confidence != SpeechRecognitionConfidence.Rejected)
                    {
                        OnStartRecognition?.Invoke(null, EventArgs.Empty);
                        SoundPlayer.Instance.Play(Sounds.Ready);

                        // Starts command recognition. It returns when the first utterance has been recognized.
                        var commandResult = await commandSpeechRecognizer.RecognizeAsync();
                        if (commandResult.Status == SpeechRecognitionResultStatus.Success && commandResult.Confidence != SpeechRecognitionConfidence.Rejected)
                        {
                            var command = commandResult.Text.EndsWith("?") ? commandResult.Text : $"{commandResult.Text}?";
                            Debug.WriteLine(command);

                            OnCommandReceived?.Invoke(null, EventArgs.Empty);

                            // Sends the activity to the Bot. The answer will be received in the WebSocket received event handler.
                            var userMessage = new Activity
                            {
                                From = new ChannelAccount(Settings.Instance.UserName),
                                Text = command,
                                Type = ActivityTypes.Message
                            };

                            await directLineClient.Conversations.PostActivityAsync(conversation.ConversationId, userMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnResponseReceived?.Invoke(null, new BotEventArgs(ex.Message));
                }
            }

            // Clean up used resources.
            SoundPlayer.Instance.Play(Sounds.SpeechStopped);

            assistantInvokerSpeechRecognizer?.Dispose();
            commandSpeechRecognizer?.Dispose();
            webSocketClient?.Dispose();
            directLineClient?.Dispose();

            assistantInvokerSpeechRecognizer = null;
            commandSpeechRecognizer = null;
            webSocketClient = null;
            conversation = null;
            directLineClient = null;
        }

        private static async void WebSocketClient_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            if (args.MessageType == SocketMessageType.Utf8)
            {
                string jsonOutput = null;
                using (var dataReader = args.GetDataReader())
                {
                    dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    jsonOutput = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                }

                // Occasionally, the Direct Line service sends an empty message as a liveness ping. Ignore these messages.
                if (string.IsNullOrWhiteSpace(jsonOutput))
                {
                    return;
                }

                var activitySet = JsonConvert.DeserializeObject<ActivitySet>(jsonOutput);
                var activities = activitySet.Activities.Where(a => a.From.Id == Settings.Instance.BotId);

                foreach (var activity in activities)
                {
                    Debug.WriteLine(activity.Text);
                    OnResponseReceived?.Invoke(null, new BotEventArgs(activity.Text));

                    // If there is an audio attached to the response, automatically speaks it.
                    var audioResponseUrl = activity.Attachments?.FirstOrDefault(a => a.ContentType == "audio/mp3")?.ContentUrl;
                    if (audioResponseUrl != null)
                    {
                        await TextToSpeech.SpeakAsync(activity.Speak ?? activity.Text, Settings.Instance.Culture, audioResponseUrl);
                    }
                }
            }
        }

        public static async void StopService()
        {
            try
            {
                await assistantInvokerSpeechRecognizer.StopRecognitionAsync();
            }
            catch
            {
            }

            isRunning = false;
        }

        private static async Task LoadSettingsAsync()
        {
            try
            {
                var storageFolder = KnownFolders.DocumentsLibrary;
                var settingsFile = await storageFolder.GetFileAsync("settings.kowalski");
                var content = await FileIO.ReadTextAsync(settingsFile);

                Settings.Instance = JsonConvert.DeserializeObject<Settings>(content);
            }
            catch
            {
                Settings.Instance = new Settings
                {
                    AssistantName = Constants.AssistantName,
                    BotId = Constants.BotId,
                    Culture = Constants.Culture,
                    DirectLineSecret = Constants.DirectLineSecret,
                    UserName = Constants.UserName,
                    VoiceGender = Constants.VoiceGender
                };
            }
        }
    }
}
