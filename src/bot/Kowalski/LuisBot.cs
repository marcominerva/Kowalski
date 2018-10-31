using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kowalski.BusinessLayer;
using Kowalski.BusinessLayer.Models;
using Kowalski.BusinessLayer.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Kowalski
{
    /// <summary>
    /// For each interaction from the user, an instance of this class is created and
    /// the OnTurnAsync method is called.
    /// This is a transient lifetime service. Transient lifetime services are created
    /// each time they're requested. For each <see cref="Activity"/> received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class LuisBot : IBot
    {
        /// <summary>
        /// Key in the bot config (.bot file) for the LUIS instance.
        /// In the .bot file, multiple instances of LUIS can be configured.
        /// </summary>
        public static readonly string LuisKey = "Kowalski";

        /// <summary>
        /// Services configured from the ".bot" file.
        /// </summary>
        private readonly BotServices services;

        private readonly AppSettings settings;
        private readonly IDateTimeService dateTimeService;
        private readonly IJokeService jokeService;
        private readonly ISearchService searchService;
        private readonly IWeatherService weatherService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisBot"/> class.
        /// </summary>
        /// <param name="services">Services configured from the ".bot" file.</param>
        /// <param name="settings">Application settings</param>
        public LuisBot(BotServices services, IOptions<AppSettings> settings,
            IDateTimeService dateTimeService, IJokeService jokeService, ISearchService searchService, IWeatherService weatherService)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
            if (!this.services.LuisServices.ContainsKey(LuisKey))
            {
                throw new ArgumentException($"Invalid configuration. Please check your '.bot' file for a LUIS service named '{LuisKey}'.");
            }

            this.settings = settings.Value;

            this.dateTimeService = dateTimeService;
            this.jokeService = jokeService;
            this.searchService = searchService;
            this.weatherService = weatherService;
        }

        /// <summary>
        /// Every conversation turn for our LUIS Bot will call this method.
        /// There are no dialogs used, the sample only uses "single turn" processing,
        /// meaning a single request and response, with no stateful conversation.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            try
            {
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    // Check LUIS model
                    var recognizerResult = await services.LuisServices[LuisKey].RecognizeAsync(turnContext, cancellationToken);
                    var topIntent = recognizerResult?.GetTopScoringIntent();
                    if (topIntent != null && topIntent.HasValue && topIntent.Value.intent != "None" && topIntent.Value.score > settings.MinimumScore)
                    {
                        await ProcessAsync(turnContext, topIntent.Value, recognizerResult.Entities, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        var message = NotUnderstoodReply(turnContext);
                        await PostMessageAsync(turnContext, message, cancellationToken: cancellationToken);
                    }
                }
                else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
                {
                    // Send a welcome message to the user and tell them what actions they may perform to use this bot
                }

            }
            catch (Exception ex)
            {
                await PostMessageAsync(turnContext, ex.Message, cancellationToken: cancellationToken);
            }
        }

        private async Task ProcessAsync(ITurnContext turnContext, (string Intent, double Score) value, JObject entities, CancellationToken cancellationToken = default)
        {
            string message = null;

            switch (value.Intent)
            {
                case "time":
                    message = dateTimeService.GetTime();
                    break;

                case "date":
                    message = dateTimeService.GetDate(entities["day"]?.FirstOrDefault()?.ToString());
                    break;

                case "joke":
                    message = await jokeService.GetJokeAsync();
                    break;

                case "search":
                    var query = entities["name"]?.FirstOrDefault()?.ToString();
                    if (string.IsNullOrWhiteSpace(query))
                    {
                        message = NotUnderstoodReply(turnContext);
                    }
                    else
                    {
                        message = await searchService.SearchAsync(query);
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            message = string.Format(Messages.NotFound, query);
                        }
                    }

                    break;

                case "weather":
                    var location = entities["location"]?.FirstOrDefault()?.ToString();
                    if (string.IsNullOrWhiteSpace(location))
                    {
                        message = NotUnderstoodReply(turnContext);
                    }
                    else
                    {
                        message = await weatherService.GetWeatherAsync(location);
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            message = string.Format(Messages.NotFound, location);
                        }
                    }
                    break;
            }

            await PostMessageAsync(turnContext, message, cancellationToken: cancellationToken);
        }

        private async Task PostMessageAsync(ITurnContext turnContext, string message, string speak = null, CancellationToken cancellationToken = default)
        {
            var reply = turnContext.Activity.CreateReply(message);
            reply.Speak = speak ?? message;

            var audioAttachment = new Attachment()
            {
                ContentType = "audio/mp3",
                ContentUrl = string.Format(settings.SpeechUri, Uri.EscapeDataString(speak ?? message))
            };
            reply.Attachments.Add(audioAttachment);

            await turnContext.SendActivityAsync(reply);
        }

        private string NotUnderstoodReply(ITurnContext turnContext) => string.Format(Messages.NotUnderstood, turnContext.Activity.Text);

    }
}
