using System;

namespace Bygdrift.Tools.CsvTool.TimeStacking.Models
{

    /// <summary></summary>
    public class CsvColumn
    {
        /// <summary>Constructor</summary>
        public CsvColumn(int col, int? csvCol, string csvHeader, string header, CsvColumnType columnType, bool isAccumulated, bool isSerialData, string format, double factor)
        {
            if (csvCol != null && string.IsNullOrEmpty(csvHeader))
                throw new ArgumentNullException(nameof(csvHeader));

            Col = col;
            CsvCol = csvCol;
            CsvHeader = csvHeader;
            Header = header ?? throw new ArgumentNullException(nameof(header));
            ColumnType = columnType;
            IsAccumulated = isAccumulated;
            IsSerialData = isSerialData;
            Format = format;
            Factor = factor;
        }

        /// <summary></summary>
        public int Col { get; }

        /// <summary></summary>
        public int? CsvCol { get; }

        /// <summary></summary>
        public string CsvHeader { get; }

        /// <summary>The name of the header</summary>
        public string Header { get; set; }

        /// <summary>The cumulationtype</summary>
        public CsvColumnType ColumnType { get; set; }

        /// <summary></summary>
        public bool IsAccumulated { get; }

        /// <summary></summary>
        public bool IsSerialData { get; }

        /// <summary></summary>
        public string Format { get; }

        /// <summary></summary>
        public double Factor { get; set; }
    }
}
