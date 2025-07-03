using Bygdrift.Tools.CsvTool;
using Bygdrift.Tools.CsvTool.TimeStacking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;

namespace CsvToolTests.TimeStacking
{
    [TestClass]
    public class TimeStackParallelTests
    {
        public static readonly string BasePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));

        [TestMethod]
        public void Ex()
        {
            var csvIn = new Csv("From, To")
                .AddRow("2022-1-1 07:40:00", "2022-1-1 07:45:00")
                .AddRow("2022-1-1 07:50:00", "2022-1-1 08:00:00")
                .AddRow("2022-1-1 07:10:00", "2022-1-1 07:40:00")
                .AddRow("2022-1-1 07:05:00", "2022-1-1 07:30:00")
                .AddRow("2022-1-1 07:20:00", "2022-1-1 07:30:00")
                .AddRow("2022-1-1 07:40:00", "2022-1-1 07:50:00");

            var timeStack = new TimeStack(csvIn, null, "From", "To").AddInfoLength("Length");
            var spans = timeStack.GetSpansPerHour();
            var csv = timeStack.GetTimeStackedCsv(spans);

            new DrawDiagram(1200, 400, "SampImag.png").DrawTimeStack(spans, true);
            TraceOutput.Output(timeStack);
            Assert.AreEqual(csv.RowCount, 1);
            Assert.AreEqual(csv.GetRecord(1, 1), 0.91666666666666663);
        }

        [TestMethod]
        public void Ex2()
        {
            var csvIn = new Csv("From, To")
                .AddRow("2022-1-1 06:10:00", "2022-1-1 06:30:00")
                .AddRow("2022-1-1 06:20:00", "2022-1-1 06:30:00")
                .AddRow("2022-1-1 06:00:00", "2022-1-1 06:20:00")
                .AddRow("2022-1-1 07:00:00", "2022-1-1 07:10:00")
                .AddRow("2022-1-1 07:10:00", "2022-1-1 07:20:00")
                .AddRow("2022-1-1 08:10:00", "2022-1-1 08:20:00")
                .AddRow("2022-1-1 08:00:00", "2022-1-1 08:30:00")
                .AddRow("2022-1-1 09:00:00", "2022-1-1 09:10:00")
                .AddRow("2022-1-1 09:00:00", "2022-1-1 09:20:00")
                .AddRow("2022-1-1 10:00:00", "2022-1-1 10:10:00")
                .AddRow("2022-1-1 10:20:00", "2022-1-1 10:30:00")
                .AddRow("2022-1-1 11:00:00", "2022-1-1 12:10:00");

            var timeStack = new TimeStack(csvIn, null, "From", "To").AddInfoLength("Length");
            var spans = timeStack.GetSpansPerHour();
            var csv = timeStack.GetTimeStackedCsv(spans);

            new DrawDiagram(1200, 600, "SampImag.png").DrawTimeStack(spans, false);
            Assert.AreEqual(csv.RowCount, 7);
            Assert.AreEqual(csv.GetRecord(1, 1), 0.5);
            Assert.AreEqual(csv.GetRecord(2, 1), 0.33333333333333331);
            Assert.AreEqual(csv.GetRecord(3, 1), 0.5);
            Assert.AreEqual(csv.GetRecord(4, 1), 0.33333333333333331);
            Assert.AreEqual(csv.GetRecord(5, 1), 0.33333333333333331);
            Assert.AreEqual(csv.GetRecord(6, 1), 1d);
            Assert.AreEqual(csv.GetRecord(7, 1), 0.16666666666666666);
        }

        [TestMethod]
        public void ExGrouped()
        {
            var csvIn = new Csv("From, To, Group")
                .AddRow("2022-1-1 07:40:00", "2022-1-1 07:45:00", "A")
                .AddRow("2022-1-1 07:50:00", "2022-1-1 08:00:00", "A")
                .AddRow("2022-1-1 07:10:00", "2022-1-1 07:40:00", "A")
                .AddRow("2022-1-1 07:05:00", "2022-1-1 07:30:00", "A")
                .AddRow("2022-1-1 07:20:00", "2022-1-1 07:30:00", "B")
                .AddRow("2022-1-1 07:40:00", "2022-1-1 07:50:00", "B");

            var timeStack = new TimeStack(csvIn, "Group", "From", "To").AddInfoLength("Length").AddInfoGroup("Group");
            var spans = timeStack.GetSpansPerHour();
            var csv = timeStack.GetTimeStackedCsv(spans);

            new DrawDiagram(1200, 400, "SampImag.png").DrawTimeStack(spans, true);
            TraceOutput.Output(timeStack);

            Assert.AreEqual(csv.Records.Count, 4);
            Assert.AreEqual(csv.GetRecord(2, 1), 0.33333333333333331);
        }

        [TestMethod]
        public void GetFirst()
        {
            var csvIn = new Csv("From, To, Group, Data")
                .AddRow("2022-1-1 07:00:00", "2022-1-1 08:00:00", "A", "D")
                .AddRow("2022-1-1 09:00:00", "2022-1-1 10:00:00", "A", "D");

            var timeStack = new TimeStack(csvIn, "Group", "From", "To").AddCalcFirstNotNull("Data");
            var spans = timeStack.GetSpansPerHour();
            var csv = timeStack.GetTimeStackedCsv(spans);
            var json = csv.ToJson(true);
            Trace.WriteLine(json);
            Assert.AreEqual("D", csv.GetRecord(2,1));
        }

        [TestMethod]
        public void TimestackPerHour()
        {
            var csvIn = new Csv("From, To, Data, Name")
                .AddRow("2022-1-1 07:30:00", "2022-1-1 09:00:00", 1, "A")
                .AddRow("2022-1-1 08:00:00", "2022-1-1 09:30:00", 1, "B")
                .AddRow("2022-1-1 11:00:00", "2022-1-1 12:00:00", 1, "C")
                .AddRow("2022-1-2 08:00:00", "2022-1-2 11:00:00", 1, "D");

            //var timeStack = new TimeStack(csvIn, "From", "To")
            //    .AddInfoFrom("Start")
            //    .AddInfoTo("End")
            //    .AddInfoRowCount("Rows")
            //    .AddCalcFirst("From")
            //    .AddCalcLast("To")
            //    .AddCalcFirst("Name")
            //    .AddCalcSum("Data", "DataSum")
            //    .AddCalcAverage("Data", "DataAvg");

            //var spans = timeStack.GetSpanGroupsPerHour();
            //new DrawDigram(1200, 800, "SampImag.png").DrawTimeStack(spans, true);
            //TraceOutput.Output(timeStack, 3);
            //var csv = timeStack.GetTimeStackedCsv(spans);
            //Assert.AreEqual(csv.GetRecord(2, "Rows"), 2);
            //Assert.AreEqual(csv.GetRecord(2, "DataSum"), 2d);
            //Assert.AreEqual(csv.GetRecord(2, "DataAvg"), 1d);
            //Assert.AreEqual(csv.GetRecord(2, "Rows"), 2);
            //Assert.AreEqual(csv.GetRecord(3, "Rows"), 1);
            //Assert.AreEqual(csv.GetRecord(4, "Rows"), 0);
            //Assert.AreEqual(csv.GetRecord(5, "Rows"), 1);
            //Assert.AreEqual(csv.GetRecord(6, "Rows"), 0);
            //Assert.AreEqual(csv.GetRecord(28, "Rows"), 1);
        }
    }
}