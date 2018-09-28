using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Kowalski.BusinessLayer;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using System.IO;
using System.Net;
using System.Linq;
using Kowalski.BusinessLayer.Services;
using System.Text.RegularExpressions;

namespace Kowalski
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class AssistantDialog : LuisDialog<object>
    {
        private readonly double minimumScore;
        private readonly string culture;

        protected override IntentRecommendation BestIntentFrom(LuisResult result)
        {
            // If the top scoring intent does not reach the minimum score, discards it by overriding it
            // with the None intent.
            if (result.TopScoringIntent.Score < minimumScore)
            {
                var intent = new IntentRecommendation() { Intent = "None", Score = 1.0 };
                result.TopScoringIntent = intent;
            }

            return base.BestIntentFrom(result);
        }

        public AssistantDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
            minimumScore = double.Parse(ConfigurationManager.AppSettings["MinimumScore"], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            culture = ConfigurationManager.AppSettings["Culture"];
        }

        [LuisIntent("time")]
        public Task TimeIntent(IDialogContext context, LuisResult result)
        {
            var message = DateTimeService.GetTime();
            return PostMessageAsync(context, message);
        }

        [LuisIntent("date")]
        public Task DateIntent(IDialogContext context, LuisResult result)
        {
            var message = DateTimeService.GetDate(result.Entities.FirstOrDefault()?.Entity);
            return PostMessageAsync(context, message);
        }

        [LuisIntent("joke")]
        public async Task JokeIntent(IDialogContext context, LuisResult result)
        {
            var message = await JokeService.GetJokeAsync();
            await PostMessageAsync(context, message);
        }

        [LuisIntent("search")]
        public async Task SearchIntent(IDialogContext context, LuisResult result)
        {
            var query = result.Entities.FirstOrDefault()?.Entity;
            if (string.IsNullOrWhiteSpace(query))
            {
                await NoneIntent(context, result);
            }
            else
            {
                var response = await SearchService.SearchAsync(query);

                if (string.IsNullOrWhiteSpace(response))
                {
                    response = string.Format(Messages.NotFound, query);
                }

                await PostMessageAsync(context, response);
            }
        }

        [LuisIntent("weather")]
        public async Task WeatherIntent(IDialogContext context, LuisResult result)
        {
            var city = result.Entities.FirstOrDefault()?.Entity;
            if (string.IsNullOrWhiteSpace(city))
            {
                await NoneIntent(context, result);
            }
            else
            {
                var response = await WeatherService.GetWeatherAsync(city);

                if (string.IsNullOrWhiteSpace(response))
                {
                    response = string.Format(Messages.NotFound, city);
                }

                await PostMessageAsync(context, response);
            }
        }

        [LuisIntent("None")]
        public Task NoneIntent(IDialogContext context, LuisResult result)
        {
            var message = string.Format(Messages.NotUnderstood, result.Query);
            return PostMessageAsync(context, message);
        }

        private async Task PostMessageAsync(IDialogContext context, string message, string speak = null)
        {
            var reply = context.MakeMessage();
            reply.Text = message;
            reply.Speak = speak ?? message;

            var audioAttachment = new Attachment()
            {
                ContentType = "audio/mp3",
                ContentUrl = string.Format(ConfigurationManager.AppSettings["SpeechUri"], Uri.EscapeDataString(speak ?? message))
            };
            reply.Attachments.Add(audioAttachment);

            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }
    }
}