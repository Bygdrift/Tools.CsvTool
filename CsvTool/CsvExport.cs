using ClosedXML.Excel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
        /// <summary>
        /// Export to a csv file
        /// </summary>
        /// <param name="filePath">Like c:\export.csv</param>
        /// <param name="take">The amount of rows to export. If null, then all will be exported</param>
        public void ToCsvFile(string filePath, int? take = null)
        {
            if (this == null || !Records.Any())
                return;

            var stream = ToCsvStream(take);

            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);
        }

        /// <summary>
        /// Export to a csv stream
        /// </summary>
        /// <param name="take">The amount of rows to export. If null, then all will be exported</param>
        public Stream ToCsvStream(int? take = null)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // Ensures right decimal punctiation

            var stream = new MemoryStream();
            if (this != null || !Records.Any())
            {
                using var writer = new StreamWriter(stream, encoding: Encoding.UTF8, leaveOpen: true);
                writer.WriteLine(WriteRow(null).StringBuilder);  //Header

                for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
                {
                    if (take != null && row == take)
                        break;

                    var res = WriteRow(row);
                    if (!res.IsEmpy)
                        writer.WriteLine(res.StringBuilder);
                }

                writer.Flush();
            }

            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Exports to a data table
        /// </summary>
        /// <param name="take"></param>
        /// <returns></returns>
        public DataTable ToDataTable(int? take = null)
        {
            var table = new DataTable();
            foreach (var header in Headers)
            {
                ColTypes.TryGetValue(header.Key, out Type type);
                table.Columns.Add(header.Value.ToString(), type);
            }

            foreach (var row in GetRowsRecords(false))
            {
                if (take != null && row.Key >= take)
                    break;

                var entities = row.Value.Select(o => o.Value).ToArray();
                table.Rows.Add(entities);
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
        public Stream ToExcelStream(string paneName, string tableName = null, int? take = null)
        {
            var workbook = ToExcelAsXlWokBook(paneName, tableName, take);
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            workbook.Dispose();
            return stream;
        }

        /// <summary>
        /// Export Excel as a file
        /// </summary>
        /// <param name="filePath">Like c:\file.xlsx</param>
        /// <param name="paneName">The name of the worksheet</param>
        /// <param name="tableName">The name of the table inside Excel. If null, no fancy talbe will be added</param>
        /// <param name="take">The amount of rows to export. If null, then all will be exported</param>
        public void ToExcelFile(string filePath, string paneName, string tableName = null, int? take = null)
        {
            var workbook = ToExcelAsXlWokBook(paneName, tableName, take);
            workbook.SaveAs(filePath);
            workbook.Dispose();
        }

        private XLWorkbook ToExcelAsXlWokBook(string paneName, string tableName = null, int? take = null)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // Ensures right decimal punctiation
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(paneName);

            if (this != null && Records.Any())
            {
                if (!string.IsNullOrEmpty(tableName))
                {
                    var dataTable = ToDataTable(take);
                    worksheet.Cell(1, 1).InsertTable(dataTable.AsEnumerable(), tableName);  //https://github.com/closedxml/closedxml/wiki/Inserting-Tables
                }
                else
                {
                    for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                        worksheet.Cell(1, col + 1).Value = Headers[col];

                    for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
                        for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
                            worksheet.Cell(row + 2, col + 1).Value = GetRecord(row, col);
                }
            }
            return workbook;
        }

        /// <summary>
        /// Exports data as a Newtonsoft JArray
        /// </summary>
        public JArray ToJArray()
        {
            var res = new JArray();

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
        /// Exports csv as a json string
        /// </summary>
        /// <param name="useFormatting">If true, then data gets indented and eas to read, but longer</param>
        public string ToJson(bool useFormatting = false)
        {
            return ToJArray().ToString(useFormatting ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None);
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
        /// Exports data as an ExandoList
        /// </summary>
        public IEnumerable<Dictionary<string, object>> ToExpandoList()
        {
            var list = new List<Dictionary<string, object>>();
            for (int row = RowLimit.Min; row <= RowLimit.Max; row++)
            {
                var isEmpty = true;
                var dict = new Dictionary<string, object>();
                foreach (var header in Headers)
                {
                    if (Records.TryGetValue((row, header.Key), out object value) && value != null)
                        isEmpty = false;

                    dict.Add(header.Value, value);
                }

                if (!isEmpty)
                    list.Add(dict);
            }
            return list;
        }

        /// <param name="row">If r is null, it is a header. Else it is a normal row</param>
        private (StringBuilder StringBuilder, bool IsEmpy) WriteRow(int? row)
        {
            //Every record must have same number of lines
            //It is NOT possible to have line breaks within a record by quoting the sentence
            //Fields with embedded commas or double-quote characters must be quoted: 1997,Ford,E350,"Super, luxurious truck"
            //Each of the embedded double-quote characters is represented by a pair of double-quote characters: 1997,Ford,E350,"Super, ""luxurious"" truck"
            //Trim leading and trailng spaces
            //spaces outside quotes in a field are not allowed
            //Its okay to have a single quote in a record
            var chars = new char[] { '"', ',' };
            var rowObject = new StringBuilder();
            var isEmpty = true;

            for (int col = ColLimit.Min; col <= ColLimit.Max; col++)
            {
                string val;
                if (row != null)
                {
                    if (Records.TryGetValue(((int)row, col), out object recordVal))
                    {
                        if (recordVal == null)
                            val = string.Empty;
                        else if (recordVal.GetType() == typeof(DateTime))
                        {
                            isEmpty = false;
                            val = ((DateTime)recordVal).ToString("s");
                        }
                        else
                        {
                            isEmpty = false;
                            val = recordVal != null ? recordVal.ToString().Trim() : string.Empty;
                        }
                    }
                    else
                        val = string.Empty;
                }
                else
                {
                    if (Headers.TryGetValue(col, out string headerVal))
                        val = headerVal.ToString();
                    else
                        val = "Col_" + col;
                }

                if (val.Contains('\n'))
                    val = val.Replace('\n', ' ');

                if (val.IndexOfAny(chars) != -1)
                    val = "\"" + val.Replace("\"", "\"\"") + "\"";

                if (col < ColLimit.Max)
                    val += ',';

                rowObject.Append(val);
            }
            return (rowObject, isEmpty);
        }

    }
}
