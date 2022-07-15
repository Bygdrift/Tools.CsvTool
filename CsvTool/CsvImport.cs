using ExcelDataReader;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Bygdrift.Tools.CsvTool
{
    public partial class Csv
    {
        private Regex csvSplit;
        internal Regex CsvSplit { get { return csvSplit ??= new("(?:^|,)(\"(?:[^\"])*\"|[^,]*)", RegexOptions.Compiled); } }

        private char delimiter = ',';

        /// <summary>
        /// Merges new csv into existing csv. 
        /// </summary>
        /// <param name="mergedCsv"></param>
        /// <param name="createNewUniqueHeaderIfAlreadyExists">True: If fx value='Id' and exists, a new header will be made, called 'Id_2'. False: If fx value='Id' and exists, then the same id will be returned and no new header will be created</param>
        public Csv FromCsv(Csv mergedCsv, bool createNewUniqueHeaderIfAlreadyExists)
        {
            if (this.RowCount == 0 && this.ColCount == 0)
            {
                this.ColMaxLengths = mergedCsv.ColMaxLengths;
                this.ColTypes = mergedCsv.ColTypes;
                this.Headers = mergedCsv.Headers;
                this.Records = mergedCsv.Records;
            }

            var headers = new Dictionary<int, int>();
            var newRowStart = this.RowLimit.Max + 1;
            for (int col = mergedCsv.ColLimit.Min; col <= mergedCsv.ColLimit.Max; col++)
            {
                this.AddHeader(mergedCsv.Headers[col], createNewUniqueHeaderIfAlreadyExists, out int newCol);
                var newRow = newRowStart;
                for (int r = mergedCsv.RowLimit.Min; r <= mergedCsv.RowLimit.Max; r++)
                {
                    this.AddRecord(newRow, newCol, mergedCsv.GetRecord(r, col));
                    newRow++;
                }
            }

            return this;
        }

        /// <summary>
        /// Imports data from a csv file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="delimiter"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public Csv FromCsvFile(string filePath, char delimiter = ',', int? take = null)
        {
            if (!File.Exists(filePath))
                return default;

            using var stream = new FileStream(filePath, FileMode.Open);
            return FromCsvStream(stream, delimiter, take);
        }

        /// <summary>
        /// Imports csv-data from a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="delimiter"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public Csv FromCsvStream(Stream stream, char delimiter = ',', int? take = null)
        {
            if (stream == null || stream.Length == 0)
                return this;

            stream.Position = 0;
            using var reader = new StreamReader(stream, leaveOpen: true);

            if (delimiter != ',')
            {
                this.delimiter = delimiter;
                csvSplit = new($"(?:^|{delimiter})(\"(?:[^\"])*\"|[^{delimiter}]*)", RegexOptions.Compiled);
            }

            ReadHeader(this, reader.ReadLine());

            int row = 1;
            string line;
            while ((line = reader.ReadLine()) != null && (take == null || take != null && row < take))
                ReadRow(this, line, row++);

            return this;
        }


        /// <summary>
        /// Load data from a dataTable
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="take">If it only shoul be some data that should be loaded, if the data amount is big</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public Csv FromDataTable(DataTable dataTable, int? take = null)
        {
            if (dataTable == null || dataTable.Columns.Count == 0)
                return this;

            for (int col = 1; col <= dataTable.Columns.Count; col++)
                this.AddHeader(col, dataTable.Columns[col - 1].ToString());

            for (int row = 1; row <= dataTable.Rows.Count; row++)
            {
                var rowItems = dataTable.Rows[row - 1].ItemArray;
                for (int col = 1; col <= rowItems.Length; col++)
                    this.AddRecord(row, col, rowItems[col - 1]);

                if (take != null && take == row)
                    break;
            }
            return this;
        }

        /// <summary>
        /// Converts a json string into a CSV.
        /// </summary>
        /// <param name="json">A string like: "{\"a\":1,\"b\":1}" or "[{\"a\":1,\"b\":1},{\"b\":1},{\"a\":1}]"</param>
        /// <param name="convertArraysToStrings">True: arrays are converted to a single string. False: They are split out and gets each a column</param>
        /// <returns></returns>
        public Csv FromJson(string json, bool convertArraysToStrings)
        {
            var jToken = JToken.Parse(json);
            var row = this.RowLimit.Max + jToken.Type == JTokenType.Array ? 0 : 1;
            FromJsonSub(jToken.Children(), row, convertArraysToStrings);
            return this;
        }

        private void FromJsonSub(IEnumerable<JToken> jTokens, int row, bool convertArraysToStrings)
        {
            foreach (JToken item in jTokens)
            {
                if (item.Parent.Parent == null && item.Parent.Type == JTokenType.Array)
                    ++row;

                if (item.Type == JTokenType.Array)
                {
                    var value = item.ToString().Replace("\r\n", string.Empty).TrimStart('[').TrimEnd(']').Trim();
                    this.AddRecord(row, item.Path, value);
                }
                else if (item.Children().Any())
                    FromJsonSub(item.Children(), row, convertArraysToStrings);
                else
                {
                    var path = item.Path;
                    if (path.StartsWith('['))
                    {
                        var end = path.IndexOf('.');
                        if (end != -1)
                            path = path.Substring(end + 1);
                    }
                    this.AddRecord(row, path, ((JValue)item).Value);
                }
            }
        }

        /// <summary>
        /// Imports Csv from an excel file.
        /// </summary>
        /// <param name="filePath">A path like c\:file.xls or c:\fil.xlsx</param>
        /// <param name="paneNumber">The pane number starting from number 1</param>
        /// <param name="colStart">The start column in the excel pane. Minimum 1</param>
        /// <param name="rowStart">The start row in the excel pane. Minimum 1</param>
        /// <param name="firstRowIsHeader">If the first row continas header values</param>
        public Csv FromExcelFile(string filePath, int paneNumber = 1, int rowStart = 1, int colStart = 1, bool firstRowIsHeader = true)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return FromExcelStream(stream, paneNumber, colStart, rowStart, firstRowIsHeader);
        }

        /// <summary>
        /// Imports Csv from an excel stream.
        /// </summary>
        /// <param name="stream">The stream containg data from an excel spread sheet</param>
        /// <param name="paneNumber">The pane number starting from number 1</param>
        /// <param name="colStart">The start column in the excel pane. Minimum 1</param>
        /// <param name="rowStart">The start row in the excel pane. Minimum 1</param>
        /// <param name="firstRowIsHeader">If the first row continas header values</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public Csv FromExcelStream(Stream stream, int paneNumber = 1, int colStart = 1, int rowStart = 1, bool firstRowIsHeader = true)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);  //Removes error: 'No data is available for encoding 1252'
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                if (paneNumber < 1 || paneNumber > reader.ResultsCount)
                    return this;

                for (int pane = 1; pane <= reader.ResultsCount; pane++)
                {
                    if (pane == paneNumber)
                        break;

                    reader.NextResult();
                }

                var excelColMax = reader.FieldCount;
                var excelRowMax = reader.RowCount;
                var csvRow = 1;

                if (excelColMax == 0 || excelRowMax == 0 || colStart > excelColMax || rowStart > excelRowMax)
                    return this;

                for (int excelRow = 1; excelRow <= excelRowMax; excelRow++)
                {
                    reader.Read();
                    if (excelRow >= rowStart)
                    {
                        var recordAdded = false;
                        var csvCol = 1;
                        for (int excelCol = colStart; excelCol <= excelColMax; excelCol++)
                        {
                            var val = reader.GetValue(excelCol - 1);

                            if (val != null && excelRow == rowStart && firstRowIsHeader)
                                this.AddHeader(csvCol, val.ToString());
                            if (val != null && (excelRow > rowStart || !firstRowIsHeader))
                            {
                                if (val.GetType() == typeof(double))
                                {
                                    var valInt = Convert.ToInt32(val);
                                    if (valInt == (double)val)
                                        val = valInt;
                                }

                                this.AddRecord(csvRow, csvCol, val);
                                recordAdded = true;
                            }
                            csvCol++;
                        }
                        if (recordAdded)
                            csvRow++;
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Load data from an Expandolist
        /// </summary>
        /// <param name="list"></param>
        /// <returns>If list is empty, then an empty csv</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public Csv FromExpandoObjects(IEnumerable<dynamic> list)
        {
            if (!list.Any())
                return this;

            int col = 1;
            foreach (KeyValuePair<string, object> item in list.First())
            {
                this.Headers.Add(col, item.Key.ToString());
                col++;
            }

            int row = 1;
            foreach (IDictionary<string, object> cols in list)
            {
                col = 1;
                foreach (var value in cols.Values)
                {
                    this.AddRecord(row, col, value);
                    col++;
                }
                row++;
            }
            return this;
        }

        /// <summary>
        /// Converts an object like a propertyclas to csv.
        /// </summary>
        /// <param name="o">An object like a class with poperties</param>
        /// <param name="convertArraysToStrings">True: arrays are converted to a single string. False: They are split out and gets each a column</param>
        /// <returns></returns>
        public Csv FromObject(object o, bool convertArraysToStrings)
        {
            var jToken = JToken.FromObject(o);
            FromJsonSub(jToken.Children(), jToken.Type == JTokenType.Array ? 0 : 1, convertArraysToStrings);
            return this;
        }

        private void ReadRow(Csv csv, string input, int row)
        {
            if (string.IsNullOrEmpty(input))
                return;

            int col = 1;
            foreach (Match match in CsvSplit.Matches(input))
            {
                string val = match.Value?.TrimStart(delimiter).Trim('"').Trim();
                csv.AddRecord(row, col, val);
                col++;
            }
        }

        internal Dictionary<int, object> SplitString(string input)
        {
            var res = new Dictionary<int, object>();
            if (!string.IsNullOrEmpty(input))
            {
                int col = 1;
                foreach (Match match in CsvSplit.Matches(input))
                {
                    var val = match.Value?.TrimStart(delimiter).Trim('"');
                    res.Add(col++, val);
                }
            }
            return res;
        }

        /// <returns>False if last record starts with " but don't end with one - then it must be truncated with the next line</returns>
        private void ReadHeader(Csv csv, string input)
        {
            if (string.IsNullOrEmpty(input))
                return;

            var matches = CsvSplit.Matches(input);
            for (int col = 1; col <= matches.Count; col++)
            {
                var val = matches[col - 1].Value?.TrimStart(delimiter).Trim();
                if (col == matches.Count && val.StartsWith('"') && !val.EndsWith('"'))
                    throw new Exception("There are linebreaks within two parantheses in the first line in the file and that is an error.");

                csv.AddHeader(col, val.Trim('"'));
            }
        }
    }
}
