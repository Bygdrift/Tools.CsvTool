using System;

namespace Bygdrift.Tools.CsvTool.TimeStacking.Models
{
    /// <summary></summary>
    public class FromTo
    {
        private TimeSpan? _timeSpan;

        /// <summary></summary>
        public FromTo(DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }

        /// <summary></summary>
        public FromTo(Row row)
        {
            From = row.From;
            To = row.To;
        }

        /// <summary></summary>
        public DateTime From { get; set; }

        /// <summary></summary>
        public DateTime To { get; set; }

        /// <summary></summary>
        public TimeSpan TimeSpan { get { return _timeSpan ??= To.Subtract(From); } }

    }
}
