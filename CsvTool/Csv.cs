using System;
using System.Collections.Generic;
using System.Linq;

namespace Bygdrift.Tools.CsvTool
{

    /// <summary>
    /// Build a csv class
    /// </summary>
    public partial class Csv
    {
        //private DateHelper _dateHelper;
        internal (int? Min, int? Max) _colLimit = (null, null);
        internal (int? Min, int? Max) _rowLimit = (null, null);

        /// <summary>
        /// Create a new csv class
        /// </summary>
        public Csv()
        {
            Config = new Config();
        }

        /// <summary>
        /// Create a new csv class
        /// </summary>
        public Csv(Config config = null)
        {
            Config = config ?? new Config();
        }

        /// <summary>
        /// Create a new csv class
        /// </summary>
        /// <param name="headers">Comma seperated string with headers. There can be space between</param>
        public Csv(string headers)
        {
            AddHeaders(headers);
            Config = new Config();
        }

        /// <summary>
        /// Create a new csv class
        /// </summary>
        /// <param name="headers">An array  with headers</param>
        public Csv(string[] headers)
        {
            AddHeaders(headers);
            Config = new Config();
        }

        /// <summary>
        /// Create a new csv class
        /// </summary>
        /// <param name="config">A config to decribe special loadings like delimiter and handling date and time</param>
        /// <param name="headers">Comma seperated string with headers. There can be space between</param>
        public Csv(Config config, string headers)
        {
            Config = config ?? new Config();
            AddHeaders(headers);
        }

        /// <summary>
        /// Create a new csv class
        /// </summary>
        /// <param name="config">A config to decribe special loadings like delimiter and handling date and time</param>
        /// <param name="headers">An array  with headers</param>
        public Csv(Config config, string[] headers)
        {
            Config = config ?? new Config();
            AddHeaders(headers);
        }

        ///// <summary>
        ///// Create a new csv class
        ///// </summary>
        ///// <param name="headers">A dictionary with headers.</param>
        //public Csv(Dictionary<int, string> headers)
        //{
        //    AddHeaders(headers);
        //    Culture();
        //}

        ///// <summary>
        ///// A class that verifies if a value is a date. It is possible to replace the predefined date formats, if another culture than the predefined should be used. Dot it by replacing the 
        ///// </summary>
        //internal DateHelper DateHelper { get { return _dateHelper ??= new DateHelper(Config); } }

        /// <summary>
        /// All headers in current CSV, represented by column id and name
        /// </summary>
        public Dictionary<int, string> Headers { get; set; } = [];

        /// <summary>
        /// All column types in current CSV, represented by column id and Type
        /// </summary>
        public Dictionary<int, Type> ColTypes { get; set; } = [];

        /// <summary>
        /// Gives the length of the record with the logest content in a given column
        /// </summary>
        public Dictionary<int, int> ColMaxLengths { get; set; } = [];

        /// <summary>
        /// Each value in the csv
        /// </summary>
        public Dictionary<(int Row, int Col), object> Records { get; set; } = [];

        /// <summary>
        /// The min and max of columns. If min=0 and max=0 there can either be zero or one col and it can be seen at ColCount
        /// </summary>
        public (int Min, int Max) ColLimit
        {
            get { return (_colLimit.Min ?? 0, _colLimit.Max ?? 0); }
        }

        /// <summary>
        /// The min and max of rows. If min=0 and max=0 there can either be zero or one row and it can be seen at RowCount
        /// </summary>
        public (int Min, int Max) RowLimit
        {
            get { return (_rowLimit.Min ?? 0, _rowLimit.Max ?? 0); }
        }

        /// <summary>
        /// The count of columns
        /// </summary>
        public int ColCount
        {
            get { return _colLimit.Min == null || _colLimit.Max == null ? 0 : ColLimit.Max - ColLimit.Min + 1; }
        }

        /// <summary>
        /// The count of rows
        /// </summary>
        public int RowCount
        {
            get { return _rowLimit.Min == null || _rowLimit.Max == null ? 0 : RowLimit.Max - RowLimit.Min + 1; }
        }

        /// <summary>Configuration of how to read/write csv</summary>
        public Config Config { get; set; }

        /// <summary>
        /// If there are two or more headers with the same name, they wil get a suffix of _n so thre times Id will become Id, Id_2, Id_3
        /// </summary>
        internal string UniqueHeader(string value, bool ignoreCase = false)
        {
            return UniqueHeader(Headers, value, ignoreCase);
        }

        /// <summary>
        /// If there are two or more headers with the same name, they wil get a suffix of _n so thre times Id will become Id, Id_2, Id_3
        /// </summary>
        internal string UniqueHeader(Dictionary<int, string> headers, string value, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(value))
                value = Config.HeaderNameIfNull;

            var res = value;
            if (headers.Any(o => o.Value.Equals(value, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture)))
            {
                var i = 2;
                while (headers.Any(o => o.Value.Equals(res, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture)))
                    res = value + "_" + i++;
            }
            return res;
        }

        private void SetAndVerifyColTypeAndLength(int col, ref object value)
        {
            Type valType = null;
            int length = 0;
            bool isTrue = false;
            if (ColTypes.ContainsKey(col) && ColTypes[col] != null)  //The type has already been dectectet earlier, so look up if it's the same type again
            {
                valType = ColTypes[col];
                (isTrue, length) = IsType(ref valType, ref value);
            }
            if (!isTrue)  //It was not the same type, so use some more time to detect the actual type
                (valType, length) = GetType(ref value);

            if (!Headers.ContainsKey(col))
            {
                this.AddHeader(col, null);
                //this.UniqueHeadersIgnoreCase(true);
            }

            if (ColMaxLengths != null && ColMaxLengths.ContainsKey(col))
            {
                if (ColMaxLengths[col] < length)
                    ColMaxLengths[col] = length;
            }
            else
                ColMaxLengths.Add(col, length);

            if (ColTypes != null && ColTypes.ContainsKey(col))
                ColTypes[col] = CardinalType(ColTypes[col], valType);
            else
                ColTypes.Add(col, valType);
        }

        private static Type CardinalType(Type oldType, Type newType)
        {
            if (oldType == null || oldType == newType) return newType;
            if (oldType != null && newType == null) return oldType;
            //if (newType != null && newType == null) return oldType;
            if (oldType == typeof(string) || newType == typeof(string)) return typeof(string);
            if (oldType == typeof(int) && newType == typeof(long)) return newType;
            if (oldType == typeof(DateTime) && newType == typeof(DateTimeOffset)) return newType;
            if (oldType == typeof(DateTimeOffset) && newType == typeof(DateTime)) return newType;
            //if (oldType == typeof(long) &&  -- string
            //if (oldType == typeof(decimal) && --string
            //if (oldType == typeof(bool) && --string
            //if (oldType == typeof(DateTime) --string
            return typeof(string);
        }
    }
}
