using System;
using System.Collections.Generic;
using System.Globalization;

namespace Bygdrift.Tools.CsvTool.Helpers
{
    /// <summary>
    /// A class that verifies if a value is a date
    /// </summary>
    public class DateHelper
    {
        private readonly string formatLocal = "s";
        private readonly string formatOffset = "yyyy-MM-ddTHH:mm:sszzz";
        private readonly string formatUtc = "yyyy-MM-ddTHH:mm:ss'Z'";

        /// <summary>
        /// Gets filled with dateformats that has worked. This way, only a few formats are looped
        /// </summary>
        public List<string> DateFormatsUsed = new();

        /// <summary>
        /// The min length of the string
        /// </summary>
        public int MinLength = 8;

        /// <summary>
        /// The max length of the string
        /// </summary>
        public int MaxLength = 29;
        private readonly Config config;

        /// <summary>
        /// 
        /// </summary>
        public DateHelper(Config config)
        {
            this.config = config;
        }


        /// <summary>Returns dateTimeOffset as decriped in config. </summary>
        public DateTimeOffset Now()
        {
            return Now(config.FormatKind);
        }

        /// <summary>Returns dateTimeOffset as decriped in config. </summary>
        public DateTimeOffset Now(DateTimeOffset utcNow)
        {
            return Now(config.FormatKind, utcNow);
        }

        /// <summary>Returns dateTimeOffset as decriped in formatKind </summary>
        public DateTimeOffset Now(FormatKind formatKind)
        {
            var utcNow = DateTimeOffset.UtcNow;
            return Now(formatKind, utcNow);
        }

        /// <summary>Returns dateTimeOffset as decriped in formatKind</summary>
        public DateTimeOffset Now(FormatKind formatKind, DateTimeOffset utcNow)
        {
            if (formatKind == FormatKind.Universal)
                return utcNow;
            if (formatKind == FormatKind.TimeOffsetUTC)
                return new DateTimeOffset(UtcToLocal(utcNow), config.TimeZoneInfo.BaseUtcOffset);
            if (formatKind == FormatKind.TimeOffsetDST)
                return UtcToLocal(utcNow);
            if (formatKind == FormatKind.Local)
                return DateTime.SpecifyKind(UtcToLocal(utcNow), DateTimeKind.Local);
            else
                throw new Exception();
        }

        /// <summary>Converts universal time to local time defined by config</summary>
        public DateTime UtcToLocal(DateTimeOffset utc)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utc.DateTime, config.TimeZoneInfo);
        }


        /// <summary>Returns dateTime as a string as decriped in config. </summary>
        public string DateTimeToString(DateTime dateTime)
        {
            if (config.FormatKind == FormatKind.Universal)
            {
                var utc = config.TimeZoneInfo != null ? TimeZoneInfo.ConvertTimeFromUtc(dateTime, config.TimeZoneInfo) : dateTime.ToUniversalTime();
                return utc.ToString(formatUtc);
            }
            if (config.FormatKind == FormatKind.TimeOffsetUTC || config.FormatKind == FormatKind.TimeOffsetDST)
            {
                var time = ToDateTimeOffset(dateTime);
                return time.ToString(formatOffset);
            }
            if (config.FormatKind == FormatKind.Local)
                return dateTime.ToString(formatLocal);
            else
                throw new Exception();
        }

        /// <summary>
        /// Returns dateTime as a string as decriped in config. 
        /// </summary>
        public string DateTimeToString(DateTimeOffset dateTimeOffset)
        {
            if (config.FormatKind == FormatKind.Universal)
                return dateTimeOffset.ToUniversalTime().ToString(formatUtc);
            if (config.FormatKind == FormatKind.TimeOffsetUTC || config.FormatKind == FormatKind.TimeOffsetDST)
                return dateTimeOffset.ToString(formatOffset);
            else
                return dateTimeOffset.ToString(formatLocal);
        }

        /// <summary>Convert DateTimeOffset to DateTime</summary>
        public DateTime ToDateTime(DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.DateTime;
        }

        /// <summary>Convert DateTime to DateTimeOffset</summary>
        public DateTimeOffset ToDateTimeOffset(DateTime dateTime)
        {
            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            return config.TimeZoneInfo != null ? new DateTimeOffset(dateTime, GetOffset(dateTime)) : new DateTimeOffset(dateTime);
        }

        /// <summary>Returns the offset, depending on the formatKind</summary>
        private TimeSpan GetOffset(DateTime dateTime)
        {
            if (config.TimeZoneInfo != null)
            {
                if (config.FormatKind == FormatKind.TimeOffsetDST)
                    return config.TimeZoneInfo.GetUtcOffset(dateTime);
                if (config.FormatKind == FormatKind.TimeOffsetUTC)
                    return config.TimeZoneInfo.BaseUtcOffset;
            }
            return default;
        }

        /// <summary>
        /// The length of the DateTime when returned
        /// </summary>
        public int DateTimeLength
        {
            get
            {
                if (config.FormatKind == FormatKind.Universal)
                    return 20;
                if (config.FormatKind == FormatKind.TimeOffsetUTC || config.FormatKind == FormatKind.TimeOffsetDST)
                    return 25;  //2022-06-01T12:00:00+02:00
                if (config.FormatKind == FormatKind.Local)
                    return 19;
                else
                    throw new Exception();
            }
        }

        ///First look if string length is between dateFormats
        ///Then look if the amount of numbers are OK
        ///Then look if any of the delimiters are pressent
        ///Users should be able to add the dateFormats
        ///Remember to handle null
        ///Use the info from Azure about timezone
        internal bool TryParseDateTime(object input, out DateTime value)
        {
            if (input.GetType() == typeof(DateTime))
            {
                value = (DateTime)input;
                return true;
            }

            var valueAsString = input.ToString();
            if (valueAsString.Length >= MinLength && valueAsString.Length <= MaxLength)
            {
                foreach (var item in DateFormatsUsed)
                    if (TryParse(valueAsString, item, out value))
                        return true;

                foreach (var item in config.Formats)
                {
                    if (TryParse(valueAsString, item, out value))
                    {
                        DateFormatsUsed.Add(item);
                        config.Formats.Remove(item);
                        return true;
                    }
                }
            }
            value = default;
            return false;
        }

        internal bool TryParseDateTimeOffset(object input, out DateTimeOffset value)
        {
            if (input.GetType() == typeof(DateTimeOffset))
            {
                value = (DateTimeOffset)input;
                return true;
            }

            var valueAsString = input.ToString();
            if (valueAsString.Length == 25)
            {
                if (DateTimeOffset.TryParse(valueAsString, out value))
                    return true;
            }
            value = default;
            return false;
        }

        private static bool TryParse(string s, string format, out DateTime value)
        {
            return DateTime.TryParseExact(s, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out value);
        }
    }
}