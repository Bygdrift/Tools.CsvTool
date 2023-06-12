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
        private readonly string headerFrom;
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
            headerFrom = headerNameFrom;
            headerFromCol = VerifyHeader(headerNameFrom).Key;
            headerToCol = VerifyHeader(headerNameTo).Key;
            Rows = CsvToTimeStacRowsNew();
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
            headerFrom = headerNameTimestamp;
            headerFromCol = VerifyHeader(headerNameTimestamp).Key;
            //headerNameFrom = headerNameTimestamp;
            headerToCol = null;
            Rows = CsvToTimeStacRowsNew();
        }

        /// <summary></summary>
        public string GroupHeader { get; }

        internal bool IsSerialData { get; }

        /// <summary>The start of the segragation. Automtically set from finding the first date in csv. Can be overridden</summary>
        public DateTime From
        {
            get { return _from ??= Csv.GetColRecords<DateTime>(headerFromCol, false).Min(o => o.Value); }
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
                    _to = Csv.GetColRecords<DateTime>(col, false).Max(o => o.Value);
                }
                return (DateTime)_to;
            }
            set { _from = value; }
        }

        internal IEnumerable<Row> Rows { get; }

        private List<Row> CsvToTimeStackRows()
        {
            var res = new List<Row>();
            List<KeyValuePair<int, DateTime>> fromRows = Csv.GetColRecords<DateTime>(headerFromCol)?.OrderBy(o => o.Value).ToList();
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

        private List<Row> CsvToTimeStacRowsNew()
        {
            var res = new List<Row>();
            if (GroupHeader != null)
            {
                var groupsRowsDate = Csv.GetColRecordsGrouped<DateTime>(GroupHeader, headerFrom);
                foreach (var groupRowsDate in groupsRowsDate)
                    AddRowDates(res, groupRowsDate.Key, groupRowsDate.Value);
            }
            else
            {
                var rowsDates = Csv.GetColRecords<DateTime>(headerFrom);
                AddRowDates(res, null, rowsDates);
            }
            return res;
        }

        private void AddRowDates(List<Row> res, object group, Dictionary<int, DateTime> rowDates)
        {
            var rd = rowDates.OrderBy(o => o.Value).ToList();
            for (int i = 0; i < rd.Count; i++)
            {
                var from = rd[i].Value;
                if (IsSerialData)  //Then rows only has timestamps
                {
                    if (i + 1 == rd.Count)
                        break;

                    var to = Csv.GetRecord<DateTime>(rd[i].Key + 1, headerFromCol);
                    res.Add(new Row(Csv, rd[i].Key, rd[i].Key + 1, group, from, to));
                }
                else  //Then both From and To are available
                {
                    var to = Csv.GetRecord<DateTime>(rd[i].Key, (int)headerToCol);
                    res.Add(new Row(Csv, rd[i].Key, null, group, from, to));
                }
            }
        }

        /// <summary>Get csv stacked</summary>
        public Csv GetTimeStackedCsv(List<Span> spans, Config csvConfig = null)
        {
            return TimeStackBuilder.GetTimeStackedCsv(this, spans, Columns, csvConfig);
        }

        /// <summary></summary>
        public List<Span> GetSpansPerHour(int fromHour = 0, int toHour = 24, int takeHours = 1, int[] weekDays = null)
        {
            var groups = Csv.GetColRecordsDistinct(GroupHeader);
            var spans = SpanArray.PerHour(From, To, groups, fromHour, toHour, weekDays);

            foreach (var row in Rows)
                foreach (var span in row.Group != null ? spans.Where(o => o.Group.Equals(row.Group)) : spans)
                    span.TryAddRow(row);

            return spans;
        }

        /// <summary></summary>
        public List<Span> GetSpansPerDay(int[] weekDays = null)
        {
            var groups = Csv.GetColRecordsDistinct(GroupHeader);
            var spans = SpanArray.PerDay(From, To, groups, weekDays);

            foreach (var row in Rows)
                foreach (var span in row.Group != null ? spans.Where(o => o.Group.Equals(row.Group)) : spans)
                    span.TryAddRow(row);

            return spans;
        }

        /// <summary></summary>
        public List<Span> GetSpansPerMonth()
        {
            var groups = Csv.GetColRecordsDistinct(GroupHeader);
            var spans = SpanArray.PerMonth(From, To, groups);

            foreach (var row in Rows)
                foreach (var span in row.Group != null ? spans.Where(o => o.Group.Equals(row.Group)) : spans)
                    span.TryAddRow(row);

            return spans;
        }

        private KeyValuePair<int, string> VerifyHeader(string header)
        {
            if (string.IsNullOrEmpty(header))
                throw new ArgumentNullException(nameof(header));

            if (!Csv.TryGetColId(header, out int col))
                throw new Exception("Error in reading headerName. A programmer must take care of this issue.");

            if (Csv.ColTypes[col] != typeof(DateTime) && Csv.ColTypes[col] != typeof(DateTimeOffset))
                throw new Exception($"Error in reading column with headerName '{header}' must only contain 'DateTime' or 'DateTimeOffset', but it is {Csv.ColTypes[col]}.");

            return new KeyValuePair<int, string>(col, header);
        }
    }
}