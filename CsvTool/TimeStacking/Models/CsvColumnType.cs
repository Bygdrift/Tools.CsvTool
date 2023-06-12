namespace Bygdrift.Tools.CsvTool.TimeStacking.Models
{
    /// <summary>
    /// How to accumulate content from an array of values in a given timeslot.
    /// The array is formed by choosing a specific column between the rows in the timeslot 
    /// </summary>
    public enum CsvColumnType
    {
        /// <summary>Calculate average from a chosen column from the rows that are in a current time slot. But only if the records content can be transformed into a double.</summary>
        CalcAverage,
        /// <summary></summary>
        CalcAverageWeighted,
        // <summary>How many percent of the timeslot that are covered by the rows apperances</summary>
        //CalcFill,
        /// <summary></summary>
        CalcFirst,
        /// <summary></summary>
        CalcFirstNotNull,
        /// <summary></summary>
        CalcLast,
        /// <summary></summary>
        CalcMax,
        /// <summary></summary>
        CalcMin,
        /// <summary>Calculate sum from a chosen column from the rows that are in a current time slot. But only if the records content can be transformed into a double.</summary>
        CalcSum,
        /// <summary></summary>
        InfoFrom,
        /// <summary></summary>
        InfoFormat,
        /// <summary></summary>
        InfoGroup,
        /// <summary></summary>
        InfoLength,
        /// <summary></summary>
        InfoRowCount,
        /// <summary></summary>
        InfoTo,
    }
}
