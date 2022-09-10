using System;
using System.Collections.Generic;
using System.Linq;

namespace Bygdrift.Tools.CsvTool
{
    public partial class Csv
    {

        /// <summary>
        /// Creates a shallow copy of the csv that will not be linked to the original
        /// </summary>
        public Csv GetCsvCopy()
        {
            var res = new Csv(Config);
            res.ColMaxLengths = ColMaxLengths;
            res.ColTypes = ColTypes;

            foreach (var item in Headers)
                res.AddHeader(item.Value);

            foreach (var item in Records)
                res.AddRecord(item.Key.Row, item.Key.Col, item.Value);

            return res;
        }

        /// <summary>
        /// Get a specific record
        /// </summary>
        /// <param name="row">rowId</param>
        /// <param name="col">colId</param>
        public object GetRecord(int row, int col)
        {
            Records.TryGetValue((row, col), out object val);
            return val;
        }

        /// <summary>
        /// Get a specific record
        /// </summary>
        /// <param name="row">rowId</param>
        /// <param name="col">colId</param>
        public T GetRecord<T>(int row, int col)
        {
            if (Records.TryGetValue((row, col), out object val))
            {
                if (typeof(T) == typeof(object))  //Less work
                    return (T)val;

                if (RecordToType(val, out T valT))
                    return valT;
            }
            return default;
        }

        /// <summary>
        /// Get a specific record
        /// </summary>
        /// <param name="row">rowId</param>
        /// <param name="col">colId</param>
        public T GetRecord<T>(int row, string headerName)
        {
            var val = GetRecord(row, headerName);
            if (RecordToType(val, out T valT))
                return valT;

            return default;
        }

        /// <summary>
        /// Get a specific record
        /// </summary>
        /// <param name="row">rowId</param>
        /// <param name="headerName">The name of the header</param>
        public object GetRecord(int row, string headerName)
        {
            if (TryGetColId(headerName, out int col))
            {
                Records.TryGetValue((row, col), out object val);
                return val;
            }
            else
                throw new Exception($"The headerName '{headerName}' are not in csv");
        }

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="col">the coloumns index</param>
        /// <param name="includeNullValues">if null values should be returned</param>
        public Dictionary<int, T> GetColRecords<T>(int col, bool includeNullValues = true)
        {
            var res = new Dictionary<int, T>();
            for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
            {
                var val = GetRecord<T>(row, col);
                if (includeNullValues || val != null)
                    res.Add(row, val);
            }

            return res;
        }

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="col">the coloumns index</param>
        /// <param name="includeNullValues">if null values should be returned</param>
        public Dictionary<int, object> GetColRecords(int col, bool includeNullValues = true)
        {
            return GetColRecords<object>(col, includeNullValues);
        }

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="headerName">The name of the header</param>
        /// <param name="includeNullValues">if null values should be returned</param>
        public Dictionary<int, T> GetColRecords<T>(string headerName, bool includeNullValues = true)
        {
            if (TryGetColId(headerName, out int col))
                return GetColRecords<T>(col, includeNullValues);

            return default;
        }

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="headerName">The name of the header</param>
        /// <param name="includeNullValues">if null values should be returned</param>
        public Dictionary<int, object> GetColRecords(string headerName, bool includeNullValues = true)
        {
            return GetColRecords<object>(headerName, includeNullValues);
        }

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="lookupHeader">The name of the header to lookup the value in</param>
        /// <param name="returnHeader">The header column to return. If null, the lookupHeader will be returned</param>
        /// <param name="lookupValue">The value to lookup within the headerName column</param>
        /// <param name="ignoreCase">If case on lookuValus should be ignored when comparing</param>
        /// <param name="includeNullValues"></param>
        public Dictionary<int, object> GetColRecords(string lookupHeader, string returnHeader, object lookupValue, bool ignoreCase, bool includeNullValues = true)
        {
            var res = new Dictionary<int, object>();
            if (TryGetColId(returnHeader, out int returnColumn) || TryGetColId(lookupHeader, out returnColumn))
            {
                var rowRecords = GetRowsRecords(lookupHeader, ignoreCase, includeNullValues);
                foreach (var rowRecord in rowRecords)
                    res.Add(rowRecord.Key, rowRecord.Value[returnColumn]);
            }
            return res;
        }

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="headerName">The name of the header to lookup the value in</param>
        /// <param name="ignoreCase">If case on lookuValus should be ignored when comparing</param>
        /// <param name="includeNullValues"></param>
        public Dictionary<int, T> GetColRecords<T>(string headerName, bool ignoreCase, bool includeNullValues = true)
        {
            var res = new Dictionary<int, T>();
            if (TryGetColId(headerName, out int returnColumn))
            {
                var rowRecords = GetRowsRecords(headerName, ignoreCase, includeNullValues);
                foreach (var rowRecord in rowRecords)
                    if (RecordToType<T>(rowRecord.Value[returnColumn], out T val))
                        res.Add(rowRecord.Key, val);
            }
            return res;
        }

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="col">the coloumns index</param>
        /// <param name="includeNullValues">if null values should be returned</param>
        public IEnumerable<T> GetColRecordsDistinct<T>(int col, bool includeNullValues = true)
        {
            var res = new List<T>();
            for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
            {
                var val = GetRecord<T>(row, col);
                if ((includeNullValues || val != null) && !res.Any(o => o.Equals(val)))
                    res.Add(val);
            }

            return res;
        }

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="col">the coloumns index</param>
        /// <param name="includeNullValues">if null values should be returned</param>
        public IEnumerable<object> GetColRecordsDistinct(int col, bool includeNullValues = true)
        {
            return GetColRecordsDistinct<object>(col, includeNullValues);
        }

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="headerName">The name of the header</param>
        /// <param name="includeNullValues">if null values should be returned</param>
        public IEnumerable<T> GetColRecordsDistinct<T>(string headerName, bool includeNullValues = true)
        {
            if (TryGetColId(headerName, out int col))
                return GetColRecordsDistinct<T>(col, includeNullValues);

            return default;
        }

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="headerName">The name of the header</param>
        /// <param name="includeNullValues">if null values should be returned</param>
        public IEnumerable<object> GetColRecordsDistinct(string headerName, bool includeNullValues = true)
        {
            return GetColRecordsDistinct<object>(headerName, includeNullValues);
        }

