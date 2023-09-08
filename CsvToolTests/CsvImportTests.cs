using Bygdrift.Tools.CsvTool;
using CsvToolTests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
        private Config csvConfig = new Config().AddDateFormats("d M yyyy, d-M-yyyy, d/M/yyyy, d M yyyy H:m:s, d-M-yyyy H:m:s, d/M/yyyy H:m:s");

        [TestMethod]
        public void FromCsv()
        {
            //Tjek at config bliver anvendt i csvIn og kommer videre til csv:
            var csv = new Csv("a,b,c").AddRows("a,b,c", "A,B,C", "AA, BB, CC");
            var csvIn = new Csv(csvConfig, "c,d,date").AddRows("cNew, dNew,26-10-2021 14:43:58", "CNew, DNew");
            csv.AddCsv(csvIn, false);
            Assert.AreEqual(typeof(DateTime), csv.ColTypes[5]);
            Assert.AreEqual(typeof(DateTime), csvIn.ColTypes[3]);

            //Her skal config alene anvendes i csv og ikke i csvIn:
            csv = new Csv(csvConfig, "a,b,c").AddRows("a,b,c", "A,B,C", "AA, BB, CC");
            csvIn = new Csv("c,d,date").AddRows("cNew, dNew,26-10-2021 14:43:58", "CNew, DNew");
            csv.AddCsv(csvIn, false);
            Assert.AreEqual(typeof(DateTime), csv.ColTypes[5]);
            Assert.AreEqual(typeof(string), csvIn.ColTypes[3]);

            Assert.AreEqual(5, csv.ColCount);
            Assert.AreEqual(5, csv.RowCount);

            csv = new Csv("a,b,c").AddRows("a,b,c", "A,B,C", "AA, BB, CC");
            csv.AddCsv(csvIn, true);
            Assert.AreEqual(5, csv.ColCount);
            Assert.AreEqual(5, csv.RowCount);

            csv.AddCsv(null, false);

            csv = new Csv("Id, Name").AddRows("A, Anders");
            csv.AddCsv(new Csv("Id, Name").AddRows("B, Bo"));
            csv.AddCsv(new Csv("Age").AddRows("22"));
            csv.AddCsv(new Csv("Id").AddRows("C"));
            Assert.AreEqual("22", csv.GetRecord<string>(3, 3));
            Assert.AreEqual("C", csv.GetRecord(4, 1));
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

            var csvFromReader = new Csv().AddDataTable(dataTable);
            Assert.IsTrue(csvFromReader.ColLimit.Equals((1, 2)));
            Assert.IsTrue(csvFromReader.RowLimit.Equals((1, 1)));
            Assert.IsTrue(csvFromReader.Records[(1, 1)].Equals(1));
            Assert.IsTrue(csvFromReader.Records[(1, 2)].Equals("text"));
        }

        [TestMethod]
        public void FromExcelXls()
        {
            var filePath = Path.Combine(basePath, "Files", "Excel", "Simple data to csv.xlsx");
            var pane0Csv = new Csv().AddExcelFile(filePath, 0, 3, 1);  //Does not exist
            Assert.AreEqual(pane0Csv.ColLimit, (0, 0));
            Assert.AreEqual(pane0Csv.RowLimit, (0, 0));

            var pane1Csv = new Csv().AddExcelFile(filePath, 1, 3, 1);
            Assert.AreEqual(pane1Csv.ColLimit, (1, 3));
            Assert.AreEqual(pane1Csv.RowLimit, (1, 5));

            var pane2Csv = new Csv().AddExcelFile(filePath, 2, 3, 2);
            Assert.AreEqual(pane2Csv.ColLimit, (1, 3));
            Assert.AreEqual(pane2Csv.RowLimit, (1, 5));

            var pane3Csv = new Csv().AddExcelFile(filePath, 3);  //Is empty
            Assert.AreEqual(pane3Csv.ColLimit, (0, 0));
            Assert.AreEqual(pane3Csv.RowLimit, (0, 0));
        }

        [TestMethod]
        public void FromExcelTable()
        {
            var filePath = Path.Combine(basePath, "Files", "Excel", "simpleExportAsTable.xlsx");
            var pane1Csv = new Csv().AddExcelFile(filePath, 1, 1, 1);
            Assert.AreEqual((1, 2), pane1Csv.ColLimit);
            Assert.AreEqual((1, 2), pane1Csv.RowLimit);
        }

        [TestMethod]
        public void FromExcelRelationalTable()
        {
            var filePath = Path.Combine(basePath, "Files", "Excel", "simpleExportAsRelationalTable.xlsx");
            var pane1Csv = new Csv().AddExcelFile(filePath, 1, 1, 1);
            //Assert.AreEqual((1, 2), pane1Csv.ColLimit);
            //Assert.AreEqual((1, 2), pane1Csv.RowLimit);
        }


        [TestMethod]
        public void FromFile_LinebreaksInHeaderMustFail()
        {
            var filePath = Path.Combine(basePath, "Files", "Csv", "LinebreaksInHeader.csv");
            try
            {
                var csv = new Csv().AddCsvFile(filePath);
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
            var csvConfig = new Config().AddDateFormats("d M yyyy, d-M-yyyy, d/M/yyyy, d M yyyy H:m:s, d-M-yyyy H:m:s, d/M/yyyy H:m:s");
            var filePath = Path.Combine(basePath, "Files", "Csv", "RightFormat.csv");
            var csv = new Csv(csvConfig).AddCsvFile(filePath);
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
            var csv = new Csv().AddCsvFile(filePath);
            Assert.AreEqual(csv.Headers[2], "Height(Inches)");
        }

        [TestMethod]
        public void FromJson()
        {
            var res = new Csv().AddJson("{\"a\":23.9,\"location\":{\"type\":\"Point\",\"coordinates\":[11.266823,55.640562442]},\"motion\":0,\"deviceId\":\"a817e03d633\", \"a\":23.9}", false);
            var output = res.ToCsvString();
            Assert.AreEqual("a,location.type,location.coordinates,motion,deviceId\n23.9,Point,\"11.266823,  55.640562442\",0,a817e03d633\n", output);

            var res2 = new Csv().AddJson("[{\"a\":1,\"b\":2},{\"b\":3},{\"a\":4}]", false);
            var output2 = res2.ToCsvString();
            Assert.AreEqual("a,b\n1,2\n,3\n4,\n", output2);

            var res3 = new Csv().AddRecord(1, "id", "0").AddJson("[{\"a\":1,\"b\":2},{\"b\":3},{\"a\":4}]", false);
            var output3 = res3.ToCsvString();
            Assert.AreEqual("id,a,b\n0,,\n,1,2\n,,3\n,4,\n", output3);

            var res4 = new Csv().AddJson("[{\"a\":1,\"b\":1,\"location\":{\"type\":\"Point\",\"coordinates\":[11.260,55.640562442]}},{\"b\":1},{\"a\":1}]", false);
            var output4 = res4.ToCsvString();
            Assert.AreEqual("a,b,location.type,[0].location.coordinates\n1,1,Point,\"11.26,  55.640562442\"\n,1,,\n1,,,\n", output4);
        }

        [TestMethod]
        public void FromModelList()
        {
            var data = new List<ModelTest>
            {
                new ModelTest{Number = 30, Text = "Test1"},
                new ModelTest{Number = 50, Text = "Test2"},
            };

            var csv = new Csv().AddModel(data);
            var json = csv.ToJson();
            Assert.AreEqual(json, "[{\"Number\":30,\"Text\":\"Test1\"},{\"Number\":50,\"Text\":\"Test2\"}]");
        }

        [TestMethod]
        public void FromStreamWithStartingComma()
        {
            var csv = "a,b\n,B\n";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            var csvFromReader = new Csv().AddCsvStream(stream);
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
            var csvFromReader = new Csv().AddCsvStream(stream, ';');
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

            var csvFromReader = new Csv().AddCsvStream(stream);
            Assert.IsTrue(csvFromReader.ColLimit.Equals((1, 2)));
            Assert.IsTrue(csvFromReader.RowLimit.Equals((1, 1)));
            Assert.IsTrue(csvFromReader.Records[(1, 1)].ToString() == "1");
            Assert.IsTrue(csvFromReader.Records[(1, 2)].Equals("text"));

            //Check null stream:
            Assert.IsTrue(new Csv().AddDataTable(null).ColLimit.Max == 0);
        }
    }
}
