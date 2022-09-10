using System;

namespace Bygdrift.Tools.CsvTool.TimeStacking.Models
{
    public class Row
    {
        private TimeSpan? _span;

        public Row(Csv csv, int rowNumber, int? row2, object group, DateTime from, DateTime to)
        {
            From = from;
            To = to;
            Csv = csv;
            RowNumber = rowNumber;
            RowNumber2 = row2;
            Group = group;
        }
        public Csv Csv { get; }

        public int RowNumber { get; }

        /// <summary>Is set if it's an accumulated timestack</summary>
        public int? RowNumber2 { get; }

        public object Group { get; }

        public DateTime From { get; }

        public DateTime To { get; }

        public TimeSpan TimeSpan { get { return (TimeSpan)(_span ??= _span = To - From); } }

        /// <param name="col">The column id. Is nullable because the method that calls this are nullable</param>
        public T GetRecord<T>(int row, int? col)
        {
            if (col == null)
                return default;

            return Csv.GetRecord<T>(row, (int)col);
        }

        public T GetRecord<T>(int row, string header)
        {
            return Csv.GetRecord<T>(row, header);
        }
    }
}