        ///// <summary>
        ///// Gets all entities for a specific column and only unique results will be returned
        ///// </summary>
        ///// <param name="col">the coloumns index</param>
        //public IEnumerable<object> GetColRecordsDistinct(int col)
        //{
        //    var res = new List<object>();
        //    for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
        //        if (Records.TryGetValue((row, col), out object val))
        //            res.Add(val);

        //    return res.Distinct();
        //}

        ///// <summary>
        ///// Gets all entities for a specific column and only unique results will be returned
        ///// </summary>
        ///// <param name="headerName">The name of the header to retrun</param>
        //public (int Col, IEnumerable<object> Records) GetColRecordsDistinct(string headerName)
        //{
        //    return TryGetColId(headerName, out int header) ? (header, GetColRecordsDistinct(header)) : default;
        //}


        /// <summary>
        /// All rows that satisfies the expresion, are returned
        /// </summary>
        public Dictionary<int, Dictionary<int, object>> GetColRecords(Func<KeyValuePair<(int Row, int Col), object>, bool> expr, bool includeNullValues = true)
        {
            var res = new Dictionary<int, Dictionary<int, object>>();
            var cols = Records.Where(expr).Select(o => o.Key.Row).Distinct();
            foreach (var col in cols)
            {
                var rowDict = new Dictionary<int, object>();
                for (int row = ColLimit.Min; row <= ColLimit.Max; row++)
                    if (Records.TryGetValue((row, col), out object val))
                        rowDict.Add(row, val);
                    else if (includeNullValues)
                        rowDict.Add(row, default);

                res.Add(col, rowDict);
            }
            return res;
        }

        /// <summary>
        /// Gives all headers in a string like "Id, Data"
        /// </summary>
        public KeyValuePair<int, string> GetHeader(string name)
        {
            return Headers.SingleOrDefault(o => o.Value == name);
        }

        /// <summary>
        /// Gives all headers in a string like "Id, Data"
        /// </summary>
        public string GetHeadersAsString(string delimiter = ", ")
        {
            return string.Join(delimiter, Headers.OrderBy(o => o.Key).Select(o => o.Value));
        }

        /// <summary>
        /// Get multiple row records
        /// </summary>
        /// <param name="row"></param>
        /// <param name="includeNullValues"></param>
        public Dictionary<int, object> GetRowRecords(int row, bool includeNullValues = true)
        {
            var res = new Dictionary<int, object>();

            for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
            {
                var val = GetRecord(row, col);
                if (includeNullValues || (!includeNullValues && val != null))
                    res.Add(col, val);
            }
            return res;
        }

        /// <summary>
        /// Finds the first mathcing row
        /// </summary>
        /// <param name="headerName">Name of the header volumn that should be looked in for the value</param>
        /// <param name="value">The value to lookup in the column with headerName</param>
        /// <param name="includeNullValues"></param>
        public Dictionary<int, object> GetRowRecordsFirstMatch(string headerName, object value, bool includeNullValues = true)
        {
            if (TryGetColId(headerName, out int colHeader))
            {
                var res = new Dictionary<int, object>();
                var records = Records.FirstOrDefault(o => o.Key.Col == colHeader && o.Value?.ToString() == value?.ToString());
                if (records.Value != null)
                {
                    var row = records.Key.Row;
                    for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                        if (Records.TryGetValue((row, col), out object val))
                            res.Add(col, val);
                        else if (includeNullValues)
                            res.Add(col, default);
                }
                return res;
            }
            return null;
        }

