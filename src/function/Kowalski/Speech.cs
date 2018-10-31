using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Kowalski.Models;
using Kowalski.Services;
using System.Threading.Tasks;
using System;
using Microsoft.Net.Http.Headers;

namespace Kowalski
{
    public static class Speech
    {
        private static AppSettings settings = null;

        [FunctionName("Speech")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, ILogger logger, ExecutionContext context)
        {
            var text = req.Query["q"];
            logger.LogInformation($"Request text: {text}");

            if (string.IsNullOrWhiteSpace(text))
            {
                return new BadRequestObjectResult("You must specify the message in the 'q' query string parameter");
            }
            else
            {
                if (settings == null)
                {
                    var config = new ConfigurationBuilder()
                                    .SetBasePath(context.FunctionAppDirectory)
                                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                                    .AddEnvironmentVariables()
                                    .Build();

                    settings = config.GetSection("AppSettings").Get<AppSettings>();
                }

                if (string.IsNullOrWhiteSpace(settings.SpeechSubscriptionKey))
                {
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
                else
                {
                    // Create the service for text-to-speech using Cognitive Services Speech.
                    var ttsClient = new SpeechClient(settings.SpeechRegion, settings.SpeechSubscriptionKey);
                    var responseStream = await ttsClient.SpeakAsync(new TextToSpeechParameters
                    {
                        Text = text,
                        VoiceType = (Gender)Enum.Parse(typeof(Gender), settings.Gender ?? "Male", true),
                        Language = settings.Culture ?? "it-IT",
                        VoiceName = settings.VoiceName ?? "Microsoft Server Speech Text to Speech Voice (it-IT, Cosimo, Apollo)",
                        // Service can return audio in different output format.
                        OutputFormat = AudioOutputFormat.Audio24Khz160KBitRateMonoMp3
                    });

                    var response = new FileStreamResult(responseStream, "audio/mp3");
                    return response;
                }
            }
        }
    }
}
