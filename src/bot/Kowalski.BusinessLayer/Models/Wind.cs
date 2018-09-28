using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kowalski.BusinessLayer.Models
{
    public class Wind
    {
        public decimal Speed { get; set; }

        [JsonProperty("deg")]
        public double Degree { get; set; }
    }
}