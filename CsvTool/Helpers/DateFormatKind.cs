namespace Bygdrift.Tools.CsvTool
{
    /// <summary>What format datetimes should have</summary>
    public enum DateFormatKind
    {
        /// <summary>Like: 2022-08-17T06:57:03</summary>
        Local,
        /// <summary>Like: 2022-08-17T06:57:03Z</summary>
        Universal,
        /// <summary>Offset with base in UTC like: 2022-08-17T06:57:03+01:00</summary>
        TimeOffsetUTC,
        /// <summary>Offset with Dayligth Saving Time like: 2022-08-17T06:57:03+02:00</summary>
        TimeOffsetDST,
    }
}
