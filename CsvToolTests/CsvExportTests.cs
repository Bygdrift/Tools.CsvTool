using Bygdrift.Tools.CsvTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CsvToolTests
{
    [TestClass]
    public class CsvExportTests
    {
        /// <summary>Path to project base</summary>
        private readonly string basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));

        [TestMethod]
        public void ToCsvFile()
        {
            var csv = new Csv("Id, Data, Date");
            csv.AddRecord(1, 1, 1);
            csv.AddRecord(1, 2, "A\nB");
            csv.AddRecord(1, 3, "26-10-2021 14:43:58");
            csv.ToCsvFile(Path.Combine(basePath, "Files", "Csv", "Export", "simpleExport.csv"));
        }

        [TestMethod]
        public void ToCsvFileDate()
        {
            var csv = new Csv("Date").AddRow("2022-03-10T10:01:53.000+01:00");
            var csvDate = csv.GetRecord<DateTime>(1, 1);
            var json = csv.ToJson(true);
            var newCsv = new Csv().FromJson(json, false);
            var newCsvdate = newCsv.GetRecord(1, 1);
            Assert.AreEqual(csvDate, newCsvdate);
        }

        [TestMethod]
        public void ToCsvStream()
        {
            var csv = new Csv("Id, Data, Date");
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
            var csv = new Csv("Id, Age").AddRow(1, 5.2).AddRow(2, 8);
            var res = csv.ToCsvString();
            Assert.AreEqual(res, "Id,Age\n1,5.2\n2,8\n");

            //Passing double can go from 21 to 24 when converted to string - ex: 0.0000001129498786607691 becomes 1,12949878660769E-07
            //I have build in a try catch, that truncates values that area longer than expected, when converted to a string
            var csv2 = new Csv("A").AddRow(0.0000001129498786607691);
            var csvAsString = csv2.ToCsvString(true);
            Assert.AreEqual(csvAsString, "A                    \n1.129498786607691E-07\n");
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
            var csv = new Csv("Id, Age").AddRow(1, 5.2).AddRow(2, 8);
            using var stream = csv.ToExcelStream("Data");
            var csvOut = new Csv().FromExcelStream(stream);
            Assert.AreEqual(csv.ToJson(), csvOut.ToJson());
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
    }
}
