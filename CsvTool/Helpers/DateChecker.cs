using System;
using System.Collections.Generic;
using System.Globalization;

namespace Bygdrift.Tools.CsvTool.Helpers
{
    /// <summary>
    /// A class that verifies if a value is a date
    /// </summary>
    public class DateChecker
    {
        ///First look if string length is between dateFormats
        ///Then look if the amount of numbers are OK
        ///Then look if any of the delimiters are pressent
        ///Users should be able to add the dateFormats
        ///Remember to handle null
        ///Use the info from Azure about timezone

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

        public DateChecker(Config config)
        {
            this.config = config;
        }

        internal bool TryParse(object input, out DateTime value)
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
                    {
                        //value = value.ToString("s");
                        return true;
                    }

                foreach (var item in config.DateFormats)
                {
                    if (TryParse(valueAsString, item, out value))
                    {
                        DateFormatsUsed.Add(item);
                        config.DateFormats.Remove(item);
                        return true;
                    }
                }
            }
            value = default!;
            return false;
        }

        private static bool TryParse(string s, string format, out DateTime value)
        {
            return DateTime.TryParseExact(s, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out value);
        }
    }
}
