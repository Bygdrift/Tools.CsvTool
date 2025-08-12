﻿using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SlapKit.Excel.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Bygdrift.Tools.CsvTool
{
    /// <summary>
    /// Export csv to different formats
    /// </summary>
    public partial class Csv
    {
        private Dictionary<int, int> _colMaxLengthsWithHeaders;

        /// <summary>
        /// Export to a csv file
        /// </summary>
        public string ToClass(string namespaceName, string className)
        {
            var fields = "";
            if (ColLimit.Max > ColLimit.Min)
                for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                {
                    var header = GetHeader(col);
                    var type = ColTypes.GetValueOrDefault(col);
                    fields += $"public {(type != null ? type.Name : "string")} {header} {{ get; set; }}\n";

                }
            var classCode = $@"
                using System;
                namespace {namespaceName}
                {{
                    public class {className}
                    {{
                        {fields}
                    }}

                }}";
            var tree = CSharpSyntaxTree.ParseText(classCode);
            var root = tree.GetRoot().NormalizeWhitespace();
            var result = root.ToFullString();

            return result;
        }

        /// <summary>
        /// Export to a csv file
        /// </summary>
        /// <param name="filePath">Like c:\export.csv</param>
        /// <param name="take">The amount of rows to export. If null, then all will be exported</param>
        public Csv ToCsvFile(string filePath, int? take = null)
        {
            if (this == null || !Records.Any())
                return this;

            var stream = ToCsvStream(false, take);

            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);

            return this;
        }

        /// <summary>
        /// Export to a csv stream
        /// </summary>
        /// <param name="take">The amount of rows to export. If null, then all will be exported</param>
        /// <param name="addSpaces">Adds spaces for easy reading of output</param>
        public Stream ToCsvStream(bool addSpaces = false, int? take = null)
        {
            //Denne rammer alt fremover - skal kun være lokal.
            //System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // Ensures right decimal punctiation

            var stream = new MemoryStream();
            if (this != null || !Records.Any())
            {
                using var writer = new StreamWriter(stream, encoding: Encoding.UTF8, leaveOpen: true);
                writer.WriteLine(WriteRow(null, addSpaces).StringBuilder);  //Header

                for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
                {
                    if (take != null && row == take)
                        break;

                    var res = WriteRow(row, addSpaces);
                    if (!res.IsEmpy)
                        writer.WriteLine(res.StringBuilder);
                }

                writer.Flush();
            }

            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Export csv to an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> ToModel<T>() where T : class
        {
            var json = ToJson(false, true);
            var res = JsonConvert.DeserializeObject<List<T>>(json);
            return res;
        }

        /// <summary>
        /// Export to a csv string
        /// </summary>
        /// <param name="addSpaces">Adds spaces for easy reading of output</param>
        /// <param name="take">The amount of rows to export. If null, then all will be exported</param>
        public string ToCsvString(bool addSpaces = false, int? take = null)
        {
            //Denne rammer alt fremover - skal kun være lokal.
            //System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // Ensures right decimal punctiation
            var writer = new StringBuilder();
            if (this != null || !Records.Any())
            {
                writer.Append(WriteRow(null, addSpaces).StringBuilder).Append("\n");  //Header
                for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
                {
                    if (take != null && row == take)
                        break;

                    var res = WriteRow(row, addSpaces);
                    if (!res.IsEmpy)
                        writer.Append(res.StringBuilder).Append("\n");
                }
            }
            return writer.ToString();
        }

        /// <summary>
        /// Exports to a data table
        /// </summary>
        /// <param name="convertModelToJson">If value is a model, then it can be converted to json</param>
        /// <param name="take"></param>
        /// <returns></returns>
        public DataTable ToDataTable(bool convertModelToJson = false, int? take = null)
        {
            var table = new DataTable();
            for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
            {
                var header = GetHeader(col);
                ColTypes.TryGetValue(col, out Type type);
                if (type == null)
                    type = typeof(string);

                if (convertModelToJson && RecordIsModel(col))
                    type = typeof(string);

                table.Columns.Add(header, type);
            }

            foreach (var row in GetRowsRecords(false))
            {
                if (take != null && row.Key >= take)
                    break;

                if (convertModelToJson)
                {
                    var entities = new List<object>();
                    foreach (var record in row.Value)
                    {
                        if (RecordIsModel(record.Key))
                            entities.Add(JsonConvert.SerializeObject(record.Value));
                        else
                            entities.Add(record.Value);
                    }
                    table.Rows.Add([.. entities]);
                }
                else
                    table.Rows.Add(row.Value.Select(o => o.Value).ToArray());

            }

            return table;
        }


        //Is removed. Instead use: new Csv().FromFile().ToDataTable()
        //public DataTable ToDataTable(string csvFilePath)
        //{
        //    var dt = new DataTable();
        //    using (var sr = new StreamReader(csvFilePath))
        //    {
        //        var headers = sr.ReadLine().Split(',');
        //        foreach (string header in headers)
        //            dt.Columns.Add(header);

        //        while (!sr.EndOfStream)
        //        {
        //            var rows = sr.ReadLine().Split(',');
        //            var dr = dt.NewRow();
        //            for (int i = 0; i < headers.Length; i++)
        //                dr[i] = rows[i];

        //            dt.Rows.Add(dr);
        //        }
        //    }
        //    return dt;
        //}


        /// <summary>
        /// Export Excel to a stream
        /// </summary>
        /// <param name="paneName">The name of the worksheet</param>
        /// <param name="tableName">The name of the table inside Excel. If null, no fancy talbe will be added</param>
        /// <param name="take">The amount of rows to export. If null, then all will be exported</param>
        public Stream ToExcelStream(string paneName = "Data", string tableName = null, int? take = null)
        {
            var workbook = ToExcelAsXlWokBook(paneName, tableName, take);
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            workbook.Dispose();
            return stream;
        }

        /// <summary>
        /// Export Excel as a file
        /// Using this library: https://docs.closedxml.io/en/latest/installation.html
        /// </summary>
        /// <param name="filePath">Like c:\file.xlsx</param>
        /// <param name="paneName">The name of the worksheet</param>
        /// <param name="tableName">The name of the table inside Excel. If null, no fancy table will be added</param>
        /// <param name="take">The amount of rows to export. If null, then all will be exported</param>
        public Csv ToExcelFile(string filePath, string paneName = "Data", string tableName = null, int? take = null)
        {
            var workbook = ToExcelAsXlWokBook(paneName, tableName, take);
            workbook.SaveAs(filePath);
            workbook.Dispose();
            return this;
        }

        /// <summary>
        /// Export Excel as a XLWorkbook
        /// Using this library: https://docs.closedxml.io/en/latest/installation.html
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="paneName">The name of the worksheet</param>
        /// <param name="tableName">The name of the table inside Excel. If null, no fancy table will be added</param>
        /// <param name="rowStart">The start row in the excel pane. Minimum 1</param>
        public IXLWorksheet ToExcelSheet(XLWorkbook workbook, string paneName = "Data", string tableName = null, int rowStart = 1)
        {
            var worksheet = workbook.Worksheets.FirstOrDefault(s => s.Name == paneName);
            var table = worksheet?.Tables.FirstOrDefault(s => s.Name == tableName);


            if (worksheet == null)
                worksheet = workbook.Worksheets.Add(paneName);

            if (table != null)
            {

                var range = table.DataRange.RangeAddress;
                var fullTableRange = worksheet.Range(table.RangeAddress.ToString());

                fullTableRange.Clear(XLClearOptions.Contents);

                //table.Range().Clear(XLClearOptions.Contents);
            }

            if (this != null && Headers.Count != 0)
            {
                for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                    worksheet.Cell(rowStart, col).Value = Headers[col];

                for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
                    for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                    {
                        TryGetRecord(row, col, true, out object value);
                        worksheet.Cell(rowStart + row, col).Value = XLCellValue.FromObject(value);
                    }
            }

            if (!string.IsNullOrEmpty(tableName))
            {
                var rangeUsed = worksheet.RangeUsed();
                var firstCell = worksheet.Cell(rowStart, 1);
                var lastCell = worksheet.Cell(RowLimit.Max+2, ColLimit.Max);
                if (table != null)
                    table.Resize(firstCell, lastCell);
                else
                    worksheet.Range(firstCell, lastCell).CreateTable(tableName);
            }
            return worksheet;
        }

        private XLWorkbook ToExcelAsXlWokBook(string paneName, string tableName = null, int? take = null)
        {
            //System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // Ensures right decimal punctiation
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(paneName);

            if (this != null && Records.Any())
            {
                for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                    worksheet.Cell(1, col).Value = Headers[col];

                for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
                    for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                    {
                        TryGetRecord(row, col, true, out object value);
                        worksheet.Cell(row + 1, col).Value = XLCellValue.FromObject(value);
                    }

                if (!string.IsNullOrEmpty(tableName))
                    worksheet.RangeUsed().CreateTable(tableName);
            }
            return workbook;
        }

        //private XLCellValue GetVal(int row, int col)
        //{
        //    var val = GetRecord(row, col);
        //    if (val != null)
        //    {
        //        var type = val.GetType();
        //        if (type != typeof(string) && type.IsClass)
        //            val = JsonConvert.SerializeObject(val);
        //    }
        //    var res = XLCellValue.FromObject(val);
        //    return res;
        //}

        /// <summary>
        /// Exports data as a Newtonsoft JArray
        /// </summary>
        /// <param name="removeSpacesFromHeaders">Removes all spaces from alle headers</param>
        public JArray ToJArray(bool removeSpacesFromHeaders = false)
        {
            var res = new JArray();

            for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
            {
                var rowObject = new JObject();
                for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                    if (TryGetRecord(row, col, true, out object val))
                    {
                        var name = removeSpacesFromHeaders ? Headers[col].Replace(" ", string.Empty) : Headers[col];

                        var a = new JProperty(name, val);
                        rowObject.Add(a);

                    }

                if (rowObject.Count > 0)
                    res.Add(rowObject);
            }

            return res;
        }

        /// <summary>
        /// Exports csv as a json string
        /// </summary>
        /// <param name="useFormatting">If true, then data gets indented and easy to read, but longer</param>
        /// <param name="removeSpacesFromHeaders">Removes all spaces from alle headers</param>
        public string ToJson(bool useFormatting = false, bool removeSpacesFromHeaders = false)
        {
            return ToJArray(removeSpacesFromHeaders).ToString(useFormatting ? Formatting.Indented : Formatting.None);
        }

        /// <summary>
        /// Exports data as a Newtonsoft JObject
        /// </summary>
        public List<JObject> ToJObjectList()
        {
            var res = new List<JObject>();

            for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
            {
                var rowObject = new JObject();
                for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                    if (TryGetRecord(row, col, out object val))
                        rowObject.Add(new JProperty(Headers[col], val));

                if (rowObject.Count > 0)
                    res.Add(rowObject);
            }

            return res;
        }

        /// <summary>
        /// Exports data as an ExpandoList.
        /// </summary>
        public IEnumerable<Dictionary<string, object>> ToExpandoList()
        {
            var res = new List<Dictionary<string, object>>();
            for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
            {
                var dict = new Dictionary<string, object>();
                foreach (var header in Headers)
                {
                    Records.TryGetValue((row, header.Key), out object val);
                    if (val?.GetType() == typeof(DateTime))
                        val = Config.DateHelper.DateTimeToString((DateTime)val);

                    //Records.TryGetValue((row, header.Key), out object value);
                    dict.Add(header.Value, val);
                }
                res.Add(dict);
            }
            return res;
        }

        private Dictionary<int, int> colMaxLengthsWithHeaders
        {
            get
            {
                if (_colMaxLengthsWithHeaders == null)
                {
                    _colMaxLengthsWithHeaders = ColMaxLengths;
                    foreach (var item in ColMaxLengths)
                    {
                        var headerLength = Headers[item.Key].Length;
                        if (headerLength > item.Value)
                            _colMaxLengthsWithHeaders[item.Key] = headerLength;
                    }
                }

                return _colMaxLengthsWithHeaders;
            }
        }

        /// <param name="row">If r is null, it is a header. Else it is a normal row</param>
        /// <param name="addSpaces">Adds spaces for easy reading of output</param>
        private (StringBuilder StringBuilder, bool IsEmpy) WriteRow(int? row, bool addSpaces)
        {
            //Every record must have same number of lines
            //It is NOT possible to have line breaks within a record by quoting the sentence
            //Fields with embedded commas or double-quote characters must be quoted: 1997,Ford,E350,"Super, luxurious truck"
            //Each of the embedded double-quote characters is represented by a pair of double-quote characters: 1997,Ford,E350,"Super, ""luxurious"" truck"
            //Trim leading and trailng spaces
            //spaces outside quotes in a field are not allowed
            //Its okay to have a single quote in a record
            var delimiter = addSpaces ? Config.Delimiter.ToString() + ' ' : Config.Delimiter.ToString();
            var chars = new char[] { '"', ',' };
            var rowObject = new StringBuilder();
            var isEmpty = true;

            for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
            {
                string val;
                if (row == null)  //Is header
                {
                    if (Headers.TryGetValue(col, out string headerVal))
                        val = headerVal.ToString();
                    else
                        val = "Col_" + col;
                }
                else //Not header
                {
                    if (Records.TryGetValue(((int)row, col), out object recordVal))
                    {
                        var type = recordVal?.GetType();
                        if (recordVal == null)
                            val = string.Empty;
                        else if (type == typeof(DateTime))
                        {
                            isEmpty = false;
                            val = Config.DateHelper.DateTimeToString((DateTime)recordVal);
                        }
                        else if (type == typeof(DateTimeOffset))
                        {
                            isEmpty = false;
                            val = Config.DateHelper.DateTimeToString((DateTimeOffset)recordVal);
                        }
                        else
                        {
                            isEmpty = false;
                            val = recordVal != null ? Convert.ToString(recordVal, Config.CultureInfo.NumberFormat) : string.Empty;
                        }
                    }
                    else
                        val = string.Empty;
                }


                if (val.Contains('\n'))
                    val = val.Replace('\n', ' ');

                if (val.IndexOfAny(chars) != -1)
                    val = "\"" + val.Replace("\"", "\"\"") + "\"";

                if (addSpaces)
                    try  //Passing double can go from 21 to 24 when converted to string - ex: 0.0000001129498786607691 becomes 1,12949878660769E-07
                    {
                        val += new string(' ', colMaxLengthsWithHeaders[col] - val.Length);
                    }
                    catch (Exception)
                    {
                        val = val.Substring(0, colMaxLengthsWithHeaders[col]);
                    }

                if (col < ColLimit.Max)
                    val += delimiter;

                rowObject.Append(val);
            }
            return (rowObject, isEmpty);
        }
    }
}
