using Bygdrift.Tools.CsvTool.TimeStacking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("CsvToolTests")]
namespace Bygdrift.Tools.CsvTool.TimeStacking.Helpers
{

    /// <summary>Splits a csv up into a desired time pertition</summary>
    internal class TimeStackBuilder
    {

        ///// <summary>
        ///// Finds the oldest and newest value and builds a csv with rows for each hour in that timespan. It can also be for each day, month, year - depending on timepartition.
        ///// </summary>
        ///// <param name="timePartition">What segregation to use</param>
        ///// <param name="settings"></param>
        //public static Csv GetTimeStackedCsv(TimeStack stack, TimePartition timePartition, int fromHour, int toHour, int takeHours, params int[] weekDays)
        //{
        //    var spans = GetSpanGroups(stack, timePartition, fromHour, toHour, takeHours, weekDays);
        //    return GetTimeStackedCsv(stack, spans);
        //}

        /// <summary>
        /// Finds the oldest and newest value and builds a csv with rows for each hour in that timespan. It can also be for each day, month, year - depending on timepartition.
        /// </summary>
        public static Csv GetTimeStackedCsv(TimeStack stack, List<Span> spans, List<CsvColumn> csvColumns, Config csvConfig = null)
        {
            var csv = new Csv(csvConfig, csvColumns.Select(o => o.Header).ToArray());
            var rowId = 1;
            foreach (var span in spans)
            {
                foreach (var col in csvColumns)
                {
                    var res = stackTypes[col.ColumnType](stack, span, col);
                    csv.AddRecord(rowId, col.Col, res);
                }
                rowId++;
            }
            return csv;
        }

        public static Dictionary<object, List<Span>> GetSpanGroups(TimeStack timeStack, TimePartition timePartition, int fromHour = 0, int toHour = 24, int takeHours = 1, int[] weekDays = null)
        {
            //var groups = timeStack.OrigCsv.GetColRecordsDistinct(timeStack.HeaderGroup);

            //var spanGroups = SpanArray.CreateSpans(timePartition, timeStack.From, timeStack.To, groups, fromHour, toHour, takeHours, weekDays);

            //foreach (var row in timeStack.TimeStackRows)
            //{
            //    var spans = row.Group != null && spanGroups.ContainsKey(row.Group) ? spanGroups[row.Group] : spanGroups.First().Value;
            //    foreach (var span in spans)
            //        span.TryAddRow(row);
            //}
            //return spanGroups;
            return default;
        }



        private static object CalcAverage(TimeStack stack, Span span, CsvColumn column)
        {
            var recValues = span.GetRecordValuessWithinSlot(column.IsSerialData, column.CsvCol);
            return recValues?.Average(o => column.IsAccumulated ? o.Value : o.Avg) * column.Factor;
        }

        private static object CalcAverageWeighted(TimeStack stack, Span span, CsvColumn column)
        {
            return span.SpanRows.Count > 0 ? span.GetRecordsWeighted(column.IsSerialData, column.CsvCol) * column.Factor : 0;
        }

        private static object CalcFirst(TimeStack stack, Span span, CsvColumn column)
        {
            var row = span.SpanRows.FirstOrDefault();
            if (column.Factor == 1)
                return row?.GetRecord<object>(row.RowNumber, column.CsvCol);
            else
                return row?.GetRecord<double>(row.RowNumber, column.CsvCol) * column.Factor;
        }

        private static object CalcFirstNotNull(TimeStack stack, Span span, CsvColumn column)
        {
            if (column?.CsvCol != null)
            {
                //var a = stack.Csv.GetColRecords(o => o.Key == stack.GroupHeader && o.Value == span.Group);
                var records = stack.Csv.GetRowRecordsFirstMatch(stack.GroupHeader, span.Group, true, true);
                var col = stack.Csv.Headers.First(o => o.Value == column.Header).Key;
                return records[col];
            }
            return default;
        }

        private static object CalcLast(TimeStack stack, Span span, CsvColumn column)
        {
            var row = span.SpanRows.LastOrDefault();
            if (column.Factor == 1)
                return row?.GetRecord<object>(column.IsSerialData ? (int)row.RowNumber2 : row.RowNumber, column.CsvCol);
            else
                return row?.GetRecord<double>(column.IsSerialData ? (int)row.RowNumber2 : row.RowNumber, column.CsvCol) * column.Factor;
        }