        /// <summary>
        /// Get multiple row records
        /// </summary>
        /// <param name="includeNullValues"></param>
        public Dictionary<int, Dictionary<int, object>> GetRowsRecords(bool includeNullValues = true)
        {
            var res = new Dictionary<int, Dictionary<int, object>>();
            for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
            {
                var rowDict = new Dictionary<int, object>();
                for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                    if (Records.TryGetValue((row, col), out object val))
                        rowDict.Add(col, val);
                    else if (includeNullValues)
                        rowDict.Add(col, default);

                res.Add(row, rowDict);
            }
            return res;
        }

        /// <summary>
        /// Get multiple row records
        /// </summary>
        /// <param name="includeNullValues"></param>
        public Dictionary<int, Dictionary<int, object>> GetRowsRecords(string orderedByHeaderName, bool ascending = true, bool includeNullValues = true)
        {
            var res = new Dictionary<(int Row, object OrderedBy), Dictionary<int, object>>();
            if (TryGetHeader(orderedByHeaderName, out int orderedByHeaderCol))
            {
                for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
                {
                    var rowDict = new Dictionary<int, object>();
                    for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                        if (Records.TryGetValue((row, col), out object val))
                            rowDict.Add(col, val);
                        else if (includeNullValues)
                            rowDict.Add(col, default);

                    var orderedByValue = GetRecord(row, orderedByHeaderCol);
                    res.Add((row, orderedByValue), rowDict);
                }
            }

            if (ascending)
                return res.OrderBy(o => o.Key.OrderedBy).ToDictionary(o => o.Key.Row, o => o.Value);
            else
                return res.OrderByDescending(o => o.Key.OrderedBy).ToDictionary(o => o.Key.Row, o => o.Value);
        }

        ///// <summary>
        ///// All rows that satisfies the expresion, are returned
        ///// </summary>
        ///// <returns>rowId and a dictionary with colId and value</returns>
        //public Dictionary<int, Dictionary<int, object>> GetRowsRecords(Func<KeyValuePair<(int Row, int Col), object>, bool> expr, bool includeNullValues = true)
        //{
        //    var res = new Dictionary<int, Dictionary<int, object>>();

        //    foreach (var row in Records.Where(expr).Select(o => o.Key.Row).Distinct())
        //    {
        //        var rowDict = new Dictionary<int, object>();
        //        for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
        //            if (Records.TryGetValue((row, col), out object val))
        //                rowDict.Add(col, val);
        //            else if (includeNullValues)
        //                rowDict.Add(col, default);

        //        res.Add(row, rowDict);
        //    }
        //    return res;
        //}

        ///// <param name="headerName">The name of the header to lookup the value in</param>
        ///// <param name="value">The value to lookup within the headerName column</param>
        ///// <param name="includeNullValues"></param>
        ///// <returns>rowId and a dictionary with colId and value</returns>
        //public Dictionary<int, Dictionary<int, object>> GetRowsRecords(string headerName, object value, bool includeNullValues = true)
        //{
        //    if (TryGetColId(headerName, out int col))
        //        return GetRowsRecords(o => o.Key.Col == col && o.Value?.ToString() == value?.ToString(), includeNullValues);

        //    return default;
        //}

        /// <param name="headerName">The name of the header to lookup the value in</param>
        /// <param name="value">The value to lookup within the headerName column. Comparison is right ignores upper an lowercase</param>
        /// <param name="ignoreCase">If case should be ignored</param>
        /// <returns>rowId and a dictionary with colId and value</returns>
        public Dictionary<int, Dictionary<int, object>> GetRowsRecords(string headerName, bool ignoreCase, object value)
        {
            var res = new Dictionary<int, Dictionary<int, object>>();
            if (TryGetColId(headerName, out int col))
            {
                var records = Records.Where(o => o.Key.Col == col && (value == null ? o.Value == null : o.Value != null && (ignoreCase ? o.Value.ToString().ToUpper().Equals(value.ToString().ToUpper()) : o.Value.Equals(value))));
                var rows = records.Select(o => o.Key.Row).ToList();
                foreach (var row in rows)
                    res.Add(row, GetRowRecords(row));
            }
            return res;
        }

        /// <summary>
        /// Builds a dictionary that contain headernames instead og column indexes
        /// </summary>
        public Dictionary<int, Dictionary<string, object>> GetRowsRecordsWithHeaderNames(bool includeNullValues = true)
        {
            var res = new Dictionary<int, Dictionary<string, object>>();
            for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
            {
                var rowDict = new Dictionary<string, object>();
                for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                    if (Records.TryGetValue((row, col), out object val))
                        rowDict.Add(Headers[col], val);
                    else if (includeNullValues)
                        rowDict.Add(Headers[col], default);

                res.Add(row, rowDict);
            }
            return res;
        }

