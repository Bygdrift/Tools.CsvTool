using System;

namespace Bygdrift.Tools.CsvTool.TimeStacking.Models
{
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

        public int Col { get; }
        public int? CsvCol { get; }
        public string CsvHeader { get; }

        /// <summary>The name of the header</summary>
        public string Header { get; set; }

        /// <summary>The cumulationtype</summary>
        public CsvColumnType ColumnType { get; set; }
        public bool IsAccumulated { get; }
        public bool IsSerialData { get; }
        public string Format { get; }

        public double Factor { get; set; }
    }
}
