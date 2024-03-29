﻿using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
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
        internal Regex CsvSplit { get { return csvSplit ??= new("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)", RegexOptions.Compiled); } }

        private char delimiter = ',';

        /// <summary>
        /// Merges new csv into the returned csv. 
        /// </summary>
        /// <param name="csvIn"></param>
        /// <param name="createNewUniqueHeaderIfAlreadyExists">True: If fx value='Id' and exists, a new header will be made, called 'Id_2'. False: If fx value='Id' and exists, then the same id will be returned and no new header will be created</param>
        public Csv AddCsv(Csv csvIn, bool createNewUniqueHeaderIfAlreadyExists = false)
        {
            if (csvIn == null || !csvIn.Records.Any())
                return this;

            if (csvIn.Config == null && Config != null)
                csvIn.Config = Config;

            var col = 0;
            var rowStart = RowLimit.Max + 1;
            foreach (var header in csvIn.Headers)
            {
                var row = rowStart;
                var origHeader = Headers.SingleOrDefault(o => o.Value.Equals(header.Value));
                if (origHeader.Value == null)
                    AddHeader(header.Value, createNewUniqueHeaderIfAlreadyExists, out col);
                else
                    col = origHeader.Key;

                for (int r = csvIn.RowLimit.Min; r <= csvIn.RowLimit.Max; r++)
                {
                    AddRecord(row, col, csvIn.GetRecord(r, header.Key));
                    row++;
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
        public Csv AddCsvFile(string filePath, char delimiter = ',', int? take = null)
        {
            if (!File.Exists(filePath))
                return default;

            using var stream = new FileStream(filePath, FileMode.Open);
            return AddCsvStream(stream, delimiter, take);
        }

        /// <summary>
        /// Imports csv-data from a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="delimiter"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public Csv AddCsvStream(Stream stream, char delimiter = ',', int? take = null)
        {
            if (stream == null || stream.Length == 0)
                return this;

            var thisIsEmpty = !Records.Any() && !Headers.Any();
            stream.Position = 0;
            using var reader = new StreamReader(stream, leaveOpen: true);
            if (delimiter != ',')
            {
                this.delimiter = delimiter;

                //TODO! Måske kan jeeg bruge denne: Kilde https://github.com/JoshClose/CsvHelper/blob/master/src/CsvHelper/CsvParser.cs#L304
                // Escape regex special chars to use as regex pattern.
                //var pattern = Regex.Replace(delimiter, @"([.$^{\[(|)*+?\\])", "\\$1");

                csvSplit = new($"(?<=^|{delimiter})(\"(?:[^\"]|\"\")*\"|[^{delimiter}]*)", RegexOptions.Compiled);
            }
            var csvIn = new Csv(this.Config);
            AddHeader(csvIn, reader.ReadLine());

            int row = 1;
            string line;
            while ((line = reader.ReadLine()) != null && (take == null || take != null && row < take))
                ReadRow(csvIn, line, row++);

            return thisIsEmpty ? csvIn : this.AddCsv(csvIn);
        }


        /// <summary>
        /// Load data from a dataTable
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="take">If it only shoul be some data that should be loaded, if the data amount is big</param>
        public Csv AddDataTable(DataTable dataTable, int? take = null)
        {
            if (dataTable == null || dataTable.Columns.Count == 0)
                return this;

            var thisIsEmpty = !Records.Any() && !Headers.Any();
            var csvIn = new Csv(this.Config);
            for (int col = 1; col <= dataTable.Columns.Count; col++)
                csvIn.AddHeader(col, dataTable.Columns[col - 1].ToString());

            for (int row = 1; row <= dataTable.Rows.Count; row++)
            {
                var rowItems = dataTable.Rows[row - 1].ItemArray;
                for (int col = 1; col <= rowItems.Length; col++)
                    csvIn.AddRecord(row, col, rowItems[col - 1]);

                if (take != null && take == row)
                    break;
            }
            return thisIsEmpty ? csvIn : this.AddCsv(csvIn);
        }

        /// <summary>
        /// Imports Csv from a list.
        /// </summary>
        public Csv AddModel<T>(IEnumerable<T> source) where T : class
        {
            var thisIsEmpty = !Records.Any() && !Headers.Any();
            var csvIn = new Csv(this.Config);
            var r = 1;
            foreach (var item in source)
            {
                foreach (var prop in item.GetType().GetProperties())
                {
                    var val = prop.GetValue(item, null);
                    csvIn.AddRecord(r, prop.Name, val);
                }
                r++;
            }
            return thisIsEmpty ? csvIn : this.AddCsv(csvIn);
        }

        /// <summary>
        /// Imports Csv from an excel file.
        /// </summary>
        /// <param name="filePath">A path like c\:file.xls or c:\fil.xlsx</param>
        /// <param name="paneNumber">The pane number starting from number 1</param>
        /// <param name="colStart">The start column in the excel pane. Minimum 1</param>
        /// <param name="rowStart">The start row in the excel pane. Minimum 1</param>
        /// <param name="firstRowIsHeader">If the first row continas header values</param>
        public Csv AddExcelFile(string filePath, int paneNumber = 1, int rowStart = 1, int colStart = 1, bool firstRowIsHeader = true)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return AddExcelStream(stream, paneNumber, colStart, rowStart, firstRowIsHeader);
        }

        /// <summary>
        /// Imports Csv from an excel stream.
        /// </summary>
        /// <param name="stream">The stream containg data from an excel spread sheet</param>
        /// <param name="paneNumber">The pane number starting from number 1</param>
        /// <param name="colStart">The start column in the excel pane. Minimum 1</param>
        /// <param name="rowStart">The start row in the excel pane. Minimum 1</param>
        /// <param name="firstRowIsHeader">If the first row continas header values</param>
        public Csv AddExcelStream(Stream stream, int paneNumber = 1, int colStart = 1, int rowStart = 1, bool firstRowIsHeader = true)
        {
            var thisIsEmpty = !Records.Any() && !Headers.Any();
            var csvIn = new Csv(this.Config);
            var wb = new XLWorkbook(stream);
            if (paneNumber < 1 || paneNumber > wb.Worksheets.Count)
                return this;

            var ws = wb.Worksheets.ElementAt(paneNumber - 1);
            var firstPossibleAddress = ws.Row(rowStart).Cell(colStart).Address;
            var lastPossibleAddress = ws.LastCellUsed().Address;
            var table = ws.Tables.Any() ? ws.Tables.First() : ws.Range(firstPossibleAddress, lastPossibleAddress).RangeUsed().AsTable();
            var csvRow = 1;
            var excelRow = 1;
            foreach (var row in table.RowsUsed())  //Inspiration: https://github.com/closedxml/closedxml/wiki/Using-Tables
            {
                var recordAdded = false;
                var csvCol = 1;
                foreach (var cell in row.Cells())
                {
                    var val = cell.CachedValue;  //By using cached Value, some errors with relation to tables are not thrown as with cell.Value. Furthermore danish punctuation as 1.000,00 are witten as 1000.00
                    if (excelRow == 1 && firstRowIsHeader)
                        csvIn.AddHeader(csvCol, val.ToString());
                    else
                    {
                        if (val.GetType() == typeof(double))
                        {
                            var valInt = Convert.ToInt32(val);
                            if (valInt == (double)val)
                                val = valInt;
                        }

                        csvIn.AddRecord(csvRow, csvCol, val);
                        recordAdded = true;
                    }
                    csvCol++;
                }
                if (recordAdded)
                    csvRow++;

                excelRow++;
            }
            return thisIsEmpty ? csvIn : this.AddCsv(csvIn);
        }

        /// <summary>
        /// Load data from an Expandolist
        /// </summary>
        /// <param name="list"></param>
        /// <returns>If list is empty, then an empty csv</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public Csv AddExpandoObjects(IEnumerable<dynamic> list)
        {
            if (!list.Any())
                return this;

            var thisIsEmpty = !Records.Any() && !Headers.Any();
            var csvIn = new Csv(this.Config);
            int col = 1;
            foreach (KeyValuePair<string, object> item in list.First())
            {
                csvIn.Headers.Add(col, item.Key.ToString());
                col++;
            }

            int row = 1;
            foreach (IDictionary<string, object> cols in list)
            {
                col = 1;
                foreach (var value in cols.Values)
                {
                    csvIn.AddRecord(row, col, value);
                    col++;
                }
                row++;
            }
            return thisIsEmpty ? csvIn : this.AddCsv(csvIn);
        }

        /// <summary>
        /// Converts a json string into a CSV.
        /// </summary>
        /// <param name="json">A string like: "{\"a\":1,\"b\":1}" or "[{\"a\":1,\"b\":1},{\"b\":1},{\"a\":1}]"</param>
        /// <param name="convertArraysToStrings">True: arrays are converted to a single string. False: They are split out and gets each a column</param>
        /// <returns></returns>
        public Csv AddJson(string json, bool convertArraysToStrings)
        {
            var thisIsEmpty = !Records.Any() && !Headers.Any();
            var csvIn = new Csv(this.Config);
            var jToken = JToken.Parse(json);
            var rowStart = csvIn.RowLimit.Max + jToken.Type == JTokenType.Array ? 0 : 1;
            FromJsonSub(jToken.Children(), rowStart, convertArraysToStrings, csvIn);
            return thisIsEmpty ? csvIn : this.AddCsv(csvIn);
        }

        private void FromJsonSub(IEnumerable<JToken> jTokens, int row, bool convertArraysToStrings, Csv csvIn)
        {
            foreach (JToken item in jTokens)
            {
                if (item.Parent.Parent == null && item.Parent.Type == JTokenType.Array)
                    ++row;

                if (item.Type == JTokenType.Array)
                {
                    var value = item.ToString().Replace("\r\n", string.Empty).TrimStart('[').TrimEnd(']').Trim();
                    csvIn.AddRecord(row, item.Path, value);
                }
                else if (item.Children().Any())
                    FromJsonSub(item.Children(), row, convertArraysToStrings, csvIn);
                else
                {
                    var path = item.Path;
                    if (path.StartsWith('['))
                    {
                        var end = path.IndexOf('.');
                        if (end != -1)
                            path = path.Substring(end + 1);
                    }
                    csvIn.AddRecord(row, path, ((JValue)item).Value);
                }
            }
        }

        /// <summary>
        /// Converts an object like a propertyclas to csv.
        /// </summary>
        /// <param name="o">An object like a class with poperties</param>
        /// <param name="convertArraysToStrings">True: arrays are converted to a single string. False: They are split out and gets each a column</param>
        /// <returns></returns>
        public Csv AddObject(object o, bool convertArraysToStrings)
        {
            var thisIsEmpty = !Records.Any() && !Headers.Any();
            var csvIn = new Csv(this.Config);
            var jToken = JToken.FromObject(o);
            var rowStart = jToken.Type == JTokenType.Array ? 0 : 1;
            FromJsonSub(jToken.Children(), rowStart, convertArraysToStrings, csvIn);
            return thisIsEmpty ? csvIn : this.AddCsv(csvIn);
        }

        private void ReadRow(Csv csv, string input, int row)
        {
            if (string.IsNullOrEmpty(input))
                return;

            int col = 1;
            foreach (Match match in CsvSplit.Matches(input))
                csv.AddRecord(row, col++, match.Value?.Trim('"'));
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

        /// <summary>
        /// Splits a string by a delimiter
        /// </summary>
        /// <param name="input">A string like "a, b" will becom "a", "b"</param>
        /// <param name="delimiter">the seprator</param>
        /// <returns></returns>
        public static Dictionary<int, string> SplitString(string input, char delimiter)
        {
            var splitter = new Regex($"(?<=^|{delimiter})(\"(?:[^\"]|\"\")*\"|[^{delimiter}]*)");

            var res = new Dictionary<int, string>();
            if (!string.IsNullOrEmpty(input))
            {
                int col = 1;
                //foreach (Match match in splitter.Matches(input.Trim(' ', delimiter)))
                foreach (Match match in splitter.Matches(input))
                {
                    var val = match.Value?.TrimStart(delimiter).Trim();
                    res.Add(col++, val);
                }
            }
            return res;
        }


        /// <returns>False if last record starts with " but don't end with one - then it must be truncated with the next line</returns>
        private void AddHeader(Csv csv, string input)
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
