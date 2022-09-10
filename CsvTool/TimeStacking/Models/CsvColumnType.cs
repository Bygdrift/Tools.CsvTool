namespace Bygdrift.Tools.CsvTool.TimeStacking.Models
{
    /// <summary>
    /// How to accumulate content from an array of values in a given timeslot.
    /// The array is formed by choosing a specific column between the rows in the timeslot 
    /// </summary>
    public enum CsvColumnType
    {
        /// <summary>Calculate average from a chosen column from the rows that are in a current time slot. But only if the records content can be transformed into a double.</summary>
        /// <summary>How many percent of the timeslot that are covered by the rows apperances</summary>
        /// <summary>Calculate sum from a chosen column from the rows that are in a current time slot. But only if the records content can be transformed into a double.</summary>
        CalcAverage,
        CalcAverageWeighted,
        CalcFill,
        CalcFirst,
        CalcLast,
        CalcMax,
        CalcMin,
        CalcSum,
        InfoFrom,
        InfoFormat,
        InfoGroup,
        InfoLength,
        InfoRowCount,
        InfoTo,
    }
}
