using Bygdrift.Tools.CsvTool;
using Bygdrift.Tools.CsvTool.TimeStacking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace CsvToolTests.TimeStacking
{
    [TestClass]
    public class TimeStackSerialTests
    {
        public static readonly string BasePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));

        [TestMethod]
        public void FormatTests()
        {
            var csvIn = new Csv("Time, Data, Value").AddRow("2022-1-1 01:30:00", 10, 10).AddRow("2022-1-1 02:10:00", 11, 10);
            var timeStack = new TimeStack(csvIn, null, "Time")
                .AddInfoFormat("From", "From: [:From], Data: [Data]")
                .AddInfoFormat("To", "To: [:To:yyyy-MM]");

            var spans = timeStack.GetSpansPerHour();
            var csv = timeStack.GetTimeStackedCsv(spans);
            Assert.AreEqual(csv.GetRecord(1, 1), "From: 2022-01-01T01:00:00, Data: 10");
            Assert.AreEqual(csv.GetRecord(1, 2), "To: 2022-01");
        }

        
        [TestMethod]
        public void SerielData3()
        {
            var csvIn = new Csv("Time, Group, Data")
                .AddRow("2022-1-1 23:50:00","A", 10)
                .AddRow("2022-1-2 00:30:00", "A",  20)
                .AddRow("2022-1-1 23:50:00", "B", 34)
                .AddRow("2022-1-2 00:30:00", "B",  35);

            var timeStack = new TimeStack(csvIn, "Group", "Time")
                .AddInfoFormat("Info", "Group: [:Group], From: [:From:dd-HH]")
                .AddCalcAverage("Data", "Avg", true)
                .AddCalcSum("Data", "Sum", true);

            var spans = timeStack.GetSpansPerHour();
            new DrawDigram(1200, 300, "SampImag.png").DrawTimeStack(spans, false);
            var csv = timeStack.GetTimeStackedCsv(spans);
            TraceOutput.Output(timeStack, 3);
        }


        [TestMethod]
        public void SerielDataAverageWeighted()
        {
            var csvIn = new Csv("Time, Data")
                .AddRow("2022-1-1 01:00:00", 10)
                .AddRow("2022-1-1 01:10:00", 11)
                .AddRow("2022-1-1 01:30:00", 13);

            var timeStack = new TimeStack(csvIn, null, "Time")
                .AddCalcAverage("Data", "DataAvg")
                .AddCalcAverageWeighted("Data", "DataAvgW");

            var spans = timeStack.GetSpansPerHour();
            new DrawDigram(1200, 300, "SampImag.png").DrawTimeStack(spans, false);
            var csv = timeStack.GetTimeStackedCsv(spans);
            Assert.AreEqual(csv.GetRecord(1, 1), 11.25d);
            Assert.AreEqual(csv.GetRecord(1, 2), 1.6666666666666665d);
        }

        [TestMethod]
        public void SerielDataAverageWeightedWithCsvConfig()
        {
            var config = new Config(null, "Central America Standard Time", FormatKind.TimeOffsetUTC);
            //Assert.AreEqual(typeof(DateTimeOffset), csvIn.ColTypes[1]);

            var csvIn = new Csv(config, "Time, Data")
                .AddRow("2022-1-1 01:00:00", 10)
                .AddRow("2022-1-1 01:10:00", 11)
                .AddRow("2022-1-1 01:30:00", 13);

            var timeStack = new TimeStack(csvIn, null, "Time")
                .AddCalcAverage("Data", "DataAvg")
                .AddCalcAverageWeighted("Data", "DataAvgW")
                .AddInfoFrom("Fra");

            var spans = timeStack.GetSpansPerHour();
            new DrawDigram(1200, 300, "SampImag.png").DrawTimeStack(spans, false);
            var csv = timeStack.GetTimeStackedCsv(spans, config);
            Assert.AreEqual(csv.GetRecord(1, 1), 11.25d);
            Assert.AreEqual(csv.GetRecord(1, 2), 1.6666666666666665d);
        }

        [TestMethod]
        public void SerielDataToPerHour3()  //Has an interesting diagram
        {
            var csvIn = new Csv("Time, Data")
                .AddRow("2022-1-1 07:50:00", 10)
                .AddRow("2022-1-1 09:10:00", 11)
                .AddRow("2022-1-1 09:20:00", 12);

            var timeStack = new TimeStack(csvIn, null, "Time")
              .AddInfoRowCount("Rows")
              .AddCalcSum("Data", "DataSum")
              .AddCalcAverage("Data", "DataAvg")
              .AddCalcAverageWeighted("Data", "DataAvgW");

            var spans = timeStack.GetSpansPerHour();
            new DrawDigram(600, 500, "SampImag.png").DrawTimeStack(spans, false);
            var csv = timeStack.GetTimeStackedCsv(spans);
            TraceOutput.Output(timeStack, 2);
            Assert.AreEqual(csv.GetColRecords<double>(2).Sum(o => o.Value), 43d);
        }

        [TestMethod]
        public void SerielDataToPerHour2()
        {
            var csvIn = new Csv("Time, Data")
                .AddRow("2022-1-1 00:00:00", 10)
                .AddRow("2022-1-1 01:10:00", 11)
                .AddRow("2022-1-1 01:20:00", 12);

            var timeStack = new TimeStack(csvIn, null, "Time")
              .AddInfoRowCount("Rows")
              .AddCalcSum("Data", "DataSum", true)
              .AddCalcSum("Data", "DataSum2", false)
              .AddCalcAverage("Data", "DataAvg", true)
              .AddCalcAverage("Data", "DataAvg2", false)
              .AddCalcAverageWeighted("Data", "DataAvgW");

            var spans = timeStack.GetSpansPerHour();
            new DrawDigram(1200, 600, "SampImag.png").DrawTimeStack(spans, false);
            var csv = timeStack.GetTimeStackedCsv(spans);
            TraceOutput.Output(timeStack, 2);
            Assert.AreEqual(csv.GetColRecords<double>(2).Sum(o => o.Value), 2);
        }

        [TestMethod]
        public void SerielDataToPerHour1()
        {
            var csvIn = new Csv("Time, Data, Value")
                .AddRow("2022-1-1 01:30:00", 10, 10)
                .AddRow("2022-1-1 02:10:00", 11, 10);

            var timeStack = new TimeStack(csvIn, null, "Time")
                .AddInfoRowCount("Rows")
                .AddCalcSum("Data", "DataSum", true)
                .AddCalcAverage("Data", "DataAvg", true)
                .AddCalcAverage("Value", "ValueAvg", false)
                .AddCalcAverageWeighted("Data", "DataAvgW", true);

            var spans = timeStack.GetSpansPerHour();
            new DrawDigram(1200, 600, "SampImag.png").DrawTimeStack(spans, false);
            var csv = timeStack.GetTimeStackedCsv(spans);
            TraceOutput.Output(timeStack, 2);
            Assert.AreEqual(csv.GetRecord(1, 2), 0.75d);
            Assert.AreEqual(csv.GetRecord(2, 2), 0.25d);
        }

        [TestMethod]
        public void DataFromHFORS()
        {
            var csvFromFtp = new Csv().AddCsvFile(Path.Combine(BasePath, "Files", "HFORS", "In", "csvFromFtp.csv"));
            var csvFromFtpFiltered = csvFromFtp.FilterRows("Målernummer", true, 65674941);
            csvFromFtpFiltered.ToCsvFile(Path.Combine(BasePath, "Files", "HFORS", "Out", "csvFromFtpFiltered.csv"));

            var timeStack = new TimeStack(csvFromFtp, "Målernummer", "Aflæst")
              .AddInfoFrom("Start").AddInfoTo("End")
              .AddInfoRowCount("Rows")
              .AddCalcFirst("Aflæst", "From")
              .AddCalcLast("Aflæst", "To")
              .AddCalcFirst("Energi_Værdi", "Energi_Værdi_First")
              .AddCalcSum("Energi_Værdi", "Sum")
              .AddCalcAverage("Energi_Værdi", "DataAvg")
              .AddCalcAverageWeighted("Energi_Værdi", "DataAvgW");

            var spans = timeStack.GetSpansPerHour();
            var csv = timeStack.GetTimeStackedCsv(spans);
            //new DrawDigram(3000, 4000, "SampImag.png").DrawTimeStack(spans, true);
            Assert.AreEqual(csv.GetRecord(1, 7), 34.5195);
            ////csv.ToCsvFile(Path.Combine(BasePath, "Files", "HFORS", "Out", "Out.csv"));
        }
    }
}
