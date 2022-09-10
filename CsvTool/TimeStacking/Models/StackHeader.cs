namespace Bygdrift.Tools.CsvTool.TimeStacking.Models
{
    /// <summary>
    /// A header from a csv. Used to descripe how to accumulate rows in a given timeslot
    /// </summary>
    internal class StackHeader
    {

        /// <summary>Constructor</summary>
        public StackHeader(int column, string name, CsvColumnType type)
        {
            Column = column;
            Name = name;
            Type = type;
        }

        /// <summary>The number of the column</summary>
        public int Column { get; set; }

        /// <summary>The name of the header</summary>
        public string Name { get; set; }

        /// <summary>The cumulationtype</summary>
        public CsvColumnType Type { get; set; }

    }
}
