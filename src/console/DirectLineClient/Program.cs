using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json;
using WebSocketSharp;

namespace DirectLineSampleClient
{

    public class Program
    {
        private static readonly string directLineSecret = ConfigurationManager.AppSettings["DirectLineSecret"];
        private static readonly string botId = ConfigurationManager.AppSettings["BotId"];
        private static readonly string fromUser = "DirectLineSampleClientUser";

        public static async Task Main(string[] args)
        {
            await StartBotConversationAsync();
        }

        private static async Task StartBotConversationAsync()
        {
            // Obtain a token using the Direct Line secret
            var tokenResponse = await new DirectLineClient(directLineSecret).Tokens.GenerateTokenForNewConversationAsync();

            // Use token to create conversation
            var directLineClient = new DirectLineClient(tokenResponse.Token);
            var conversation = await directLineClient.Conversations.StartConversationAsync();

            Console.Write("\nCommand> ");

            using (var webSocketClient = new WebSocket(conversation.StreamUrl))
            {
                webSocketClient.OnMessage += WebSocketClient_OnMessage;
                webSocketClient.Connect();

                while (true)
                {
                    var input = Console.ReadLine().Trim();
                    if (input.ToLower() == "exit")
                    {
                        break;
                    }
                    else
                    {
                        if (input.Length > 0)
                        {
                            var userMessage = new Activity
                            {
                                From = new ChannelAccount(fromUser),
                                Text = input,
                                Type = ActivityTypes.Message
                            };

                            await directLineClient.Conversations.PostActivityAsync(conversation.ConversationId, userMessage);
                        }
                    }
                }
            }
        }

        private static void WebSocketClient_OnMessage(object sender, MessageEventArgs e)
        {
            // Occasionally, the Direct Line service sends an empty message as a liveness ping. Ignore these messages.
            if (string.IsNullOrWhiteSpace(e.Data))
            {
                return;
            }

            var activitySet = JsonConvert.DeserializeObject<ActivitySet>(e.Data);
            var activities = from x in activitySet.Activities
                             where x.From.Id == botId
                             select x;

            foreach (var activity in activities)
            {
                Console.WriteLine(activity.Text);

                if (activity.Attachments != null)
                {
                    foreach (var attachment in activity.Attachments)
                    {
                        switch (attachment.ContentType)
                        {
                            case "application/vnd.microsoft.card.hero":
                                RenderHeroCard(attachment);
                                break;

                            case "image/png":
                                Console.WriteLine($"Opening the requested image '{attachment.ContentUrl}'");

                                Process.Start(attachment.ContentUrl);
                                break;
                        }
                    }
                }

                Console.Write("\nCommand> ");
            }
        }

        private static void RenderHeroCard(Attachment attachment)
        {
            const int Width = 70;
            Func<string, string> contentLine = (content) => string.Format($"{{0, -{Width}}}", string.Format("{0," + ((Width + content.Length) / 2).ToString() + "}", content));

            var heroCard = JsonConvert.DeserializeObject<HeroCard>(attachment.Content.ToString());

            if (heroCard != null)
            {
                Console.WriteLine("/{0}", new string('*', Width + 1));
                Console.WriteLine("*{0}*", contentLine(heroCard.Title));
                Console.WriteLine("*{0}*", new string(' ', Width));
                Console.WriteLine("*{0}*", contentLine(heroCard.Text));
                Console.WriteLine("{0}/", new string('*', Width + 1));
            }
        }
    }
}
