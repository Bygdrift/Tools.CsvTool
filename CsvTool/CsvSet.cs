using System;
using System.Collections.Generic;
using System.Linq;

namespace Bygdrift.Tools.CsvTool
{
    public partial class Csv
    {
        /// <summary>
        /// Add a whole column to the CSV and give each record in the column, the same value
        /// </summary>
        /// <param name="headerName">name of the header</param>
        /// <param name="value">The value to be distributed out in each record in the colum</param>
        /// <param name="createNewUniqueHeaderIfAlreadyExists">True: If fx value='Id' and exists, a new header will be made, called 'Id_2'. False: If fx value='Id' and exists, then the same id will be returned and no new header will be created</param>
        public Csv AddColumn(string headerName, object value, bool createNewUniqueHeaderIfAlreadyExists)
        {
            AddHeader(headerName, createNewUniqueHeaderIfAlreadyExists, out int col);
            for (int row = 1; row <= RowLimit.Max; row++)
                AddRecord(row, col, value);

            return this;
        }

        /// <param name="col">First col has lowest number and the same number used to referenced records</param>
        /// <param name="headerName">Headers name</param>
        public Csv AddHeader(int col, string headerName)
        {
            headerName = UniqueHeader(headerName, false);
            if (Headers.ContainsKey(col))
                Headers[col] = headerName;
            else
                Headers.Add(col, headerName);

            if (ColTypes == null || !ColTypes.ContainsKey(col))
                ColTypes.Add(col, null);  //Remember to tak care of those nulls that doesnt gets parsed to string, decimal, long and bool

            if (_colLimit.Min > col || _colLimit.Min == null)
                _colLimit.Min = col;

            if (_colLimit.Max < col || _colLimit.Max == null)
                _colLimit.Max = col;

            if (!ColMaxLengths.ContainsKey(col))
                ColMaxLengths.Add(col, 0);

            return this;
        }


        /// <summary>Adds a header to the end</summary>
        public Csv AddHeader(string headerName)
        {
            var col = _colLimit.Max == null ? 1 : ColLimit.Max + 1;
            AddHeader(col, headerName);
            return this;
        }

        /// <summary>Adds a header to the end and returns the col</summary>
        public Csv AddHeader(string headerName, out int col)
        {
            col = _colLimit.Max == null ? 1 : ColLimit.Max + 1;
            AddHeader(col, headerName);
            return this;
        }

        /// <summary>
        /// Adds a header
        /// </summary>
        /// <param name="headerName">the name of the header</param>
        /// <param name="createNewUniqueHeaderIfAlreadyExists">True: If fx value='Id' and exists, a new header will be made, called 'Id_2'. False: If fx value='Id' and exists, then the same id will be returned and no new header will be created</param>
        /// <param name="col">The int of the header</param>
        /// <returns></returns>
        public Csv AddHeader(string headerName, bool createNewUniqueHeaderIfAlreadyExists, out int col)
        {
            if (createNewUniqueHeaderIfAlreadyExists)
                headerName = UniqueHeader(headerName, false);

            var header = Headers.SingleOrDefault(o => o.Value.Equals(headerName));
            if (header.Value != null)
                col = header.Key;
            else
            {
                col = ColLimit.Max + 1;
                AddHeader(col, headerName);
            }

            return this;
        }

        /// <summary>
        /// Add multiple headers to a csv.
        /// First header col is 1 unless some cols already has been added
        /// </summary>
        /// <param name="headers">Comma seperated string with headers. There can be space between</param>
        public Csv AddHeaders(string headers)
        {
            var cols = ColCount;
            foreach (var item in SplitString(headers, ','))
                AddHeader(item.Key + cols, item.Value.ToString().Trim());

            return this;
        }

        /// <summary>
        /// Add multiple headers to a csv
        /// </summary>
        public Csv AddHeaders(Dictionary<int, string> headers)
        {
            foreach (var item in headers)
                AddHeader(item.Key, item.Value);

            return this;
        }

