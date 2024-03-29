﻿using System;

namespace Bygdrift.Tools.CsvTool.TimeStacking.Models
{
    /// <summary></summary>
    public class SpanRow : Row
    {
        private double? _spanInnerSlot;
        private double? _spanOuterSlot;

        /// <summary></summary>
        public SpanRow(Row row, Span span, TimeSpan timeWithinSpan) : base(row.Csv, row.RowNumber, row.RowNumber2, row.Group, row.From, row.To)
        {
            Span = span;
            TimeWithinSpan = timeWithinSpan;
        }

        /// <summary></summary>
        public TimeSpan TimeWithinSpan { get; }

        /// <summary></summary>
        public Span Span { get; }

        /// <summary>
        /// How many percent the 'row presence in timeslot' fills of the whole rowspan.
        /// In other words: Row presence in timeslot divided by row timespan.
        /// </summary>
        public double SpanOuterSlot { get { return _spanOuterSlot ??= TimeWithinSpan / TimeSpan; } }

        /// <summary>How many percent the row is pressent inside timeslot</summary>
        public double SpanInnerSlot { get { return _spanInnerSlot ??= TimeWithinSpan / Span.TimeSpan; } }

    }
}
