using Bygdrift.Tools.CsvTool.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Bygdrift.Tools.CsvTool
{
    /// <summary>
    /// Build a csv class
    /// </summary>
    public partial class Csv
    {
        
        /// <summary>
        /// Current culture
        /// </summary>
        private CultureInfo cultureInfo;

        /// <summary>
        /// Create a new csv class
        /// </summary>
        public Csv()
        {
            Culture();
        }

        /// <summary>
        /// Create a new csv class
        /// </summary>
        /// <param name="headers">Comma seperated string with headers. There can be space between</param>
        public Csv(string headers)
        {
            AddHeaders(headers);
            Culture();
        }

        /// <summary>
        /// A class that verifies if a value is a date. It is possible to replace the predefined date formats, if another culture than the predefined should be used. Dot it by replacing the 
        /// </summary>
        public DateChecker DateChecker { get; internal set; } = new();

        /// <summary>
        /// Add another culture. Is by default set to Invariant culture 
        /// </summary>
        /// <param name="cultureName">Like 'da-DK' or 'en-US' or 'en-GB' and so on. If null then invariant culture is used</param>
        public Csv Culture(string cultureName = null)
        {
            cultureInfo = string.IsNullOrEmpty(cultureName) ? CultureInfo.InvariantCulture : CultureInfo.CreateSpecificCulture(cultureName);
            return this;
        }

        /// <summary>
        /// All headers in current CSV, represented by column id and name
        /// </summary>
        public Dictionary<int, string> Headers { get; internal set; } = new();

        /// <summary>
        /// All column types in current CSV, represented by column id and Type
        /// </summary>
        public Dictionary<int, Type> ColTypes { get; internal set; } = new();

        /// <summary>
        /// Gives the length of the record with the logest content in a given column
        /// </summary>
        public Dictionary<int, int> ColMaxLengths { get; internal set; } = new();

        /// <summary>
        /// Each value in the csv
        /// </summary>
        public Dictionary<(int Row, int Col), object> Records { get; internal set; } = new();

        internal (int? Min, int? Max) _colLimit = (null, null);
        internal (int? Min, int? Max) _rowLimit = (null, null);

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


        /// <summary>
        /// If there are two or more headers with the same name, they wil get a suffix of _n so thre times Id will become Id, Id_2, Id_3
        /// </summary>
        internal string UniqueHeader(Dictionary<int, string> headers, string value, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(value))
                value = "col";

            var res = value;
            if (headers.Any(o => o.Value.Equals(value, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture)))
            {
                var i = 2;
                while (headers.Any(o => o.Value.Equals(res, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture)))
                    res = value + "_" + i++;
            }
            return res;
        }

        /// <summary>
        /// If you have an idea of the format already, then this will test if it's correct
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value">An object</param>
        public (bool IsTrue, int Length) IsType(Type type, ref object value)
        {
            var valueAsString = Convert.ToString(value, cultureInfo.NumberFormat);
            if (string.IsNullOrEmpty(valueAsString))
            {
                value = null;
                return (true, 0);
            }

            var valueLength = valueAsString != null ? valueAsString.Length : 0;

            if (type == typeof(string))
            {
                return (true, valueLength);
            }
            if (type == typeof(int) && int.TryParse(valueAsString, NumberStyles.Integer, cultureInfo.NumberFormat, out int intVal))
            {
                value = intVal;
                return (true, valueLength);
            }
            if (type == typeof(long) && long.TryParse(valueAsString, NumberStyles.Integer, cultureInfo.NumberFormat, out long longVal))
            {
                value = longVal;
                return (true, valueLength);
            }
            if (type == typeof(decimal) && decimal.TryParse(valueAsString, NumberStyles.Any, cultureInfo.NumberFormat, out decimal decVal))
            {
                value = decVal;
                return (true, valueLength);
            }
            if (type == typeof(bool) && bool.TryParse(valueAsString, out bool boolVal))
            {
                value = boolVal;
                return (true, valueLength);
            }
            if (type == typeof(DateTime) && DateChecker.TryParse(valueAsString, out DateTime dateValue))
            {
                value = dateValue;
                return (true, 19);  //Length 19 == DateTime.Now.ToString("s").Length;
            }
            return (false, valueLength);
        }

        /// <summary>
        /// The type and length of a value
        /// </summary>
        /// <param name="value">An object</param>
        public (Type Type, int Length) GetType(ref object value)
        {
            var valueAsString = Convert.ToString(value, cultureInfo.NumberFormat);
            if (string.IsNullOrEmpty(valueAsString))
            {
                value = null;
                return (null, 0);
            }

            var valueLength = valueAsString.Length;
            var type = value.GetType();

            if (type == typeof(string) && string.IsNullOrEmpty((string)value))
            {
                return (null, 0);
            }
            if (type != typeof(string))
            {
                var (IsTrue, Length) =  IsType( type, ref value);
                if (IsTrue)
                    return (type, Length);
            }
            if (int.TryParse(valueAsString, NumberStyles.Integer, cultureInfo.NumberFormat, out int intVal))
            {
                value = intVal;
                return (typeof(int), valueLength);
            }
            if (long.TryParse(valueAsString, NumberStyles.Integer, cultureInfo.NumberFormat, out long longVal))
            {
                value = longVal;
                return (typeof(long), valueLength);
            }
            if (decimal.TryParse(valueAsString, NumberStyles.Any, cultureInfo.NumberFormat, out decimal decVal))
            {
                value = decVal;
                return (typeof(decimal), valueLength);
            }
            if (bool.TryParse(valueAsString, out bool boolVal))
            {
                value = boolVal;
                return (typeof(bool), valueLength);
            }
            if (DateChecker.TryParse(valueAsString, out DateTime dateValue))
            {
                value = dateValue;
                return (typeof(DateTime), 19);  //Length 19 == DateTime.Now.ToString("s").Length;
            }
            return (typeof(string), valueLength);
        }

        /// <summary>
        /// Try cast a record to a specific type
        /// </summary>
        /// <typeparam name="T">The type to cast to</typeparam>
        /// <param name="value">an object</param>
        /// <param name="result">the casted value</param>
        /// <returns>If the casting succeeded</returns>
        public bool RecordToType<T>(object value, out T result)
        {
            if (RecordToType(typeof(T), value, out object subResult))
            {
                result = (T)subResult;
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Try cast a record to a specific type
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="value">an object</param>
        /// <param name="result">the casted value</param>
        /// <returns>If the casting succeeded</returns>
        public bool RecordToType(Type type, object value, out object result)
        {
            result = default;
            var valueAsString = Convert.ToString(value, cultureInfo.NumberFormat);

            if (type == typeof(string))
            {
                result = Convert.ChangeType(valueAsString, type);
                return true;
            }

            if (type == typeof(int))
                if (int.TryParse(valueAsString, NumberStyles.Integer, cultureInfo.NumberFormat, out int intVal))
                {
                    result = Convert.ChangeType(intVal, type);
                    return true;
                }
                else
                    return false;

            if (type == typeof(long))
                if (long.TryParse(valueAsString, NumberStyles.Integer, cultureInfo.NumberFormat, out long longVal))
                {
                    result = Convert.ChangeType(longVal, type);
                    return true;
                }
                else
                    return false;

            if (type == typeof(decimal))
                if (decimal.TryParse(valueAsString, NumberStyles.Any, cultureInfo.NumberFormat, out decimal decVal))
                {
                    result = Convert.ChangeType(decVal, type);
                    return true;
                }
                else
                    return false;

            if (type == typeof(bool))
                if (bool.TryParse(valueAsString, out bool boolVal))
                {
                    result = Convert.ChangeType(boolVal, type);
                    return true;
                }
                else
                    return false;

            if (type == typeof(DateTime))
                if (DateChecker.TryParse(valueAsString, out DateTime dateValue))
                {
                    result = Convert.ChangeType(dateValue, type);
                    return true;
                }
                else
                    return false;

            try
            {
                result = Convert.ChangeType(valueAsString, type);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SetAndVerifyColTypeAndLength(int col, ref object value)
        {
            Type valType = null;
            int length = 0;
            bool isTrue = false;
            if (ColTypes.ContainsKey(col) && ColTypes[col] != null)  //The type has already been dectectet earlier, so look up if it's the same type again
            {
                valType = ColTypes[col];
                (isTrue, length) = IsType(valType, ref value);
            }
            if(!isTrue)  //It was not the same type, so use some more time to detect the actual type
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
            //if (oldType == typeof(long) &&  -- string
            //if (oldType == typeof(decimal) && --string
            //if (oldType == typeof(bool) && --string
            //if (oldType == typeof(DateTime) --string
            return typeof(string);
        }
    }
}