        /// <summary>
        /// Add multiple headers to a csv
        /// </summary>
        public Csv AddHeaders(string[] headers)
        {
            var col = 1;
            foreach (var item in headers)
                AddHeader(col++, item);

            return this;
        }

        ///// <summary>
        ///// Add multiple headers to a csv
        ///// </summary>
        //public Csv AddHeaders(Dictionary<string, int> headers)
        //{
        //    foreach (var item in headers)
        //        AddHeader(item.Value, item.Key);

        //    return this;
        //}

        /// <summary>
        /// Adds or updates a record in the csv
        /// </summary>
        public Csv AddRecord(int row, int col, object value)
        {
            SetAndVerifyColTypeAndLength(col, ref value);

            if (Records.ContainsKey((row, col)))
                Records[(row, col)] = value;
            else
                Records.Add((row, col), value);

            if (_colLimit.Min > col || _colLimit.Min == null)
                _colLimit.Min = col;
            if (_colLimit.Max < col || _colLimit.Max == null)
                _colLimit.Max = col;
            if (_rowLimit.Min > row || _rowLimit.Min == null)
                _rowLimit.Min = row;
            if (_rowLimit.Max < row || _rowLimit.Max == null)
                _rowLimit.Max = row;

            return this;
        }

        /// <summary>
        /// Adds or updates a record in the csv
        /// </summary>
        public Csv AddRecord(int row, string headerName, object value, bool createNewUniqueHeaderIfAlreadyExists = false)
        {
            return AddHeader(headerName, createNewUniqueHeaderIfAlreadyExists, out int col).AddRecord(row, col, value);
        }

        /// <summary>
        /// Add a row to the csv
        /// </summary>
        public Csv AddRow(Dictionary<int, object> columnRecords)
        {
            var r = Records.Count == 0 ? 1 : RowLimit.Max + 1;
            foreach (var record in columnRecords)
                AddRecord(r, record.Key, record.Value);

            return this;
        }

        /// <summary>
        /// If there are 3 columns and 3 args, the 3 args will be distributed into each column in the order they are mentioned.
        /// If there are 3 columns but only 2 args, it will be the first 2 columns that gets the arguments.
        /// If there are 3 columns and 4 args, then a new column will be added.
        /// </summary>
        /// <param name="args">the objects ot add to the row.</param>
        public Csv AddRow(params object[] args)
        {
            if (args == null || args == default)
                return this;

            var row = Records.Count == 0 ? 1 : RowLimit.Max + 1;
            var col = ColCount == 0 ? 1 : ColLimit.Min;
            foreach (var item in args)
            {
                if (!Headers.ContainsKey(col))
                    AddHeader(col, string.Empty);

                AddRecord(row, col, item);
                col++;
            }

            return this;
        }

        /// <summary>
        /// Add a row to the csv
        /// </summary>
        public Csv AddRow(int row, Dictionary<int, object> columnRecords)
        {
            foreach (var record in columnRecords)
                AddRecord(row, record.Key, record.Value);

            return this;
        }

        /// <param name="rows">Data like AddRows("a,1","b,2") gives two rows and two columns</param>
        public Csv AddRows(params string[] rows)
        {
            var r = RowLimit.Max + 1;
            for (int row = 0; row < rows.Length; row++)
            {
                foreach (var item in SplitString(rows[row]))
                    AddRecord(r, item.Key, item.Value);

                r++;
            }
            return this;
        }

        /// <summary>
        /// Changes all valuse in a row of strings, to either be uppercase or lowercase letters
        /// </summary>
        /// <param name="col">The columnId</param>
        /// <param name="toUpper">If true to upper. If false to lower</param>
        public void ColumnChangeCase(int col, bool toUpper)
        {
            for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
            {
                var val = GetRecord(row, col);
                if (val != null && val is string)
                    AddRecord(row, col, toUpper ? val.ToString().ToUpper() : val.ToString().ToLower());
            }
        }

