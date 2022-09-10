using Bygdrift.Tools.CsvTool.TimeStacking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

//namespace Bygdrift.Tools.CsvTool.TimeStacking.Models
namespace Bygdrift.Tools.CsvTool.TimeStacking

{
    public partial class TimeStack
    {
        private static readonly Regex csvSplit = new("(?:^|,)(\"(?:[^\"])*\"|[^,]*)", RegexOptions.Compiled);
        //private readonly bool isSerialData;
        //private readonly Csv origCsv;

        internal List<CsvColumn> Columns { get; set; } = new();


        //public TimeStack(bool isSerialData, Csv origCsv)
        //{
        //    this.isSerialData = isSerialData;
        //    this.origCsv = origCsv;
        //}

        private TimeStack AddCol(CsvColumnType stackType, string header, string newHeader, bool isAccumulated, bool mustBeInOrigCsv, string format, double factor)
        {
            int? col = Csv.TryGetColId(header, out int val) ? val : null;
            if (mustBeInOrigCsv && col == null)
                throw new InvalidOperationException($"The header name '{header}' has to be present in the original csv, to make the '{stackType}' operation.");

            Columns.Add(new CsvColumn(Columns.Count + 1, col, col != null ? header : null, newHeader != null ? UniqueHeader(newHeader) : UniqueHeader(header), stackType, isAccumulated, IsSerialData, format, factor));
            return this;
        }

        /// <summary>
        /// If newHeaders are longer than existing, the remain will be ignored
        /// </summary>
        private TimeStack AddCols(CsvColumnType stackType, string existingHeaders, string newHeaders, bool isAccumulated, bool mustBeInOrigCsv, string format, double factor)
        {
            if (existingHeaders == null && newHeaders == null)
                return this;

            var existingH = SplitString(existingHeaders).ToArray();
            var newH = SplitString(newHeaders).ToArray();

            for (int i = 0; i < existingH.Length; i++)
                AddCol(stackType, existingH[i], newH.Length > i ? newH[i] : null, isAccumulated, mustBeInOrigCsv, format, factor);

            return this;
        }

        public TimeStack AddCalcAverage(string existingHeaders, string newHeaders = null, bool isAccumulated = false, double factor = 1)
        {
            return AddCols(CsvColumnType.CalcAverage, existingHeaders, newHeaders, isAccumulated, false, null, factor);
        }

        /// <summary>
        /// Weighted by time in each timeslot. So the longer a certain value is present in a slot, the more weight it gets
        /// </summary>
        public TimeStack AddCalcAverageWeighted(string existingHeaders, string newHeaders = null, bool isAccumulated = false, double factor = 1)
        {
            return AddCols(CsvColumnType.CalcAverageWeighted, existingHeaders, newHeaders, isAccumulated, false, null, factor);
        }

        public TimeStack AddCalcFirst(string existingHeaders, string newHeaders = null, bool isAccumulated = false, double factor = 1)
        {
            return AddCols(CsvColumnType.CalcFirst, existingHeaders, newHeaders, isAccumulated, false, null, factor);
        }

        public TimeStack AddCalcLast(string existingHeaders, string newHeaders = null, bool isAccumulated = false, double factor = 1)
        {
            return AddCols(CsvColumnType.CalcLast, existingHeaders, newHeaders, isAccumulated, false, null, factor);
        }

        public TimeStack AddCalcMax(string existingHeaders, string newHeaders = null, bool isAccumulated = false, double factor = 1)
        {
            return AddCols(CsvColumnType.CalcMax, existingHeaders, newHeaders, isAccumulated, false, null, factor);
        }

        public TimeStack AddCalcMin(string existingHeaders, string newHeaders = null, bool isAccumulated = false, double factor = 1)
        {
            return AddCols(CsvColumnType.CalcMin, existingHeaders, newHeaders, isAccumulated, false, null, factor);
        }

        public TimeStack AddCalcSum(string existingHeaders, string newHeaders = null, bool isAccumulated = false, double factor = 1)
        {
            return AddCols(CsvColumnType.CalcSum, existingHeaders, newHeaders, isAccumulated, false, null, factor);
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="format">Can be: 'From: [:From:YYYY], Data: [Data]'. Will return dynamic content on all that is inside brackets. If no colon, then it's a name on a header. 
        /// If it starts with colon lik [:From], it will return the From date. Can also use [:To]. If [:From:YYYY], then YYYY is the date-format of from</param>
        /// <returns></returns>
        public TimeStack AddInfoFormat(string header, string format)
        {
            return AddCol(CsvColumnType.InfoFormat, header, null, false, false, format, 1);
        }

        public TimeStack AddInfoFrom(string header, string format = null)
        {
            return AddCol(CsvColumnType.InfoFrom, header, null, false, false, format, 1);
        }

        public TimeStack AddInfoGroup(string header, string format = null)
        {
            return AddCol(CsvColumnType.InfoGroup, header, null, false, false, format, 1);
        }


        public TimeStack AddInfoLength(string header, string format = null, double factor = 1)
        {
            return AddCols(CsvColumnType.InfoLength, header, null, false, false, format, factor);
        }

        public TimeStack AddInfoRowCount(string header, string format = null, double factor = 1)
        {
            return AddCol(CsvColumnType.InfoRowCount, header, null, false, false, format, factor);
        }

        public TimeStack AddInfoTo(string header, string format = null)
        {
            return AddCol(CsvColumnType.InfoTo, header, null, false, false, format, 1);
        }

        public TimeStack RemoveCols()
        {
            Columns = new();
            return this;
        }

        private static IEnumerable<string> SplitString(string input)
        {
            if (!string.IsNullOrEmpty(input))
                foreach (Match match in csvSplit.Matches(input.Trim(' ', ',')))
                    yield return match.Value?.TrimStart(',').Trim('"', ' ');
        }

        private string UniqueHeader(string value)
        {
            if (string.IsNullOrEmpty(value))
                value = "col";

            var headers = Columns.Select(o => o.Header);

            var res = value;
            if (headers.Any(o => o.Equals(value, StringComparison.InvariantCulture)))
            {
                var i = 2;
                while (headers.Any(o => o.Equals(res, StringComparison.InvariantCulture)))
                    res = value + "_" + i++;
            }
            return res;
        }
    }
}
