using Bygdrift.Tools.CsvTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace CsvToolTests
{
    [TestClass]
    public class CsvImportTests
    {
        /// <summary>Path to project base</summary>
        private readonly string basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));

        [TestMethod]
        public void FromCsv()
        {
            var csv = new Csv("a,b,c").AddRows("a,b,c", "A,B,C", "AA, BB, CC");
            var csvIn = new Csv("c,d").AddRows("cNew, dNew", "CNew, DNew");
            var csvOut = csv.FromCsv(csvIn, false);
            Assert.IsTrue(csv.ColCount == 3);
            Assert.IsTrue(csvOut.ColCount == 4);
            Assert.IsTrue(csvOut.RowCount == 5);

            csv = new Csv("a,b,c").AddRows("a,b,c", "A,B,C", "AA, BB, CC");
            csvOut = csv.FromCsv(csvIn, true);
            Assert.IsTrue(csvOut.ColCount == 5);
            Assert.IsTrue(csvOut.RowCount == 5);

            csv.FromCsv(null, false);
        }

        [TestMethod]
        public void FromDataTable()
        {
            //var ccsv = new Csv("Id, Name").AddRow(1, "Eric").AddRow(2,"Anders");
            //var ccsv = new Csv("Id, Name").AddRows("1, Eric", "2, Anders");
            var csv2 = new Csv().AddHeader(1, "Id").AddHeader(2, "Name").AddRecord(1, 1, 1).AddRecord(1, 2, "Eric").AddRecord(2, 1, 2).AddRecord(2, 2, "Anders");

            var csv = new Csv("Id, Data");
            csv.AddRecord(1, 1, 1);
            csv.AddRecord(1, 2, "text");
            var dataTable = csv.ToDataTable();

            var csvFromReader = new Csv().FromDataTable(dataTable);
            Assert.IsTrue(csvFromReader.ColLimit.Equals((1, 2)));
            Assert.IsTrue(csvFromReader.RowLimit.Equals((1, 1)));
            Assert.IsTrue(csvFromReader.Records[(1, 1)].Equals(1));
            Assert.IsTrue(csvFromReader.Records[(1, 2)].Equals("text"));
        }

        [TestMethod]
        public void FromExcel()
        {
            var filePath = Path.Combine(basePath, "Files", "Excel", "Simple data to csv.xls");
            var pane0Csv = new Csv().FromExcelFile(filePath, 0, 3, 1);  //Does not exist
            Assert.AreEqual(pane0Csv.ColLimit, (0, 0));
            Assert.AreEqual(pane0Csv.RowLimit, (0, 0));

            var pane1Csv = new Csv().FromExcelFile(filePath, 1, 3, 1);
            Assert.AreEqual(pane1Csv.ColLimit, (1, 3));
            Assert.AreEqual(pane1Csv.RowLimit, (1, 5));

            var pane2Csv = new Csv().FromExcelFile(filePath, 2, 3, 2);
            Assert.AreEqual(pane2Csv.ColLimit, (1, 3));
            Assert.AreEqual(pane2Csv.RowLimit, (1, 5));

            var pane3Csv = new Csv().FromExcelFile(filePath, 3);  //Is empty
            Assert.AreEqual(pane3Csv.ColLimit, (0, 0));
            Assert.AreEqual(pane3Csv.RowLimit, (0, 0));
        }

        [TestMethod]
        public void FromFile_LinebreaksInHeaderMustFail()
        {
            var filePath = Path.Combine(basePath, "Files", "Csv", "LinebreaksInHeader.csv");
            try
            {
                var csv = new Csv().FromCsvFile(filePath);
                Assert.Fail("No exception thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "There are linebreaks within two parantheses in the first line in the file and that is an error.");
            }
        }

        [TestMethod]
        public void FromFile_RightFormat()
        {
            var filePath = Path.Combine(basePath, "Files", "Csv", "RightFormat.csv");
            var csv = new Csv().FromCsvFile(filePath);

            Assert.AreEqual(csv.Records[(1, 1)].GetType(), typeof(int));
            Assert.AreEqual(csv.ColTypes[1], typeof(int));
            Assert.AreEqual(csv.Records[(1, 2)].GetType(), typeof(double));
            Assert.AreEqual(csv.ColTypes[2], typeof(double));
            Assert.AreEqual(csv.Records[(1, 3)].GetType(), typeof(long));
            Assert.AreEqual(csv.ColTypes[3], typeof(long));
            Assert.AreEqual(csv.Records[(1, 4)].GetType(), typeof(DateTime));
            Assert.AreEqual(csv.ColTypes[4], typeof(DateTime));
            Assert.AreEqual(csv.Records[(1, 5)].GetType(), typeof(string));
            Assert.AreEqual(csv.ColTypes[5], typeof(string));
        }

        [TestMethod]
        public void FromFile_ParanthesisAndSpacesInHeader()
        {
            var filePath = Path.Combine(basePath, "Files", "Csv", "ParanthesisAndSpacesInHeader.csv");
            var csv = new Csv().FromCsvFile(filePath);
            Assert.AreEqual(csv.Headers[2], "Height(Inches)");
        }

        [TestMethod]
        public void FromJson()
        {
            var res = new Csv().FromJson("{\"a\":23.9,\"location\":{\"type\":\"Point\",\"coordinates\":[11.266823,55.640562442]},\"motion\":0,\"deviceId\":\"a817e03d633\", \"a\":23.9}", false);
            Assert.AreEqual(res.Headers.Count, 5);
            Assert.AreEqual(res.Records.Count, 5);
            Assert.AreEqual(res.GetRecord(1, 3), "11.266823,  55.640562442");
            Assert.AreEqual(res.Records.Last().Key, (1, 5));
            Assert.IsTrue(res.RowLimit == (1, 1));

            var res2 = new Csv().FromJson("[{\"a\":1,\"b\":2},{\"b\":3},{\"a\":4}]", false);
            Assert.IsTrue(res2.RowLimit == (1, 3));

            var res3 = new Csv().AddRecord(1, "id", "0").FromJson("[{\"a\":1,\"b\":2},{\"b\":3},{\"a\":4}]", false);
            Assert.IsTrue(res3.RowLimit == (1, 4));

            var res4 = new Csv().FromJson("[{\"a\":1,\"b\":1,\"location\":{\"type\":\"Point\",\"coordinates\":[11.260,55.640562442]}},{\"b\":1},{\"a\":1}]", false);
            Assert.IsTrue(res4.RowLimit == (1, 3));
            Assert.AreEqual(res4.Records.Count, 6);
        }

        [TestMethod]
        public void FromStreamWithStartingComma()
        {
            var csv = "a,b\n,B\n";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            var csvFromReader = new Csv().FromCsvStream(stream);
            var json = csvFromReader.ToJson();
            Assert.AreEqual(json, "[{\"a\":null,\"b\":\"B\"}]");
        }

        [TestMethod]
        public void FromStreamWithOtherDelimiter()
        {
            var csv = "a;b;c\n";
            csv += "A;B;C\n";
            csv += "1.1;1,1;11\n";

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            var csvFromReader = new Csv().FromCsvStream(stream, ';');
            var json = csvFromReader.ToJson();
            Assert.AreEqual(json, "[{\"a\":\"A\",\"b\":\"B\",\"c\":\"C\"},{\"a\":\"1.1\",\"b\":\"1,1\",\"c\":\"11\"}]");
        }

        [TestMethod]
        public void FromStream()
        {
            var csv = new Csv("Id, Data");
            csv.AddRecord(1, 1, 1);
            csv.AddRecord(1, 2, "text");
            var stream = csv.ToCsvStream();

            var csvFromReader = new Csv().FromCsvStream(stream);
            Assert.IsTrue(csvFromReader.ColLimit.Equals((1, 2)));
            Assert.IsTrue(csvFromReader.RowLimit.Equals((1, 1)));
            Assert.IsTrue(csvFromReader.Records[(1, 1)].ToString() == "1");
            Assert.IsTrue(csvFromReader.Records[(1, 2)].Equals("text"));

            //Check null stream:
            Assert.IsTrue(new Csv().FromDataTable(null).ColLimit.Max == 0);
        }
    }
}
