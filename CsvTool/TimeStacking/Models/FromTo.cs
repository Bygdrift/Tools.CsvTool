using System;

namespace Bygdrift.Tools.CsvTool.TimeStacking.Models
{
    public class FromTo
    {
        private TimeSpan? _timeSpan;

        public FromTo(DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }

        public FromTo(Row row)
        {
            From = row.From;
            To = row.To;
        }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public TimeSpan TimeSpan { get { return _timeSpan ??= To.Subtract(From); } }

    }
}
