using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Kowalski.Extensions;
using Kowalski.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Kowalski
{
    public static class Speech
    {
        [FunctionName("Speech")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            HttpResponseMessage response = null;

            var text = req.GetQueryStringValue("q");
            log.Info($"Request text: {text}");

            if (string.IsNullOrWhiteSpace(text))
            {
                response = req.CreateErrorResponse(HttpStatusCode.BadRequest, "You must specify the message in the 'q' query string parameter", log);
            }
            else
            {
                var speechSubscriptionKey = ConfigurationManager.AppSettings["SpeechSubscriptionKey"];
                if (string.IsNullOrWhiteSpace(speechSubscriptionKey))
                {
                    response = req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Speech Subscription Key is missing from application settings", log);
                }
                else
                {
                    // Create the service for text-to-speech using Cognitive Services Speech.
                    var cortana = new Synthesize();
                    var auth = new Authentication(ConfigurationManager.AppSettings["AuthenticationUri"], speechSubscriptionKey);

                    var responseStream = await cortana.SpeakAsync(new InputOptions(ConfigurationManager.AppSettings["AppName"], ConfigurationManager.AppSettings["AppId"], ConfigurationManager.AppSettings["ClientId"])
                    {
                        RequestUri = new Uri(ConfigurationManager.AppSettings["SpeechRequestUri"]),
                        Text = text,
                        VoiceType = (Gender)Enum.Parse(typeof(Gender), ConfigurationManager.AppSettings["Gender"] ?? "Male", true),
                        Locale = ConfigurationManager.AppSettings["Culture"] ?? "it-IT",
                        VoiceName = ConfigurationManager.AppSettings["VoiceName"] ?? "Microsoft Server Speech Text to Speech Voice (it-IT, Cosimo, Apollo)",

                        // Service can return audio in different output format.
                        OutputFormat = AudioOutputFormat.Audio24Khz160KBitRateMonoMp3,
                        AuthorizationToken = "Bearer " + auth.GetAccessToken(),
                    });

                    // Sends the response to the client.
                    response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(responseStream)
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/mp3");
                }
            }

            return response;
        }
    }
}
