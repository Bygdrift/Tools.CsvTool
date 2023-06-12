using DocumentFormat.OpenXml.Vml;
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
        /// <param name="headerName">The name of the header</param>
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

        /// <summary>
        /// All rows that satisfies the expresion, are returned
        /// </summary>
        /// <returns>Dictionary&lt;ColNumber, Dictionary&lt;RowNumber, T&gt;&gt;</returns>
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

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="col">the coloumns index</param>
        /// <param name="includeNullValues">if null values should be returned</param>
        /// <returns>Dictionary&lt;ColNumber, T&gt;</returns>
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
        /// <returns>Dictionary&lt;ColNumber, object&gt;</returns>
        public Dictionary<int, object> GetColRecords(int col, bool includeNullValues = true)
        {
            return GetColRecords<object>(col, includeNullValues);
        }

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="headerName">The name of the header</param>
        /// <param name="includeNullValues">if null values should be returned</param>
        /// <returns>Dictionary&lt;ColNumber, T&gt;</returns>
        public Dictionary<int, T> GetColRecords<T>(string headerName, bool includeNullValues = true)
        {
            if (TryGetColId(headerName, out int col))
                return GetColRecords<T>(col, includeNullValues);

            return default;
        }

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="headerName">The name of the header</param>
        /// <param name="includeNullValues">if null values should be returned</param>
        /// <returns>Dictionary&lt;ColNumber, object&gt;</returns>
        public Dictionary<int, object> GetColRecords(string headerName, bool includeNullValues = true)
        {
            return GetColRecords<object>(headerName, includeNullValues);
        }

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="lookupHeader">The name of the header to lookup the value in</param>
        /// <param name="returnHeader">The header column to return. If null, the lookupHeader will be returned</param>
        /// <param name="lookupValue">The value to lookup within the headerName column</param>
        /// <param name="caseSensitive">If case on lookuValus should be ignored when comparing</param>
        /// <param name="includeNullValues"></param>
        /// <returns>Dictionary&lt;ColNumber, object&gt;</returns>
        public Dictionary<int, object> GetColRecords(string lookupHeader, string returnHeader, object lookupValue, bool includeNullValues = true, bool caseSensitive = true)
        {
            var res = new Dictionary<int, object>();
            if (TryGetColId(returnHeader, out int returnColumn))
            {
                var rowRecords = GetRowRecordsMatch(lookupHeader, lookupValue, includeNullValues, caseSensitive);
                foreach (var rowRecord in rowRecords)
                    res.Add(rowRecord.Key, rowRecord.Value[returnColumn]);
            }
            return res;
        }

        ///// <summary>Gets all entities for a specific column</summary>
        ///// <param name="headerName">The name of the header to lookup the value in</param>
        ///// <param name="includeNullValues"></param>
        ///// <returns>Dictionary&lt;ColNumber, T&gt;</returns>
        //public Dictionary<int, T> GetColRecords<T>(string headerName, bool includeNullValues = true)
        //{
        //    var res = new Dictionary<int, T>();
        //    if (TryGetColId(headerName, out int returnColumn))
        //    {
        //        var rowRecords = GetRowsRecords(headerName, true, includeNullValues);
        //        foreach (var rowRecord in rowRecords)
        //            if (RecordToType(rowRecord.Value[returnColumn], out T val))
        //                res.Add(rowRecord.Key, val);
        //    }
        //    return res;
        //}

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

        /// <summary>Gets all entities for a specific column</summary>
        /// <param name="headerName">The name of the header</param>
        /// <param name="headerNameGroup">The header to group against</param>
        /// <param name="includeNullValues">if null values should be returned</param>
        /// <param name="caseSensetive"></param>
        /// <returns>Dictionary&lt;GroupValue, Dictionary&lt;RowNumber, T&gt;&gt;</returns>
        public Dictionary<object, Dictionary<int, T>> GetColRecordsGrouped<T>(string headerNameGroup, string headerName, bool includeNullValues = true, bool caseSensetive = true)
        {
            var colNumber = GetHeader(headerName);
            var res = new Dictionary<object, Dictionary<int, T>>();
            var groupColRecords = GetColRecordsDistinct(headerNameGroup, includeNullValues);
            foreach (object groupRecord in groupColRecords)
            {
                var rowRecords = GetRowRecordsMatch(headerNameGroup, groupRecord, includeNullValues, caseSensetive);
                if (rowRecords != null)
                {
                    var subRes = new Dictionary<int, T>();
                    foreach (var r in rowRecords.Keys)
                        if (rowRecords[r].TryGetValue(colNumber, out object val) && RecordToType<T>(val, out T resultVal))  //TODO: Denne kan være pænere
                            subRes.Add(r, resultVal);

                    res.Add(groupRecord, subRes);
                }
            }
            return res;
        }

        /// <summary>
        /// Gives a header
        /// </summary>
        /// <returns>-1 if none found</returns>
        public int GetHeader(string headerName)
        {
            return Headers.ContainsValue(headerName) ? Headers.Single(o => o.Value == headerName).Key : -1;
        }

        /// <summary>
        /// Gives a header
        /// </summary>
        public string GetHeader(int col)
        {
            if (Headers.TryGetValue(col, out string value))
                return value;

            return string.Empty;
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
        /// <param name="caseSensitive">If the search should be case sensetive</param>
        public Dictionary<int, object> GetRowRecordsFirstMatch(string headerName, object value, bool includeNullValues = true, bool caseSensitive = true)
        {
            if (TryGetColId(headerName, out int colHeader))
            {
                var res = new Dictionary<int, object>();
                var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                var records = Records.FirstOrDefault(o => o.Key.Col == colHeader && o.Value.ToString().Equals(value.ToString(), comparison));
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
        /// Finds all mathcing rows
        /// </summary>
        /// <param name="headerName">Name of the header volumn that should be looked in for the value</param>
        /// <param name="value">The value to lookup in the column with headerName</param>
        /// <param name="includeNullValues"></param>
        /// <param name="caseSensitive">If the search should be case sensetive</param>
        /// <returns>Dictionary&lt;RowNumber, Dictionary&lt;ColNumber, object&gt;&gt;</returns>
        public Dictionary<int, Dictionary<int, object>> GetRowRecordsMatch(string headerName, object value, bool includeNullValues = true, bool caseSensitive = true)
        {
            var res = new Dictionary<int, Dictionary<int, object>>();
            if (TryGetColId(headerName, out int colHeader))
            {
                var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                var rows = Records.Where(o => o.Key.Col == colHeader && o.Value.ToString().Equals(value.ToString(), comparison))?.Select(o => o.Key.Row);
                if (rows.Any())
                {
                    foreach (var row in rows)
                    {
                        var subRes = new Dictionary<int, object>();
                        for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                        {
                            var val = GetRecord(row, col);
                            if (val != null)
                                subRes.Add(col, val);
                            else if (includeNullValues)
                                subRes.Add(col, default);
                        }
                        res.Add(row, subRes);
                    }
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
        /// <param name="orderedByHeaderName">The name of the header that result should be ordered by</param>
        /// <param name="ascending"></param>
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

        /// <param name="headerName">The name of the header to lookup the value in</param>
        /// <param name="value">The value to lookup within the headerName column. Comparison is right ignores upper an lowercase</param>
        /// <param name="caseSensetive">If case should be ignored</param>
        /// <returns>rowId and a dictionary with colId and value</returns>
        public Dictionary<int, Dictionary<int, object>> GetRowsRecords(string headerName, bool caseSensetive, object value)
        {
            var res = new Dictionary<int, Dictionary<int, object>>();
            if (TryGetColId(headerName, out int col))
            {
                var records = Records.Where(o => o.Key.Col == col && (value == null ? o.Value == null : o.Value != null && (caseSensetive ? o.Value.Equals(value) : o.Value.ToString().ToUpper().Equals(value.ToString().ToUpper()))));
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
        /// Try to get a record an return if it succeded or not
        /// </summary>
        /// <param name="row">rowId</param>
        /// <param name="headerName">The name of the header</param>
        /// <param name="value">The value will be available from here</param>
        public bool TryGetRecord(int row, string headerName, out object value)
        {
            if (TryGetColId(headerName, out int col))
                return Records.TryGetValue((row, col), out value);
            else
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Try to get a record an return if it succeded or not
        /// </summary>
        /// <param name="row">rowId</param>
        /// <param name="col">colId</param>
        /// <param name="value">The value will be available from here</param>
        public bool TryGetRecord<T>(int row, int col, out T value)
        {
            if (Records.TryGetValue((row, col), out object val))
                if (RecordToType(val, out value))
                    return true;

            value = default;
            return false;
        }

        /// <summary>
        /// Try to get a record an return if it succeded or not
        /// </summary>
        /// <param name="row">rowId</param>
        /// <param name="headerName">The name of the header</param>
        /// <param name="value">The value will be available from here</param>
        public bool TryGetRecord<T>(int row, string headerName, out T value)
        {
            if(TryGetRecord(row, headerName, out object val))
                if (RecordToType(val, out value))
                    return true;

            value = default;
            return false;
        }

        /// <summary>
        /// Try to get the col ID for the first header that mathes
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="row">rowId</param>
        /// <param name="includeNullValues"></param>
        /// <param name="value">The value will be available from here</param>
        /// <param name="caseSensetive"></param>
        public bool TryGetRowFirstId(string headerName, object value, bool caseSensetive, out int row, bool includeNullValues = true)
        {
            var colValues = GetColRecords(headerName, includeNullValues);
            if (colValues != default)
            {
                var res = caseSensetive ? colValues.FirstOrDefault(o => o.Value?.ToString() == value?.ToString()) : colValues.FirstOrDefault(o => o.Value?.ToString().ToUpper() == value?.ToString().ToUpper());
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
        /// <param name="caseSensetive"></param>
        /// <returns>RowIds or null if none</returns>
        public int[] GetRowsRecordsRedundants(string headerName, bool omitEachFirstRedundantValue, bool caseSensetive)
        {
            var rowIds = new List<int>();
            var colRecords = GetColRecords(headerName);

            if (colRecords == null)
                return null;

            var duplicates = caseSensetive ? colRecords.GroupBy(o => o.Value).Where(o => o.Count() > 1) : colRecords.GroupBy(o => o.Value?.ToString().ToUpper()).Where(o => o.Count() > 1);
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