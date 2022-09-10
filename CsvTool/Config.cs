using System.Collections.Generic;
using System.Globalization;

namespace Bygdrift.Tools.CsvTool
{
    /// <summary>
    /// Configuration of how to read/write csv
    /// </summary>
    public class Config
    {
        private bool _dateTimeIsUniversal;

        /// <param name="cultureName">Like 'da-DK' or 'en-US' or 'en-GB' and so on. If null then invariant culture is used</param>
        public Config(string cultureName = null, string dateTimeOutput = "s", bool dateTimeIsUniversal = false)
        {
            CultureInfo = cultureName != null ? CultureInfo.CreateSpecificCulture(cultureName) : CultureInfo.InvariantCulture;
            DateTimeOutput = dateTimeOutput;
            DateTimeIsUniversal = dateTimeIsUniversal;
        }

        public string DateTimeOutput { get; set; }

        /// <summary>
        /// If true: datetime can be returned as "2022-08-17 06:56:03Z" If false (default): "2022-08-17T06:57:03"
        /// </summary>
        public bool DateTimeIsUniversal
        {
            get { return _dateTimeIsUniversal; }
            set
            {
                DateTimeOutput = value ? "u" : "s";
                _dateTimeIsUniversal = value;
            }
        }


        /// <summary>
        /// Current culture
        /// </summary>
        public CultureInfo CultureInfo { get; set; }

        /// <summary>
        /// The list of formats that the date checker verifies agianst. It is posible to add or replace these.
        /// </summary>
        public List<string> DateFormats = new()
        {
            "yyyy M d",
            "yyyy-M-d",
            "yyyy/M/d",
            "yyyy-M-dTH:m:s",
            "yyyy-M-dTH:m:s.fffzzz",
            "yyyy M d H:m:s",
            "yyyy-M-d H:m:s",
            "yyyy/M/d H:m:s",
            "d M yyyy",
            "d-M-yyyy",
            "d/M/yyyy",
            "d M yyyy H:m:s",
            "d-M-yyyy H:m:s",
            "d/M/yyyy H:m:s",
        };

        ///// <summary>
        ///// Add another culture. Is by default set to Invariant culture 
        ///// </summary>
        ///// <param name="cultureName">Like 'da-DK' or 'en-US' or 'en-GB' and so on. If null then invariant culture is used</param>
        //public void Culture(string cultureName = null)
        //{
        //    CultureInfo = string.IsNullOrEmpty(cultureName) ? CultureInfo.InvariantCulture : CultureInfo.CreateSpecificCulture(cultureName);
        //}
    }
}
