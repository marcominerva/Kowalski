using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kowalski.BusinessLayer.Models
{
    public class Clouds
    {
        [JsonProperty("all")]
        public int Cloudiness { get; set; }
    }

    public class Position
    {
        [JsonProperty("lon")]
        public double Longitude { get; set; }

        [JsonProperty("lat")]
        public double Latitude { get; set; }
    }

    public class WeatherInfo
    {
        public int Id { get; set; }

        [JsonProperty("main")]
        public string Condition { get; set; }

        public string Description { get; set; }

        [JsonProperty("icon")]
        public string ConditionIcon { get; set; }
    }

    public class Wind
    {
        public decimal Speed { get; set; }

        [JsonProperty("deg")]
        public double Degree { get; set; }
    }

    public class CurrentWeatherDetail
    {
        [JsonProperty("temp")]
        public decimal Temperature { get; set; }

        public double Pressure { get; set; }

        public int Humidity { get; set; }
    }

    public class Sun
    {
        public string Country { get; set; }

        [JsonConverter(typeof(UnixToDateTimeConverter))]
        public DateTime Sunrise { get; set; }

        [JsonConverter(typeof(UnixToDateTimeConverter))]
        public DateTime Sunset { get; set; }
    }

    public class CurrentWeather
    {
        [JsonProperty("coord")]
        public Position Position { get; set; }

        [JsonProperty("weather")]
        public IEnumerable<WeatherInfo> WeatherInfo { get; set; }

        [JsonProperty("main")]
        public CurrentWeatherDetail WeatherDetail { get; set; }

        public int Visibility { get; set; }

        public Wind Wind { get; set; }

        public Clouds Clouds { get; set; }

        [JsonProperty("sys")]
        public Sun Sun { get; set; }

        [JsonProperty("dt")]
        [JsonConverter(typeof(UnixToDateTimeConverter))]
        public DateTime Date { get; set; }

        public string Name { get; set; }

        [JsonProperty("cod")]
        public int Code { get; set; }
    }
}