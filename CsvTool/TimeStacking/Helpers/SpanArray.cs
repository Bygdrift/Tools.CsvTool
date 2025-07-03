using Bygdrift.Tools.CsvTool.TimeStacking.Models;
using System;
using System.Collections.Generic;

namespace Bygdrift.Tools.CsvTool.TimeStacking.Helpers
{
    internal class SpanArray
    {
        //internal static Dictionary<object, List<Span>> CreateSpans(TimePartition timePartition, DateTime from, DateTime to, IEnumerable<object> groups = null, int fromHour = 0, int toHour = 24, int takeHours = 1, int[] weekDays = null)
        //{
        //    var res = new Dictionary<object, List<Span>>();
        //    if (groups == null)
        //        res.Add(string.Empty, AddPartitions(timePartition, from, to, null, fromHour, toHour, weekDays));
        //    else
        //        foreach (var group in groups)
        //            res.Add(group, AddPartitions(timePartition, from, to, group, fromHour, toHour, weekDays));

        //    return res;
        //}

        //private static List<Span> AddPartitions(TimePartition timePartition, DateTime from, DateTime to, object headerGroup, int fromHour, int toHour, params int[] weekDays)
        //{
        //    //if (timePartition == TimePartition.Hours)
        //    //    return PerHour(from, to, headerGroup, fromHour, toHour, weekDays);
        //    //if (timePartition == TimePartition.Days)
        //    //    return PerDay(from, to, headerGroup, weekDays);
        //    //if (timePartition == TimePartition.Months)
        //    //    return PerMonth(from, to, headerGroup);
        //    //if (timePartition == TimePartition.Years)
        //    //    return PerYear(from, to, headerGroup);
        //    //else
        //    //    throw new ArgumentException("Not implemented");
        //    return default;
        //}



        //internal static Dictionary<object, List<Span>> PerHourGroup(DateTime from, DateTime to, IEnumerable<object> groups, int fromHour, int toHour, int[] weekDays)
        //{
        //    var spans = PerHour(from, to, fromHour, toHour, weekDays);
        //    return BuildGroup(spans, groups);
        //}


        internal static List<Span> PerHour(DateTime from, DateTime to, DateTime fromLimit, DateTime toLimit, IEnumerable<object> groups = null, int takeHours = 1, int[] weekDays = null)
        {
            var res = new List<Span>();
            fromLimit = fromLimit.Date.AddHours(fromLimit.Hour);
            toLimit = toLimit.Date.AddHours(toLimit.Hour);
            int hour = 0;
            while (true)
            {
                var fromRes = (fromLimit).AddHours(hour);
                var toRes = fromRes.AddHours(1);

                if (fromRes >= fromLimit && toRes <= toLimit)
                {
                    if (groups != null)
                        foreach (var group in groups)
                            res.Add(new Span(TimePartition.Hours, fromRes, toRes, group));
                    else
                        res.Add(new Span(TimePartition.Hours, fromRes, toRes, null));
                }

                if (toRes >= (toLimit))
                    break;

                hour += takeHours;
            }

            return res;
        }

        internal static List<Span> PerHour(DateTime from, DateTime to, int? fromHour = null, int? toHour = null, IEnumerable<object> groups = null, int takeHours = 1, int[] weekDays = null)
        {
            var res = new List<Span>();
            //if (from > to)
            //    return res;

            var startTime = from.Date.AddHours(fromHour != null && fromHour < from.Hour ? (int)fromHour : from.Hour);
            var endTime = startTime.AddHours(toHour != null && toHour > to.Hour ? (int)toHour : to.Hour);
            int hour = 0;
            while (true)
            {
                var fromRes = startTime.AddHours(hour);
                var toRes = fromRes.AddHours(1);

                if ((fromHour == null || fromRes.Hour >= fromHour) && (toHour == null || toRes.Hour <= toHour))
                {
                    if (groups != null)
                        foreach (var group in groups)
                            res.Add(new Span(TimePartition.Hours, fromRes, toRes, group));
                    else
                        res.Add(new Span(TimePartition.Hours, fromRes, toRes, null));
                }

                if (toRes >= endTime)
                    break;

                hour += takeHours;
            }

            return res;
        }

