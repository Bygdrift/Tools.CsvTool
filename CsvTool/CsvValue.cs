using System;
using System.Globalization;

namespace Bygdrift.Tools.CsvTool
{
    public partial class Csv
    {
        /// <summary>
        /// Returns if values in current column is model.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public bool RecordIsModel(int col)
        {
            ColTypes.TryGetValue(col, out Type type);
            return type != null && type != typeof(string) && type.IsClass;
        }

        /// <summary>
        /// The type and length of a value
        /// </summary>
        /// <param name="value">An object</param>
        public (Type Type, int Length) GetType(ref object value)
        {
            var valueAsString = Convert.ToString(value, Config.CultureInfo.NumberFormat);
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
                var (IsTrue, Length) = IsType(ref type, ref value);
                if (IsTrue)
                    return (type, Length);
            }
            if (int.TryParse(valueAsString, NumberStyles.Integer, Config.CultureInfo.NumberFormat, out int intVal))
            {
                value = intVal;
                return (typeof(int), valueLength);
            }
            if (long.TryParse(valueAsString, NumberStyles.Integer, Config.CultureInfo.NumberFormat, out long longVal))
            {
                value = longVal;
                return (typeof(long), valueLength);
            }
            //I am not using double or float because 0.0000001129498786607691 becomes 1,12949878660769E-07
            if (double.TryParse(valueAsString, NumberStyles.Any, Config.CultureInfo.NumberFormat, out double doubleVal))
            {
                value = doubleVal;
                return (typeof(double), valueLength);
            }
            if (decimal.TryParse(valueAsString, NumberStyles.Any, Config.CultureInfo.NumberFormat, out decimal decVal))
            {
                value = decVal;
                return (typeof(decimal), valueLength);
            }
            if (bool.TryParse(valueAsString, out bool boolVal))
            {
                value = boolVal;
                return (typeof(bool), valueLength);
            }
            if (Config.DateHelper.TryParseDateTimeOffset(value, out DateTimeOffset dateOffsetValue))
            {
                value = dateOffsetValue;
                return (typeof(DateTime), 25);
            }
            if (Config.DateHelper.TryParseDateTime(value, out DateTime dateValue))
            {
                if (Config.DateFormatKind == DateFormatKind.TimeOffsetUTC || Config.DateFormatKind == DateFormatKind.TimeOffsetDST)
                {
                    value = Config.DateHelper.ToDateTimeOffset(dateValue);
                    return (typeof(DateTimeOffset), Config.DateHelper.DateTimeLength);
                }

                value = dateValue;
                return (typeof(DateTime), Config.DateHelper.DateTimeLength);
            }

            return (type, valueLength);
            //return (typeof(string), valueLength);
        }

        /// <summary>
        /// If you have an idea of the format already, then this will test if it's correct
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value">An object</param>
        public (bool IsTrue, int Length) IsType(ref Type type, ref object value)
        {
            var valueAsString = Convert.ToString(value, Config.CultureInfo.NumberFormat);
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
            if (type == typeof(int) && int.TryParse(valueAsString, NumberStyles.Integer, Config.CultureInfo.NumberFormat, out int intVal))
            {
                value = intVal;
                return (true, valueLength);
            }
            if (type == typeof(long) && long.TryParse(valueAsString, NumberStyles.Integer, Config.CultureInfo.NumberFormat, out long longVal))
            {
                value = longVal;
                return (true, valueLength);
            }
            //I am not using double or float because 0.0000001129498786607691 becomes 1,12949878660769E-07
            if (type == typeof(double) && double.TryParse(valueAsString, NumberStyles.Any, Config.CultureInfo.NumberFormat, out double doubleVal))
            {
                value = doubleVal;
                return (true, valueLength);
            }
            if (type == typeof(decimal) && decimal.TryParse(valueAsString, NumberStyles.Any, Config.CultureInfo.NumberFormat, out decimal decVal))
            {
                value = decVal;
                return (true, valueLength);
            }
            if (type == typeof(bool) && bool.TryParse(valueAsString, out bool boolVal))
            {
                value = boolVal;
                return (true, valueLength);
            }
            if (type == typeof(DateTimeOffset) && Config.DateHelper.TryParseDateTimeOffset(value, out DateTimeOffset dateOffsetValue))
            {
                value = dateOffsetValue;
                return (true, Config.DateHelper.DateTimeLength);
            }
            if (type == typeof(DateTime) && (Config.DateFormatKind == DateFormatKind.TimeOffsetUTC || Config.DateFormatKind == DateFormatKind.TimeOffsetDST) && Config.DateHelper.TryParseDateTime(value, out DateTime dateValue))
            {
                //if (Config.TimeZoneInfo == null)
                //    value = dateValue;
                //else
                //{
                value = Config.DateHelper.ToDateTimeOffset(dateValue);
                type = typeof(DateTimeOffset);
                //}

                return (true, Config.DateHelper.DateTimeLength);
            }
            if (type == typeof(DateTime) && Config.DateHelper.TryParseDateTime(value, out DateTime dateValue2))
            {
                value = dateValue2;
                return (true, Config.DateHelper.DateTimeLength);
            }
            return (false, valueLength);
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
            if (value == null)
            {
                result = default;
                return true;
            }

            var valueType = value.GetType();
            try
            {
                if (type == valueType)
                {
                    result = value;
                    return true;
                }
                if (type == typeof(string))
                {
                    result = value.ToString();
                    return true;
                }
                if (type == typeof(DateTimeOffset) && Config.DateHelper.TryParseDateTimeOffset(value, out DateTimeOffset dateOffsetValue))
                {
                    result = dateOffsetValue;
                    return true;
                }
                if (type == typeof(DateTime) && valueType == typeof(DateTimeOffset) && Config.DateHelper.TryParseDateTimeOffset(value, out DateTimeOffset dateOffsetValue2))
                {
                    result = Config.DateHelper.ToDateTime(dateOffsetValue2);
                    return true;
                }
                if (type == typeof(DateTime) && Config.DateHelper.TryParseDateTime(value, out DateTime dateValue))
                {
                    result = dateValue;
                    return true;
                }
                result = Convert.ChangeType(value, type, Config.CultureInfo);
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
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
            if (value == null)
            {
                result = default;
                return true;
            }

            if (RecordToType(typeof(T), value, out object subResult))
            {
                result = (T)subResult;
                return true;
            }
            result = default;
            return false;
        }
    }
}
