using System;

namespace Core.Objects.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime NextMidnight(this DateTime date)
        {
            DateTime updatedDate = date.Date.AddDays(1);
            return new DateTime(updatedDate.Year, updatedDate.Month, updatedDate.Day);
        }

        public static DateTimeOffset NextMidnight(this DateTimeOffset date) => new(date.Date.NextMidnight(), TimeSpan.Zero);
        public static DateTimeOffset AsUTCOffset(this DateTime datetime) => new(datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, datetime.Second, TimeSpan.Zero);

        public static DateTimeOffset? AsUTCOffset(this DateTime? nullableDateTime)
        {
            if (nullableDateTime != null)
            {
                DateTime datetime = nullableDateTime.Value;
                return new DateTimeOffset(datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, datetime.Second, TimeSpan.Zero);
            }

            return null;
        }
    }
}
