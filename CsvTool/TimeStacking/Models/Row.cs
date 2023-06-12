using System;

namespace Bygdrift.Tools.CsvTool.TimeStacking.Models
{
    /// <summary></summary>
    public class Row
    {
        private TimeSpan? _span;

        /// <summary></summary>
        public Row(Csv csv, int rowNumber, int? row2, object group, DateTime from, DateTime to)
        {
            From = from;
            To = to;
            Csv = csv;
            RowNumber = rowNumber;
            RowNumber2 = row2;
            Group = group;
        }

        /// <summary></summary>
        public Csv Csv { get; }

        /// <summary></summary>
        public int RowNumber { get; }

        /// <summary>Is set if it's an accumulated timestack</summary>
        public int? RowNumber2 { get; }

        /// <summary></summary>
        public object Group { get; }

        /// <summary></summary>
        public DateTime From { get; }

        /// <summary></summary>
        public DateTime To { get; }

        /// <summary></summary>
        public TimeSpan TimeSpan { get { return (TimeSpan)(_span ??= _span = To - From); } }

        /// <param name="row"></param>
        /// <param name="col">The column id. Is nullable because the method that calls this are nullable</param>
        public T GetRecord<T>(int row, int? col)
        {
            if (col == null)
                return default;

            return Csv.GetRecord<T>(row, (int)col);
        }

        /// <summary></summary>
        public T GetRecord<T>(int row, string header)
        {
            return Csv.GetRecord<T>(row, header);
        }
    }
}