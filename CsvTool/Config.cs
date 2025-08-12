using Bygdrift.Tools.CsvTool.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Bygdrift.Tools.CsvTool
{
    /// <summary>Configuration of how to read/write csv</summary>
    public class Config
    {
        private DateHelper _dateHelper;
        private TimeZoneInfo _timeZoneInfo;
        private bool timeZoneInfoIsSet;

        /// <summary>
        /// 
        /// </summary>
        public Config()
        {
            CultureInfo = CultureInfo.InvariantCulture;
            TimeZoneId = null;
            DateFormatKind = DateFormatKind.Local;
        }

        /// <summary>
        /// 
        /// </summary>
        public Config(char delimiter)
        {
            Delimiter = delimiter;
            CultureInfo = CultureInfo.InvariantCulture;
            TimeZoneId = null;
            DateFormatKind = DateFormatKind.Local;
        }


        /// <param name="cultureName">Like 'da-DK' or 'en-US' or 'en-GB' and so on. If null then invariant culture is used</param>
        public Config(string cultureName)
        {
            CultureInfo = cultureName != null ? CultureInfo.CreateSpecificCulture(cultureName) : CultureInfo.InvariantCulture;
            TimeZoneId = null; // "GMT Standard Time";
            DateFormatKind = DateFormatKind.Local;
        }

        /// <param name="cultureName">Like 'da-DK' or 'en-US' or 'en-GB' and so on. If null then invariant culture is used</param>
        /// <param name="timeZoneId">Like: 'Romance Standard Time'. If null, then local time zone is used.  Get timeZoneId from here: https://raw.githubusercontent.com/Bygdrift/Warehouse/master/Docs/TimeZoneIds.csv</param>
        /// <param name="timeFormat"></param>
        /// <param name="delimiter"></param>
        public Config(string cultureName, string timeZoneId, DateFormatKind timeFormat, char delimiter = ',')
        {
            CultureInfo = cultureName != null ? CultureInfo.CreateSpecificCulture(cultureName) : CultureInfo.InvariantCulture;
            TimeZoneId = timeZoneId;
            DateFormatKind = timeFormat;
            Delimiter = delimiter;
        }

        /// <param name="cultureInfo">Like 'da-DK' or 'en-US' or 'en-GB' and so on. If null then invariant culture is used</param>
        /// <param name="timeZoneInfo">Like: 'Romance Standard Time'. If null, then local time zone is used. Get timeZoneId from here: https://raw.githubusercontent.com/Bygdrift/Warehouse/master/Docs/TimeZoneIds.csv</param>
        /// <param name="timeFormat"></param>
        /// <param name="delimiter"></param>
        public Config(CultureInfo cultureInfo, TimeZoneInfo timeZoneInfo, DateFormatKind timeFormat, char delimiter = ',')
        {
            CultureInfo = cultureInfo;
            TimeZoneInfo = timeZoneInfo;
            DateFormatKind = timeFormat;
            Delimiter = delimiter;
        }

        /// <summary>
        /// A class that verifies if a value is a date. It is possible to replace the predefined date formats, if another culture than the predefined should be used. Dot it by replacing the 
        /// </summary>
        public DateHelper DateHelper { get { return _dateHelper ??= new DateHelper(this); } }

        /// <summary>
        /// Current culture
        /// </summary>
        public CultureInfo CultureInfo { get; set; }

        /// <summary>The delimiter that seperates data when rendered to csv</summary>
        public char Delimiter { get; set; } = ',';

        /// <summary>
        /// The list of formats that the date checker verifies agianst. It is posible to add or replace these.
        /// </summary>
        public List<string> DateFormats { get; set; } =
        [
            "yyyy M d",
            "yyyy-M-d",
            "yyyy/M/d",
            "yyyy-M-dTH:m:s",
            "yyyy-M-dTH:m:s'Z'",
            "yyyy-M-dTH:m:s.fffK",
            "yyyy-M-dTH:m:sK",
            "yyyy M d H:m:s",
            "yyyy-M-d H:m:s",
            "yyyy/M/d H:m:s",
        ];

        /// <summary>What format datetimes should have</summary>
        public DateFormatKind DateFormatKind { get; set; }

        /// <summary>The name if header is empty</summary>
        public string HeaderNameIfNull { get; set; } = "col";


        /// <summary>
        /// Like: 'Romance Standard Time'. Get timeZoneId from here: https://raw.githubusercontent.com/Bygdrift/Warehouse/master/Docs/TimeZoneIds.csv
        /// </summary>
        public string TimeZoneId { get; set; }

        /// <summary>
        /// The timeZoneInfo
        /// </summary>
        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                if (!timeZoneInfoIsSet)
                {
                    timeZoneInfoIsSet = true;
                    if (!string.IsNullOrEmpty(TimeZoneId))
                        _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
                }

                return _timeZoneInfo;
            }
            set { _timeZoneInfo = value; }
        }

        /// <summary>
        /// Add another culture. Is by default set to Invariant culture 
        /// </summary>
        /// <param name="cultureName">Like 'da-DK' or 'en-US' or 'en-GB' and so on. If null then invariant culture is used</param>
        public Config AddCulture(string cultureName)
        {
            CultureInfo = string.IsNullOrEmpty(cultureName) ? CultureInfo.InvariantCulture : CultureInfo.CreateSpecificCulture(cultureName);
            return this;
        }

        /// <summary>Add a delimitter used when rendering csv</summary>
        public Config AddDelimiter(char delimiter)
        {
            Delimiter = delimiter;
            return this;
        }

        /// <summary>
        /// Adds comma-separetad formats. You can se the predefined in Config.Formats. They are all going like Year, month, day, hour, minute, second.
        /// </summary>
        /// <param name="formats">Like: "yyyy M d H:m:s, s"</param>
        /// <param name="delimiter">The delimitter that splits up the string. Comma by default</param>
        public Config AddDateFormats(string formats, char delimiter = ',')
        {
            foreach (var item in Csv.SplitString(formats, delimiter))
                DateFormats.Add(item.Value);

            return this;
        }
    }
}