using Bygdrift.Tools.CsvTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace CsvToolTests
{
    [TestClass]
    public class CsvExportTests
    {
        /// <summary>Path to project base</summary>
        private readonly string basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));

        private Config csvConfig = new Config().AddFormats("d M yyyy, d-M-yyyy, d/M/yyyy, d M yyyy H:m:s, d-M-yyyy H:m:s, d/M/yyyy H:m:s");

        [TestMethod]
        public void ToCsvFile()
        {
            var csv = new Csv(csvConfig, "Id, Data, Date");
            csv.AddRecord(1, 1, 1);
            csv.AddRecord(1, 2, "A\nB");
            csv.AddRecord(1, 3, "26-10-2021 14:43:58");
            csv.ToCsvFile(Path.Combine(basePath, "Files", "Csv", "Export", "simpleExport.csv"));
        }

        [TestMethod]
        public void ToCsvFileDate()
        {
            var csv = new Csv(csvConfig, "Date").AddRow("2022-03-10T10:01:53.000+01:00");
            var csvDate = csv.GetRecord<DateTime>(1, 1);
            var json = csv.ToJson(true);
            var newCsv = new Csv().FromJson(json, false);
            var newCsvdate = newCsv.GetRecord(1, 1);
            Assert.AreEqual(csvDate, newCsvdate);
        }

        [TestMethod]
        public void ToCsvStream()
        {
            var csv = new Csv(csvConfig, "Id, Data, Date");
            csv.AddRecord(1, 1, 1);
            csv.AddRecord(1, 2, "A\nB");
            csv.AddRecord(1, 3, "26-10-2021 14:43:58");
            Assert.AreEqual(csv.ColTypes[1], typeof(int));
            Assert.AreEqual(csv.ColTypes[2], typeof(string));
            Assert.AreEqual(csv.ColTypes[3], typeof(DateTime));

            var stream = csv.ToCsvStream();

            var csvFromReader = new Csv().FromCsvStream(stream);
            Assert.IsTrue(csvFromReader.ColLimit.Equals((1, 3)));
            Assert.IsTrue(csvFromReader.RowLimit.Equals((1, 1)));
            Assert.IsTrue(csvFromReader.Records[(1, 1)].Equals(1));
            Assert.IsTrue(csvFromReader.Records[(1, 2)].Equals("A B"));
            Assert.AreEqual(csvFromReader.ColTypes[1], typeof(int));
            Assert.AreEqual(csvFromReader.ColTypes[2], typeof(string));
            Assert.AreEqual(csvFromReader.ColTypes[3], typeof(DateTime));
        }

        [TestMethod]
        public void ToCsvString()
        {
            var csv = new Csv(csvConfig, "Id, Age").AddRow(1, 5.2).AddRow(2, 8);
            var res = csv.ToCsvString();
            Assert.AreEqual(res, "Id,Age\n1,5.2\n2,8\n");

            //Passing double can go from 21 to 24 when converted to string - ex: 0.0000001129498786607691 becomes 1,12949878660769E-07
            //I have build in a try catch, that truncates values that area longer than expected, when converted to a string
            var csv2 = new Csv("A").AddRow(0.0000001129498786607691);
            var csvAsString = csv2.ToCsvString(true);
            Assert.AreEqual(csvAsString, "A                    \n1.129498786607691E-07\n");
        }

        [TestMethod]
        public void ToCsvString_CheckDate()
        {
            var time = new DateTime(2022, 6, 1, 12, 0, 0);

            var csv1 = new Csv(new Config("da-DK", "GMT Standard Time", FormatKind.Universal), "Date").AddRow(time);
            Assert.AreEqual(20, csv1.ColMaxLengths[1]);
            Assert.AreEqual("Date\n2022-06-01T13:00:00Z\n", csv1.ToCsvString());
            Assert.AreEqual(typeof(DateTime), csv1.ColTypes[1]);

            var csv2 = new Csv(new Config("en-US", "Romance Standard Time", FormatKind.Local), "Date").AddRow(time);
            Assert.AreEqual("Date\n2022-06-01T12:00:00\n", csv2.ToCsvString());
            Assert.AreEqual(19, csv2.ColMaxLengths[1]);
            Assert.AreEqual(typeof(DateTime), csv2.ColTypes[1]);

            var csv3 = new Csv(new Config("en-US", "Romance Standard Time", FormatKind.TimeOffsetUTC), "Date").AddRow(time);
            Assert.AreEqual("Date\n2022-06-01T12:00:00+01:00\n", csv3.ToCsvString());
            Assert.AreEqual(25, csv3.ColMaxLengths[1]);
            Assert.AreEqual(typeof(DateTimeOffset), csv3.ColTypes[1]);
        }

        [TestMethod]
        public void ToExcelFile()
        {
            var csv = new Csv("Id, Age").AddRow(1, 5).AddRow(2, 8);
            csv.ToExcelFile(Path.Combine(basePath, "Files", "Excel", "Export", "simpleExport.xlsx"), "Data");
            csv.ToExcelFile(Path.Combine(basePath, "Files", "Excel", "Export", "simpleExportAsTable.xlsx"), "Data", "Table1");
        }

        [TestMethod]
        public void ToExcelStream()
        {
            var csv = new Csv("Id, Age").AddRow(5.2, 1).AddRow(2, 8);
            using var stream = csv.ToExcelStream("Data");
            var csvOut = new Csv().FromExcelStream(stream);
            Assert.AreEqual(csv.ToJson(), csvOut.ToJson());
        }

        [TestMethod]
        public void ToExpandoList()
        {
            var csv = new Csv("Id").AddRecord(1, 1, null).AddRecord(2, 1, 1);
            var res = csv.ToExpandoList();
            var json = JsonConvert.SerializeObject(res);
            Assert.AreEqual("[{\"Id\":null},{\"Id\":1}]", json);
        }

        [TestMethod]
        public void ToJson()
        {
            var csv = new Csv("Id, Age, Name").AddRow(1, 5.2, "Anders").AddRow(2, 8, "Bo");
            var res = csv.ToJson();
            Assert.AreEqual(res, "[{\"Id\":1,\"Age\":5.2,\"Name\":\"Anders\"},{\"Id\":2,\"Age\":8.0,\"Name\":\"Bo\"}]");

            //And with danish culture:
            var csv2 = new Csv(new Config("da-DK"), "Id, Age, Name").AddRow(1, "5,2", "Anders").AddRow(2, 8, "Bo");
            var res2 = csv2.ToJson();
            Assert.AreEqual(res2, "[{\"Id\":1,\"Age\":5.2,\"Name\":\"Anders\"},{\"Id\":2,\"Age\":8.0,\"Name\":\"Bo\"}]");
        }

        [TestMethod]
        public void ToModel()
        {
            var csv = new Csv(csvConfig, "Id, Data, Date");
            csv.AddRecord(1, 1, 1);
            csv.AddRecord(1, 2, "A\nB");
            csv.AddRecord(1, 3, "26-10-2021 14:43:58");

            var res = csv.ToModel<ModelExample>();

            Assert.IsNotNull(res);
            Assert.AreEqual(res.First().Date, csv.GetRecord(1, 3));
        }
    }

    public class ModelExample
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public DateTime Date { get; set; }
    }
}