        /// <summary>If header does not exist, then create it and retrun the col</summary>
        [Obsolete("Use AddHeader(headerName, false, out int col) instead. This function will be removed november 2022.")]
        public int GetOrCreateHeader(string headerName)
        {
            AddHeader(headerName, false, out int col);
            return col;
        }

        /// <summary>
        /// Remove empty cols
        /// </summary>
        public void RemoveEmptyColumns()
        {
            var newHeaders = new Dictionary<int, string>();
            var newColMaxLengths = new Dictionary<int, int>();
            var newColTypes = new Dictionary<int, Type>();
            var newRecords = new Dictionary<(int Row, int Col), object>();

            int newCol = 0;
            for (int origCol = ColLimit.Min; origCol <= ColLimit.Max; origCol++)
            {
                var colIsSet = false;
                for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
                    if (Records.TryGetValue((row, origCol), out object val))
                    {
                        if (val != null)
                        {
                            if (!colIsSet)
                            {
                                newCol++;
                                newHeaders.Add(newCol, Headers[origCol]);
                                newColTypes.Add(newCol, ColTypes[origCol]);
                                newColMaxLengths.Add(newCol, ColMaxLengths[origCol]);
                            }
                            colIsSet = true;
                            newRecords.Add((row, newCol), val);
                        }
                    }

            }

            Headers = newHeaders;
            ColTypes = newColTypes;
            Records = newRecords;
            ColMaxLengths = newColMaxLengths;
            _colLimit.Min = newColTypes.Keys.Count > 0 ? newColTypes.Keys.Min() : 0;
            _colLimit.Max = newColTypes.Keys.Count > 0 ? newColTypes.Keys.Max() : 0;
        }

        /// <summary>
        /// Remove empty rows
        /// </summary>
        public void RemoveEmptyRows()
        {
            var newRecords = new Dictionary<(int Row, int Col), object>();

            int newRow = 0;
            for (int origRow = RowLimit.Min; origRow <= RowLimit.Max; origRow++)
            {
                var isEmpty = true;
                foreach (var header in Headers)
                    if (Records.TryGetValue((origRow, header.Key), out object val) && val != null && !val.Equals(""))
                    {
                        if (isEmpty)
                        {
                            isEmpty = false;
                            newRow++;
                        }
                        newRecords.Add((origRow, header.Key), val);
                    }
            }

            Records = newRecords;
            _rowLimit.Min = 0;
            _rowLimit.Max = newRow == 0 ? 0 : newRow;
        }

        /// <summary>
        /// Remove a specific column
        /// </summary>
        /// <param name="col">the colId</param>
        public void RemoveColumn(int col)
        {
            if (col < ColLimit.Min || col > ColLimit.Max)
                return;

            var newHeaders = new Dictionary<int, string>();
            var newColTypes = new Dictionary<int, Type>();
            var newRecords = new Dictionary<(int Row, int Col), object>();

            int newCol = ColLimit.Min;
            for (int origCol = ColLimit.Min; origCol <= ColLimit.Max; origCol++)
            {
                if (origCol != col)
                {
                    if (Headers.TryGetValue(origCol, out string header))
                        newHeaders.Add(newCol, header);

                    newColTypes.Add(newCol, ColTypes[origCol]);
                    for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
                        if (Records.TryGetValue((row, origCol), out object val))
                            newRecords.Add((row, newCol), val);

                    newCol++;
                }
            }

            Headers = newHeaders;
            ColTypes = newColTypes;
            Records = newRecords;
            _colLimit.Min = newColTypes.Keys.Min();
            _colLimit.Max = newColTypes.Keys.Max();
        }

        /// <summary>
        /// Remove all emty records
        /// </summary>
        public void RemoveEmptyRecords()
        {
            var hasChanges = false;
            for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
                    if (Records.TryGetValue((row, col), out object val))
                        if (val == null || string.IsNullOrEmpty(val.ToString()))
                        {
                            Records.Remove((row, col));
                            hasChanges = true;
                        }

            if (hasChanges)
            {
                if (Records.Any())
                {
                    _rowLimit.Min = Records.Min(o => o.Key.Row);
                    _rowLimit.Max = Records.Max(o => o.Key.Row);
                }
                else
                {
                    _rowLimit.Min = null;
                    _rowLimit.Max = null;
                }
            }
        }

