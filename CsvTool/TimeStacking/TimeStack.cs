using Bygdrift.Tools.CsvTool.TimeStacking.Helpers;
using Bygdrift.Tools.CsvTool.TimeStacking.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bygdrift.Tools.CsvTool.TimeStacking
{
    public partial class TimeStack
    {
        internal readonly Csv Csv;
        private readonly int headerFromCol;
        private readonly int? headerToCol;  //Is null if data is in timeserie
        private DateTime? _from;
        private DateTime? _to;

        /// <summary>
        /// If each row has a start and a finish
        /// </summary>
        public TimeStack(Csv csv, string headerGroup, string headerNameFrom, string headerNameTo)
        {
            Csv = csv ?? throw new ArgumentNullException(nameof(csv));
            if (csv.Records.Count == 0)
                throw new ArgumentNullException("There has to be data in the csv");

            GroupHeader = headerGroup;
            IsSerialData = false;
            headerFromCol = VerifyHeader<DateTime>(headerNameFrom).Key;
            headerToCol = VerifyHeader<DateTime>(headerNameTo).Key;
            Rows = CsvToTimeStackRows();
        }

        /// <summary>
        /// If each row only has a timestamp 
        /// Example: Given: Value X has timestamp A, while value y has TimeB. Result: From=A, To=B, Value = Y (if accumulated = Y-X).
        /// There must at least be two rows in CSV
        /// If one stack is loaded with TimeA and TimeB, then if a later stack should be loaded, then TimeB should be applied again to avoid hole in data.
        /// </summary>
        public TimeStack(Csv csv, string headerGroup, string headerNameTimestamp)
        {
            Csv = csv ?? throw new ArgumentNullException(nameof(csv));
            GroupHeader = headerGroup;
            IsSerialData = true;
            headerFromCol = VerifyHeader<DateTime>(headerNameTimestamp).Key;
            //headerNameFrom = headerNameTimestamp;
            headerToCol = null;
            Rows = CsvToTimeStackRows();
        }


        public string GroupHeader { get; }

        internal bool IsSerialData { get; }

        /// <summary>The start of the segragation. Automtically set from finding the first date in csv. Can be overridden</summary>
        public DateTime From
        {
            get { return _from ??= (DateTime)Csv.GetColRecords(headerFromCol, false).Min(o => o.Value); }
            set { _from = value; }
        }

        /// <summary>The end of the segragation. Automtically set from finding the first date in csv. Can be overridden</summary>
        public DateTime To
        {
            get
            {
                if (_to == null)
                {
                    int col = headerToCol != null ? (int)headerToCol : headerFromCol;
                    _to = (DateTime)Csv.GetColRecords(col, false).Max(o => o.Value);
                }
                return (DateTime)_to;
            }
            set { _from = value; }
        }

        internal IEnumerable<Row> Rows { get; }

        private List<Row> CsvToTimeStackRows()
        {
            var res = new List<Row>();
            var fromRows = Csv.GetColRecords<DateTime>(headerFromCol)?.OrderBy(o => o.Value).ToList();
            for (int r = 0; r < Csv.RowLimit.Max; r++)
            {
                var group = GroupHeader != null ? Csv.GetRecord(fromRows[r].Key, GroupHeader) : null;
                if (headerToCol != null)  //Then both From and To are available
                {
                    var to = Csv.GetRecord<DateTime>(fromRows[r].Key, (int)headerToCol);
                    res.Add(new Row(Csv, fromRows[r].Key, null, group, fromRows[r].Value, to));
                }
                else  //Then rows only has timestamps
                {
                    if (r + 1 == Csv.RowLimit.Max)
                        break;

                    res.Add(new Row(Csv, fromRows[r].Key, fromRows[r + 1].Key, group, fromRows[r].Value, fromRows[r + 1].Value));
                }
            }
            return res;
        }

        //public Dictionary<object, List<Span>> GetSpansPerHour(string groupHeader = null, int fromHour = 0, int toHour = 24, int takHours = 1, int[] weekDays = null)
        //{
        //    //if(groupHeader == null)
        //    //{
        //    //    var g = SpanArray.PerHour(From, To)
        //    //}
        //    //var groups = OrigCsv.GetColRecordsDistinct(groupHeader);

        //    //var a = SpanArray.PerHour(From, To, 
        //    return default;
        //}


        public Csv GetTimeStackedCsv(List<Span> spans)
        {
            return TimeStackBuilder.GetTimeStackedCsv(this, spans, Columns);
        }

        //public Csv GetTimeStackedCsv(TimePartition timePartition)
        //{
        //    return TimeStackBuilder.GetTimeStackedCsv(this, timePartition, 0, 24, new int[] {1, 2, 3, 4, 5, 6, 7 });
        //}

        //public Csv GetTimeStackedCsvPerHour(int fromHour = 0, int toHour = 24, int?[] weekDays = null)
        //{
        //    return TimeStackBuilder.GetTimeStackedCsv(this, timePartition, 0, 24, new int[] { 1, 2, 3, 4, 5, 6, 7 });
        //}

        //public Csv GetTimeStackedCsv(TimePartition timePartition, int fromHour, int toHour, params int[] weekDays)
        //{
        //    return TimeStackBuilder.GetTimeStackedCsv(this, timePartition, fromHour, toHour, weekDays);
        //}


        //public Csv GetTimeStackedCsv(TimePartition timePartition, int fromHour, int toHour, params int[] weekDays)
        //{
        //    return TimeStackBuilder.GetTimeStackedCsv(this, timePartition, fromHour, toHour, weekDays);
        //}



        public List<Span> GetSpansPerHour(int fromHour = 0, int toHour = 24, int takeHours = 1, int[] weekDays = null)
        {
            var groups = Csv.GetColRecordsDistinct(GroupHeader);
            var spans = SpanArray.PerHour(From, To, groups, fromHour, toHour, weekDays);

            foreach (var row in Rows)
                foreach (var span in row.Group != null ? spans.Where(o => o.Group.Equals(row.Group)) : spans)
                    span.TryAddRow(row);

            return spans;
        }


        private KeyValuePair<int, string> VerifyHeader<T>(string header)
        {
            if (string.IsNullOrEmpty(header))
                throw new ArgumentNullException(nameof(header));

            if (!Csv.TryGetColId(header, out int col))
                throw new Exception("Error in reading headerName. A programmer must take care of this issue.");

            if (Csv.ColTypes[col] != typeof(T))
                throw new Exception($"Error in reading column with headerName '{header}' must only contain {typeof(T).Name}, but it is {Csv.ColTypes[col]}.");

            return new KeyValuePair<int, string>(col, header);
        }
    }
}