        internal static List<Span> PerDay(DateTime from, DateTime to, IEnumerable<object> groups = null, int[] weekDays = null)
        {
            var res = new List<Span>();
            if (from > to)
                return res;

            var startTime = new DateTime(from.Year, from.Month, from.Day);
            int day = 0;
            while (true)
            {
                var fromRes = startTime.AddDays(day++);
                var toRes = fromRes.AddDays(1);

                if (groups != null)
                    foreach (var group in groups)
                        res.Add(new Span(TimePartition.Days, fromRes, toRes, group));
                else
                    res.Add(new Span(TimePartition.Days, fromRes, toRes, null));

                if (toRes >= to)
                    break;
            }
            return res;
        }

        internal static List<Span> PerMonth(DateTime from, DateTime to, IEnumerable<object> groups = null)
        {
            var res = new List<Span>();
            if (from > to)
                return res;

            var startTime = new DateTime(from.Year, from.Month, 1);
            int month = 0;
            while (true)
            {
                var fromRes = startTime.AddMonths(month++);
                var toRes = fromRes.AddMonths(1);
                if (groups != null)
                    foreach (var group in groups)
                        res.Add(new Span(TimePartition.Months, fromRes, toRes, group));
                else
                    res.Add(new Span(TimePartition.Months, fromRes, toRes, null));

                if (toRes >= to)
                    break;
            }
            return res;
        }

        internal static List<Span> PerYear(DateTime from, DateTime to, IEnumerable<object> groups = null)
        {
            var res = new List<Span>();
            if (from > to)
                return res;

            var startTime = new DateTime(from.Year, from.Month, 1);
            int year = 0;
            while (true)
            {
                var fromRes = startTime.AddYears(year++);
                var toRes = fromRes.AddMonths(1);
                if (groups != null)
                    foreach (var group in groups)
                        res.Add(new Span(TimePartition.Years, fromRes, toRes, group));
                else
                    res.Add(new Span(TimePartition.Years, fromRes, toRes, null));

                if (toRes >= to)
                    break;
            }
            return res;
        }


        //private static List<Timeslot> PerHour(DateTime fromDateTime, DateTime toDateTime, TimeSpan fromTime, TimeSpan toTime, short minuteSlice, bool includeWeekends)
        //{
        //    var res = new List<Timeslot>();
        //    for (var day = fromDateTime.Date; day.Date <= toDateTime.Date; day = day.AddDays(1))
        //        if (includeWeekends || !includeWeekends && (int)day.DayOfWeek < 6)
        //            for (var fromSlice = fromTime; fromSlice < toTime; fromSlice = fromSlice.Add(new TimeSpan(0, minuteSlice, 0)))
        //            {
        //                var toSlice = fromSlice.Add(new TimeSpan(0, minuteSlice, 0));
        //                if (toSlice > toTime)
        //                    toSlice = toTime;

        //                var start = day.Add(fromSlice);
        //                var end = day.Add(toSlice);

        //                if (start >= fromDateTime && end <= toDateTime)
        //                    res.Add(new Timeslot(TimePartition.Hours, start, end));
        //            }
        //    return res;
        //}

        //public static double FactorPerHour(DateTime from, DateTime to, DateTime segmentFrom, DateTime segmentEnd)
        //{
        //    if (segmentFrom < from)
        //        if (segmentEnd > to)
        //            return ((to - from)).TotalMilliseconds / 3600000;
        //        else
        //            return (from - segmentFrom).TotalMilliseconds / 3600000;
        //    if (segmentEnd > to)
        //        return (segmentEnd - to).TotalMilliseconds / 3600000;

        //    return 1;
        //}

    }
}
