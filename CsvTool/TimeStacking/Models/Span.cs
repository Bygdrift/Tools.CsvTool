using System;
using System.Collections.Generic;
using System.Linq;

namespace Bygdrift.Tools.CsvTool.TimeStacking.Models
{
    /// <summary></summary>
    public class Span
    {
        private TimeSpan? _span;
        private Dictionary<(bool, int?), List<(double Value, double Length, double Avg)>> recordsWeighted = new();
        private List<FromTo> _timesInSpan;
        private double? _timeInSpan;

        /// <summary></summary>
        public Span(TimePartition timePartition, DateTime from, DateTime to, object group)
        {
            TimePartition = timePartition;
            From = from;
            To = to;
            Group = group;
        }

        /// <summary></summary>
        public TimePartition TimePartition { get; }

        /// <summary></summary>
        public DateTime From { get; }

        /// <summary></summary>
        public DateTime To { get; }

        /// <summary></summary>
        public object Group { get; set; }

        /// <summary></summary>
        public TimeSpan TimeSpan { get { return (TimeSpan)(_span ??= _span = To - From); } }

        /// <summary></summary>
        public List<SpanRow> SpanRows { get; set; } = new();

        /// <summary></summary>
        public double GetRecordsWeighted(bool isSerialData, int? col)
        {
            var recs = GetRecordValuessWithinSlot(isSerialData, col);
            var length = recs.Sum(o => o.Length);
            var product = recs.Sum(o => o.Value * o.Length);
            return product / length;
        }

        /// <summary>Timespans for How much time the rows are within this span</summary>
        public List<FromTo> TimesInSpan { get { return _timesInSpan ?? GetTimesinSpan(); } }

        /// <summary>How much time the rows are within this span, returned in hours</summary>
        public double TimeInSpan { get { return _timeInSpan ??= TimesInSpan.Sum(o => o.To.Subtract(o.From).TotalHours); } }

        private List<FromTo> GetTimesinSpan()
        {
            _timesInSpan = new();
            foreach (var row in SpanRows)
            {
                var fromTo = GetFromToWithinSpan(new FromTo(row.From, row.To));

                foreach (var item in _timesInSpan)
                {
                    if (fromTo.From >= item.From && fromTo.To <= item.To)
                    {
                        fromTo.From = fromTo.To;
                        break;
                    }

                    if (item.From <= fromTo.From && item.To >= fromTo.From)
                        fromTo.From = item.To;

                    if (item.From <= fromTo.To && item.To >= fromTo.To)
                        fromTo.To = item.From;
                }
                if (fromTo.From < fromTo.To)
                {
                    var res = _timesInSpan.SingleOrDefault(o => o.To == fromTo.From);
                    if (res != null)
                        res.To = fromTo.To;
                    else
                        _timesInSpan.Add(fromTo);
                }
            }

            return _timesInSpan;
        }

        /// <returns>Value: The record value. Length: The row length within the slot in hours</returns>
        public IEnumerable<(double Value, double Length, double Avg)> GetRecordValuessWithinSlot(bool isSerialData, int? col)
        {
            if (col == null)
                return null;

            if (!recordsWeighted.TryGetValue((isSerialData, col), out List<(double Value, double Length, double Avg)> res))
            {
                if (SpanRows.Count == 0)
                    return null;

                res = new();
                double val;
                double avg;
                double? r2 = null;
                foreach (var spanRow in SpanRows)
                {
                    if (isSerialData)
                    {
                        if (spanRow.RowNumber2 == null)
                            throw new ArgumentException("Rownumber2 has to be set when isSerialData = true");

                        var r1 = r2 == null ? spanRow.GetRecord<double>(spanRow.RowNumber, col) : r2;
                        r2 = spanRow.GetRecord<double>((int)spanRow.RowNumber2, col);
                        val = (double)(r2 - r1) * spanRow.SpanOuterSlot;
                        avg = (double)(r2 + r1) / 2;
                    }
                    else
                    {
                        avg = spanRow.GetRecord<double>(spanRow.RowNumber, col);
                        val = avg * spanRow.SpanOuterSlot;
                    }

                    var length = spanRow.TimeSpan.TotalHours * spanRow.SpanOuterSlot;
                    res.Add((val, length, avg));
                }
                recordsWeighted.Add((isSerialData, col), res);
            }

            return res;
        }

        /// <summary>Try add a row</summary>
        public void TryAddRow(Row row)
        {
            if ((row.From < From && row.To < From) || (row.From > To && row.To > To))
                return;

            var fromTo = GetFromToWithinSpan(new FromTo(row.From, row.To));
            if (fromTo.TimeSpan > new TimeSpan())
                SpanRows.Add(new SpanRow(row, this, fromTo.TimeSpan));
        }

        private FromTo GetFromToWithinSpan(FromTo fromTo)
        {
            var startsBefore = From < fromTo.From;
            var endsAfter = To > fromTo.To;
            var fromRes = startsBefore ? fromTo.From : From;
            var toRes = endsAfter ? fromTo.To : To;
            return new FromTo(fromRes, toRes);
        }
    }
}