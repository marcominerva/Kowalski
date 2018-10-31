using Kowalski.BusinessLayer.Extensions;
using Kowalski.BusinessLayer.Models;
using Microsoft.Extensions.Options;
using System;
using System.Configuration;
using System.Globalization;

namespace Kowalski.BusinessLayer.Services
{
    public class DateTimeService : IDateTimeService
    {
        private readonly AppSettings settings;

        public DateTimeService(IOptions<AppSettings> settings)
        {
            this.settings = settings.Value;
        }

        public string GetTime()
        {
            var localTime = ConvertDate(DateTime.UtcNow, 0, settings.TimeZone);
            var message = string.Format(Messages.Time, localTime.ToString(settings.TimeFormat));

            return message;
        }

        public string GetDate(string day)
        {
            var localTime = ConvertDate(DateTime.UtcNow, 0, settings.TimeZone);

            var addDays = 0;
            var baseMessage = Messages.TodayDate;
            if (day.EqualsIgnoreCase("domani"))
            {
                addDays = 1;
                baseMessage = Messages.TomorrowDate;
            }

            var message = string.Format(baseMessage, localTime.Date.AddDays(addDays).ToString(settings.DateFormat, CultureInfo.CreateSpecificCulture(settings.Culture)));
            return message;
        }

        private DateTime ConvertDate(DateTime inputTime, int fromOffset, string toZone)
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