using Bygdrift.Tools.CsvTool.TimeStacking;
using Bygdrift.Tools.CsvTool.TimeStacking.Helpers;
using Bygdrift.Tools.CsvTool.TimeStacking.Models;
using System.Diagnostics;

namespace CsvToolTests.TimeStacking
{
    public class TraceOutput
    {
        public static void Output(TimeStack timeStack, int? colValue = null)
        {
            if (!Debugger.IsAttached)
                return;

            var spans = timeStack.GetSpansPerHour();
            var csv = timeStack.GetTimeStackedCsv(spans);

            var spanGroups = TimeStackBuilder.GetSpanGroups(timeStack, TimePartition.Hours);
            if (spanGroups != null)
                foreach (var spanGroup in spanGroups)
                    for (int s = 0; s < spanGroup.Value.Count; s++)
                    {
                        var span = spanGroup.Value[s];
                        Write($"Slot {s}: From: {span.From}, To: {span.To}:");
                        WriteTT("rows", span.SpanRows.Count);

                        for (int r = 0; r < spanGroup.Value[s].SpanRows.Count; r++)
                        {
                            var row = span.SpanRows[r];
                            WriteT($"Row {r}:");
                            WriteTT($"From: {row.From}, To: {row.To}");
                            WriteTT("Span", row.TimeSpan);
                            WriteTT("SpanInnerSlot", row.SpanInnerSlot);
                            WriteTT("SpanOuterSlot", row.SpanOuterSlot);
                            if (colValue.HasValue)
                            {
                                WriteTT("Record1", row.GetRecord<double>(row.RowNumber, colValue));
                                if (row.RowNumber2 != null)
                                    WriteTT("Record2", row.GetRecord<double>((int)row.RowNumber2, colValue));
                            }
                        }
                    }

            Write("Result:");
            WriteT(csv.ToJson(true));
        }
        private static void Write(object name, object text = null)
        {
            if (text == null)
                Trace.WriteLine(name);
            else
                Trace.WriteLine(name + ": " + text);
        }

        private static void WriteT(object name, object text = null)
        {
            if (text == null)
                Trace.WriteLine("\t" + name);
            else
                Trace.WriteLine("\t" + name + ": " + text);
        }

        private static void WriteTT(object name, object text = null)
        {
            if (text == null)
                Trace.WriteLine("\t\t" + name);
            else
                Trace.WriteLine("\t\t" + name + ": " + text);
        }
    }
}