        /// <summary>
        /// Remove a specific row
        /// </summary>
        /// <param name="row">RowId</param>
        public void RemoveRow(int row)
        {
            for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                Records.Remove((row, col));

            if (Records.Any())
            {
                _rowLimit.Min = Records.Min(o => o.Key.Row);
                _rowLimit.Max = Records.Max(o => o.Key.Row);
            }
            else
            {
                _rowLimit.Min = null;
                _rowLimit.Max = null;
            }
        }


        /// <summary>
        /// Remove multiple rows
        /// </summary>
        public void RemoveRows(int[] rows, bool includeNullValues = true)
        {
            var newRecords = new Dictionary<(int Row, int Col), object>();

            int newRow = 0;
            for (int origRow = RowLimit.Min; origRow <= RowLimit.Max; origRow++)
            {
                if (!rows.Contains(origRow))
                {
                    var isEmpty = true;
                    foreach (var header in Headers)
                        if (Records.TryGetValue((origRow, header.Key), out object val))
                            if (includeNullValues || (!includeNullValues && val != null && !val.Equals("")))
                            {
                                if (isEmpty)
                                {
                                    isEmpty = false;
                                    newRow++;
                                }
                                newRecords.Add((newRow, header.Key), val);
                            }
                }
            }

            Records = newRecords;
            if (Records.Any())
            {
                _rowLimit.Min = Records.Min(o => o.Key.Row);
                _rowLimit.Max = Records.Max(o => o.Key.Row);
            }
            else
            {
                _rowLimit.Min = null;
                _rowLimit.Max = null;
            }
        }

        /// <summary>
        /// Removes rows that has a repeating header. Could be 'Id' that are equal to 9, two times, then the second will be removed.
        /// </summary>
        /// <param name="headerName">The name of the row tha should have unque content after the method has been run</param>
        /// <param name="ignoreCase"></param>
        /// <returns>True if som is removed</returns>
        public int RemoveRedundantRows(string headerName, bool ignoreCase)
        {
            var rowIds = GetRowsRecordsRedundants(headerName, true, ignoreCase);
            if (rowIds == null)
                return 0;

            RemoveRows(rowIds.ToArray());
            return rowIds.Length;
        }

        /// <summary>
        /// Rename an already named header
        /// </summary>
        public void RenameHeader(string origName, string newName)
        {
            var res = Headers.SingleOrDefault(o => o.Value.Equals(origName));
            if (res.Value != null)
                Headers[res.Key] = newName;
        }

        ///// <summary>
        ///// Update a record
        ///// </summary>
        ///// <param name="col"></param>
        ///// <param name="row"></param>
        ///// <param name="value"></param>
        //public void UpdateRecord(int col, int row, object value)
        //{
        //    SetAndVerifyColTypeAndLength(col, ref value);
        //    Records[(row, col)] = value;
        //}


        /// <summary>
        /// Could be used like: csv.ReplaceRecordContent('\n', '\r');
        /// </summary>
        public void ReplaceRecordContent(char oldChar, char newChar)
        {
            for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
                    if (Records.TryGetValue((row, col), out object val))
                        if (val != null && val.ToString().Contains(oldChar))
                            Records[(row, col)] = val.ToString().Replace(oldChar, newChar);
        }

        /// <summary>
        /// Could be used like: csv.ReplaceRecordContent("Adam", "Eva");
        /// </summary>
        public void ReplaceRecordContent(string oldValue, string newValue)
        {
            for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
                    if (Records.TryGetValue((row, col), out object val))
                        if (val != null && val.ToString().Contains(oldValue))
                            Records[(row, col)] = val.ToString().Replace(oldValue, newValue);
        }
    }
}