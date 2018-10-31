using System;

namespace Kowalski.BusinessLayer.Models
{
    public class AppSettings
    {
        public string TimeZone { get; set; }

        public string Culture { get; set; }

        public string DateFormat { get; set; }

        public string TimeFormat { get; set; }

        public double MinimumScore { get; set; }

        public string SpeechUri { get; set; }

        public string SearchUri { get; set; }

        public string SearchSubscriptionKey { get; set; }

        public string WeatherServiceUri { get; set; }
    }
}
