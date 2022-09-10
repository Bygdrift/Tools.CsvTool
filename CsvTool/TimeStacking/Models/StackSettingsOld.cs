//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;

//namespace Bygdrift.Tools.CsvTool.Segregator.Models
//{
//    /// <summary>
//    /// A header from a csv. Used to descripe how to accumulate rows in a given timeslot
//    /// </summary>
//    public class StackSettings
//    {
//        private static readonly Regex csvSplit = new("(?:^|,)(\"(?:[^\"])*\"|[^,]*)", RegexOptions.Compiled);

//        /// <param name="sumHeaders">A comma seperated string with names on headers that should be returned summarized over the rows that are in the current time slot. But only if the records content can be transformed into a double.</param>
//        /// <param name="averageHeaders">A comma seperated string with names on headers that should be returned with the average of the rows that are in the current time slot. But only if the records content can be transformed into a double.</param>
//        /// <param name="excludedHeaders"></param>
//        /// <param name="fromHeader">If set, a column will be added with the name and a DateTime value for when this timeslot starts. If there already is a column with the same name, then the originail column will be overridden.</param>
//        /// <param name="toHeader">If set, a column will be added with the name and a DateTime value for when this timeslot ends. If there already is a column with the same name, then the originail column will be overridden.</param>
//        /// <param name="fillHeader">If set, a column will be added with the name and a value of how many percent of the given timeslot, is covered with data from rows.</param>
//        public StackSettings(string sumHeaders = null, string averageHeaders = null, string excludedHeaders = null, string fromHeader = null, string toHeader = null, string fillHeader = null)
//        {
//            SumHeaders = AddIfEmpty(sumHeaders);
//            AverageHeaders = AddIfEmpty(averageHeaders);
//            ExcludedHeaders = AddIfEmpty(excludedHeaders);
//            FromHeader = AddIfEmpty(fromHeader);
//            ToHeader = AddIfEmpty(toHeader);
//            FillHeader = AddIfEmpty(fillHeader);
//        }

//        /// <param>A comma seperated string with names on headers that should be returned summarized over the rows that are in the current time slot. But only if the records content can be transformed into a double.</param>
//        public string SumHeaders { get; set; }
//        /// <param>A comma seperated string with names on headers that should be returned with the average of the rows that are in the current time slot. But only if the records content can be transformed into a double.</param>
//        public string AverageHeaders { get; set; }
//        /// <param></param>
//        public string ExcludedHeaders { get; set; }
//        /// <param>If set, a column will be added with the name and a DateTime value for when this timeslot starts. If there already is a column with the same name, then the originail column will be overridden.</param>
//        public string FromHeader { get; set; }
//        /// <param>If set, a column will be added with the name and a DateTime value for when this timeslot ends. If there already is a column with the same name, then the originail column will be overridden.</param>
//        public string ToHeader { get; set; }
//        /// <param>If set, a column will be added with the name and a value of how many percent of the given timeslot, is covered with data from rows.</param>
//        public string FillHeader { get; set; }

//        /// <summary>
//        /// The return is combined because csv has to be build first
//        /// </summary>
//        internal (Csv Csv, List<StackHeader> StackHeaders) GetEmptyCsvAndStackHeaders(Csv csv)
//        {
//            var csvHeaders = new Dictionary<int, string>(csv.Headers);
//            var stackHeaders = new List<StackHeader>();

//            foreach (var item in SplitString(ExcludedHeaders))
//                if (csvHeaders.ContainsValue(item))
//                    csvHeaders.Remove(csvHeaders.First(o => o.Value == item).Key);

//            if (FromHeader != null)
//            {
//                if (!csvHeaders.ContainsValue(FromHeader))
//                    csvHeaders.Add(csvHeaders.Count+1, FromHeader);
                
//                stackHeaders.Add(new StackHeader(csvHeaders.First(o => o.Value == FromHeader).Key, FromHeader, StackType.None));
//            }

//            if (ToHeader != null)
//            {
//                if (!csvHeaders.ContainsValue(ToHeader))
//                    csvHeaders.Add(csvHeaders.Count + 1, ToHeader);

//                stackHeaders.Add(new StackHeader(csvHeaders.First(o => o.Value == ToHeader).Key, ToHeader, StackType.None));
//            }

//            var avg = SplitString(AverageHeaders);
//            var sum = SplitString(SumHeaders);
//            var exclude = SplitString(ExcludedHeaders);
//            foreach (var item in csvHeaders)
//            {
//                var type = StackType.None;
//                if (sum.Any(o => o == item.Value))
//                    type = StackType.Sum;
//                else if (avg.Any(o => o == item.Value))
//                    type = StackType.Average;
//                else if (exclude.Any(o => o == item.Value))
//                    type = StackType.Exclude;

//                stackHeaders.Add(new StackHeader(item.Key, item.Value, type));
//            }

//            return (new Csv(csvHeaders), stackHeaders);
//        }

//        private static IEnumerable<string> SplitString(string input)
//        {
//            if (!string.IsNullOrEmpty(input))
//                foreach (Match match in csvSplit.Matches(input.Trim(' ', ',')))
//                    yield return match.Value?.TrimStart(',').Trim('"');
//        }

//        private static string AddIfEmpty(string input)
//        {
//            return !string.IsNullOrEmpty(input) ? input : null;
//        }
//    }
//}
