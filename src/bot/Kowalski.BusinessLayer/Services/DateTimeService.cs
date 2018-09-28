using Kowalski.BusinessLayer.Extensions;
using System;
using System.Configuration;
using System.Globalization;

namespace Kowalski.BusinessLayer.Services
{
    public static class DateTimeService
    {
        private static readonly string timeZone;
        private static readonly string timeFormat;
        private static readonly string culture;
        private static readonly string dateFormat;

        static DateTimeService()
        {
            culture = ConfigurationManager.AppSettings["Culture"];
            timeZone = ConfigurationManager.AppSettings["TimeZone"];
            timeFormat = ConfigurationManager.AppSettings["TimeFormat"];
            dateFormat = ConfigurationManager.AppSettings["DateFormat"];
        }

        public static string GetTime()
        {
            var localTime = ConvertDate(DateTime.UtcNow, 0, timeZone);
            var message = string.Format(Messages.Time, localTime.ToString(timeFormat));

            return message;
        }

        public static string GetDate(string day)
        {
            var localTime = ConvertDate(DateTime.UtcNow, 0, timeZone);

            var addDays = 0;
            var baseMessage = Messages.TodayDate;
            if (day.EqualsIgnoreCase("domani"))
            {
                addDays = 1;
                baseMessage = Messages.TomorrowDate;
            }

            var message = string.Format(baseMessage, localTime.Date.AddDays(addDays).ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture)));
            return message;
        }

        private static DateTime ConvertDate(DateTime inputTime, int fromOffset, string toZone)
        {
            // Ensure that the given date and time is not a specific kind.
            inputTime = DateTime.SpecifyKind(inputTime, DateTimeKind.Unspecified);

            var fromTimeOffset = new TimeSpan(0, -fromOffset, 0);
            var to = TimeZoneInfo.FindSystemTimeZoneById(toZone);
            var offset = new DateTimeOffset(inputTime, fromTimeOffset);
            var destination = TimeZoneInfo.ConvertTime(offset, to);
            return destination.DateTime;
        }
    }
}