        private static object CalcMin(TimeStack stack, Span span, CsvColumn column)
        {
            return span.SpanRows.Count > 0 ? span.GetRecordValuessWithinSlot(column.IsSerialData, column.CsvCol).Min().Value * column.Factor : 0;
        }

        private static object CalcMax(TimeStack stack, Span span, CsvColumn column)
        {
            return span.SpanRows.Count > 0 ? span.GetRecordValuessWithinSlot(column.IsSerialData, column.CsvCol).Max().Value * column.Factor : 0;
        }

        private static object CalcSum(TimeStack stack, Span span, CsvColumn column)
        {
            var vals = span.GetRecordValuessWithinSlot(column.IsSerialData, column.CsvCol);
            if (vals == null)
                return 0d;

            return vals.Sum(o => column.IsAccumulated ? o.Value : o.Avg) * column.Factor;
        }

        //private static object CalcFill(TimeStack stack, Span span, CsvColumn column)
        //{
        //    return default;
        //}

        private static object InfoFrom(TimeStack stack, Span span, CsvColumn column)
        {
            return span.From;
        }

        private static object InfoFormat(TimeStack stack, Span span, CsvColumn column)
        {
            var res = "";
            var matches = Regex.Matches(column.Format, @"\[(.*?)\]");

            if (matches.Count > 0)
            {
                var index = 0;
                foreach (Match m in matches)
                {
                    res += column.Format.Substring(index, m.Index - index);
                    index = m.Index + m.Length;

                    var bracketContent = m.Groups[1].ToString().Trim();
                    if (bracketContent.StartsWith(':'))  //:From
                    {
                        string colonContentStart = bracketContent.Trim(':');
                        string colonContentEnd = null;
                        if (colonContentStart.Contains(':'))
                        {
                            colonContentEnd = colonContentStart.Split(':')[1];
                            colonContentStart = colonContentStart.Split(':')[0];
                        }

                        if (colonContentStart == "From")
                            res += span.From.ToString(colonContentEnd ?? "s");
                        else if (colonContentStart == "Group")
                            res += span.Group;
                        else if (colonContentStart == "To")
                            res += span.To.ToString(colonContentEnd ?? "s");

                    }
                    else  //Then it's a header
                    {
                        var row = span.SpanRows.FirstOrDefault();
                        if (row != null)
                            res += row.GetRecord<string>(row.RowNumber, bracketContent);
                    }
                }
            }
            else
                res = column.Format;

            return res;
        }

        private static object InfoGroup(TimeStack stack, Span span, CsvColumn column)
        {

            return span.Group;
        }

        private static object InfoLength(TimeStack stack, Span span, CsvColumn column)
        {
            return span.TimeInSpan * column.Factor;
        }

        private static object InfoRowCount(TimeStack stack, Span span, CsvColumn column)
        {
            return span.SpanRows.Count * column.Factor;
        }

        private static object InfoTo(TimeStack stack, Span span, CsvColumn column)
        {
            return span.To;
        }

        private static readonly Dictionary<CsvColumnType, Func<TimeStack, Span, CsvColumn, object>> stackTypes = new()
        {
            { CsvColumnType.CalcFirstNotNull, CalcFirstNotNull },
            { CsvColumnType.CalcAverage, CalcAverage },
            { CsvColumnType.CalcAverageWeighted, CalcAverageWeighted },
            //{ CsvColumnType.CalcFill, CalcFill },
            { CsvColumnType.CalcFirst, CalcFirst },
            { CsvColumnType.CalcLast, CalcLast },
            { CsvColumnType.CalcMax, CalcMax },
            { CsvColumnType.CalcMin, CalcMin },
            { CsvColumnType.CalcSum, CalcSum },
            { CsvColumnType.InfoFrom, InfoFrom },
            { CsvColumnType.InfoFormat, InfoFormat },
            { CsvColumnType.InfoGroup, InfoGroup },
            { CsvColumnType.InfoLength, InfoLength },
            { CsvColumnType.InfoRowCount, InfoRowCount },
            { CsvColumnType.InfoTo, InfoTo },
        };
    }
}