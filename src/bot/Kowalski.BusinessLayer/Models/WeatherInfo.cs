using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kowalski.BusinessLayer.Models
{
    public class WeatherInfo
    {
        public int Id { get; set; }

        [JsonProperty("main")]
        public string Condition { get; set; }

        public string Description { get; set; }

        [JsonProperty("icon")]
        public string ConditionIcon { get; set; }
    }
}