using Kowalski.BusinessLayer.Extensions;
using Kowalski.BusinessLayer.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kowalski.BusinessLayer.Services
{
    public static class WeatherService
    {
        private static readonly string weatherServiceUri;

        static WeatherService()
        {
            weatherServiceUri = ConfigurationManager.AppSettings["WeatherServiceUri"];
        }

        public static async Task<string> GetWeatherAsync(string city)
        {
            var url = string.Format(weatherServiceUri, Uri.EscapeDataString(city));

            try
            {
                using (var client = new HttpClient())
                {
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

                    message = string.Format(message, weather.Name, weather.WeatherInfo.FirstOrDefault().Description, Math.Round(weather.WeatherDetail.Temperature, 0));
                    return message;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}