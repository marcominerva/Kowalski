using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kowalski.BusinessLayer
{
    public static class Messages
    {
        public const string NotUnderstood = "Mi dispiace, non ho capito di cosa hai bisogno. Hai detto '{0}'.";
        public const string NotFound = "Mi dispiace, non ho trovato nessuna informazione utile su '{0}'.";

        public const string Time = "Sono le ore {0}.";
        public const string TodayDate = "Oggi è {0}.";
        public const string TomorrowDate = "Domani sarà {0}.";

        public const string WeatherSingular = "A {0} oggi c'è {1}, con una temperatura di {2} gradi.";
        public const string WeatherPlural = "A {0} oggi ci sono {1}, con una temperatura di {2} gradi.";
    }
}