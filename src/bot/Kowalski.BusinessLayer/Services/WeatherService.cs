using Kowalski.BusinessLayer.Extensions;
using Kowalski.BusinessLayer.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kowalski.BusinessLayer.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly AppSettings settings;
        private readonly IHttpClientFactory httpClientFactory;

        public WeatherService(IOptions<AppSettings> settings, IHttpClientFactory httpClientFactory)
        {
            this.settings = settings.Value;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetWeatherAsync(string location)
        {
            var url = string.Format(settings.WeatherServiceUri, Uri.EscapeDataString(location));

            try
            {
                var client = httpClientFactory.CreateClient();
                var json = await client.GetStringAsync(url);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return null;
                }

                var weather = JsonConvert.DeserializeObject<CurrentWeather>(json);

                // Checks the correct message to use.
                var message = Messages.WeatherSingular;
                switch (weather.WeatherInfo.FirstOrDefault().Id)
                {
                    case 801:
                    case 802:
                    case 803:
                    case 804:
                        message = Messages.WeatherPlural;

                        break;
                }

                message = string.Format(message, location, weather.WeatherInfo.FirstOrDefault().Description, Math.Round(weather.WeatherDetail.Temperature, 0));
                return message;
            }
            catch
            {
                return null;
            }
        }
    }
}