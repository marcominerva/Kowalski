using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.SpeechSynthesis;
using System.Diagnostics;
using Windows.Media.Playback;
using System.Collections.Generic;
using Windows.Media.Core;
using Windows.Storage.Streams;
using Kowalski.Models;

namespace Kowalski.Services
{
    /// <summary>
    /// Text To Speech Impelemenatation Windows
    /// </summary>
    public static class TextToSpeech
    {
        private readonly static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly static SpeechSynthesizer speechSynthesizer;

        /// <summary>
        /// SpeechSynthesizer
        /// </summary>
        static TextToSpeech() => speechSynthesizer = new SpeechSynthesizer();

        /// <summary>
        /// Speak back text
        /// </summary>
        /// <param name="text">Text to speak</param>
        /// <param name="languageCode">Locale of voice</param>
        /// <param name="cancelToken">Canelation token to stop speak</param>
        /// <exception cref="ArgumentNullException">Thrown if text is null</exception>
        /// <exception cref="ArgumentException">Thrown if text length is greater than maximum allowed</exception>
        public async static Task SpeakAsync(string text, string languageCode, string speechUrl, CancellationToken cancelToken = default)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text), "Text can not be null");
            }

            try
            {
                await semaphore.WaitAsync(cancelToken);

                var player = new MediaPlayer();

                var tcs = new TaskCompletionSource<object>();
                var handler = new TypedEventHandler<MediaPlayer, object>((sender, args) => tcs.TrySetResult(null));

                var voice = SpeechSynthesizer.AllVoices.FirstOrDefault(v => v.Language.StartsWith(languageCode) && v.Gender == Settings.Instance.VoiceGender);
                if (voice != null)
                {
                    speechSynthesizer.Voice = voice;
                    var stream = await speechSynthesizer.SynthesizeTextToStreamAsync(text);

                    player.Source = MediaSource.CreateFromStream(stream, stream.ContentType);
                }
                else
                {
                    // In the system there isn't a voice that can speech the text. Uses the online version.
                    player.Source = MediaSource.CreateFromUri(new Uri(speechUrl));
                }

                try
                {
                    player.MediaEnded += handler;
                    player.Play();

                    void OnCancel()
                    {
                        player.PlaybackSession.PlaybackRate = 0;
                        tcs.TrySetResult(null);
                    }

                    using (cancelToken.Register(OnCancel))
                    {
                        await tcs.Task;
                    }

                    player.MediaEnded -= handler;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to playback stream: " + ex);
                }

            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}