        /// <summary>
        /// Builds Dictionary
        /// Can only be done with a column that contains unique values
        /// </summary>
        public Dictionary<object, Dictionary<string, object>> GetRowsRecordsWithHeaderNames(string primaryKeyHeader, bool includeNullValues = true)
        {
            var res = new Dictionary<object, Dictionary<string, object>>();
            if (Records == null || Records.Count == 0 || !TryGetColId(primaryKeyHeader, out int colId))
                return default;

            var colRecords = GetColRecords(primaryKeyHeader);

            foreach (var item in colRecords)
            {
                var row = item.Key;
                var rowDict = new Dictionary<string, object>();
                for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                    if (colId != col)
                    {
                        if (TryGetRecord(row, col, out object val))
                            rowDict.Add(Headers[col], val);
                        else if (includeNullValues)
                            rowDict.Add(Headers[col], default);
                    }

                res.Add(item.Value, rowDict);
            }
            return res;
        }

        /// <summary>
        /// Gives all headers in a string like "Id, Data"
        /// </summary>
        public bool TryGetHeader(string name, out int headerCol)
        {
            headerCol = Headers.SingleOrDefault(o => o.Value == name).Key;
            return !string.IsNullOrEmpty(name) && Headers.ContainsValue(name);
        }

        /// <summary>
        /// Get a columns index
        /// </summary>
        /// <param name="headerName">The name of the column</param>
        /// <param name="col"></param>
        /// <returns>The headers indx</returns>
        public bool TryGetColId(string headerName, out int col)
        {
            if (string.IsNullOrEmpty(headerName))
            {
                col = default;
                return false;
            }
            var header = Headers.SingleOrDefault(o => o.Value.ToString().Equals(headerName));
            col = header.Key;
            return header.Value != null && header.Value.Equals(headerName);
        }

        /// <summary>
        /// Try get the id for the specified headername.
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="value"></param>
        /// <param name="caseSensitive"></param>
        public bool TryGetColId(string headerName, out int value, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(headerName))
            {
                value = 0;
                return false;
            }

            var res = Headers.SingleOrDefault(o => caseSensitive ? o.Value.Equals(headerName) : o.Value.ToString().ToUpper().Equals(headerName.ToString().ToUpper()));
            value = res.Key;
            return res.Value != null;
        }

        /// <summary>
        /// Try to get a record an return if it succeded or not
        /// </summary>
        /// <param name="row">rowId</param>
        /// <param name="col">colId</param>
        /// <param name="value">The value will be available from here</param>
        public bool TryGetRecord(int row, int col, out object value)
        {
            return Records.TryGetValue((row, col), out value);
        }

        /// <summary>
        /// Try to get the col ID for the first header that mathes
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="row">rowId</param>
        /// <param name="includeNullValues"></param>
        /// <param name="value">The value will be available from here</param>
        /// <param name="ignoreCase"></param>
        public bool TryGetRowFirstId(string headerName, object value, bool ignoreCase, out int row, bool includeNullValues = true)
        {
            var colValues = GetColRecords(headerName, includeNullValues);
            if (colValues != default)
            {
                var res = ignoreCase ? colValues.FirstOrDefault(o => o.Value?.ToString().ToUpper() == value?.ToString().ToUpper()) : colValues.FirstOrDefault(o => o.Value?.ToString() == value?.ToString());
                if (res.Value != null)
                {
                    row = res.Key;
                    return true;
                }
            }
            row = 0;
            return false;
        }

        /// <param name="headerName"></param>
        /// <param name="omitEachFirstRedundantValue">If false, al redundant vals are returned. If true: For each set of redundant values, the first value will not be returned.</param>
        /// <param name="ignoreCase"></param>
        /// <returns>RowIds or null if none</returns>
        public int[] GetRowsRecordsRedundants(string headerName, bool omitEachFirstRedundantValue, bool ignoreCase)
        {
            var rowIds = new List<int>();
            var colRecords = GetColRecords(headerName);

            if (colRecords == null)
                return null;

            var duplicates = ignoreCase ? colRecords.GroupBy(o => o.Value?.ToString().ToUpper()).Where(o => o.Count() > 1) : colRecords.GroupBy(o => o.Value).Where(o => o.Count() > 1);
            if (!duplicates.Any())
                return null;

            foreach (var item in duplicates.GroupBy(o => o))
            {
                bool first = omitEachFirstRedundantValue;
                foreach (var sub in item.Key)
                {
                    if (!first)
                        rowIds.Add(sub.Key);

                    first = false;
                }
            }

            return rowIds.ToArray();
        }



    }
}
