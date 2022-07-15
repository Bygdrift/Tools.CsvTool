using Bygdrift.Tools.CsvTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CsvToolTests
{
    [TestClass]
    public class CsvSetTests
    {
        [TestMethod]
        public void AddHeaders2()
        {
            var csv = new Csv().AddHeaders("A,B");
            Assert.IsTrue(csv.Headers[1] == "A");
            Assert.IsTrue(csv.Headers[2] == "B");

            csv = new Csv().AddHeader("A").AddHeaders("B,C");
            Assert.IsTrue(csv.Headers[1] == "A");
            Assert.IsTrue(csv.Headers[2] == "B");
            Assert.IsTrue(csv.Headers[3] == "C");
        }

        [TestMethod]
        public void AddHeaders()
        {
            var csv = new Csv();
            var headers = new Dictionary<int, string>
            {
                { 1, "A" },
                { 2, "B" },
                { 3, "A" }
            };
            csv.AddHeaders(headers);
            Assert.IsTrue(csv.Headers[3] == "A_2");

            csv = new Csv();
            csv.AddRecord(1, 1, "21");
            csv.AddHeader(1, "Container");  //Ending with adding rows to provoke an eventual error
            var stream = csv.ToCsvStream();
            Assert.IsTrue(stream.Length == 18);
        }

        [TestMethod]
        public void AddRecordByHeaderName()
        {
            var csv = new Csv();
            csv.AddRecord(1, "A", null);
            Assert.AreEqual(csv.ColTypes[1], null);
            csv.AddRecord(2, "A", "21");
            Assert.AreEqual(csv.ColTypes[1], typeof(int));
            csv.AddRecord(3, "A", "text");
            Assert.AreEqual(csv.ColTypes[1], typeof(string));
            csv.AddRecord(4, "A", "21");
            Assert.AreEqual(csv.ColTypes[1], typeof(string));

            csv.AddRecord(1, "B", "a");
            csv.AddRecord(2, "B", "ab");
            csv.AddRecord(3, "B", "a");
            csv.AddRecord(1, "C", "abc");
            Assert.AreEqual(csv.ColLimit, (1, 3));
            Assert.AreEqual(csv.RowLimit, (1, 4));
        }

        [TestMethod]
        public void AddRecord()
        {
            var csv = new Csv("Number,Text");
            csv.AddRecord(1, 1, null);
            Assert.AreEqual(csv.ColTypes[1], null);
            csv.AddRecord(2, 1, "21");
            Assert.AreEqual(csv.ColTypes[1], typeof(int));
            csv.AddRecord(3, 1, "text");
            Assert.AreEqual(csv.ColTypes[1], typeof(string));
            csv.AddRecord(4, 1, "21");
            Assert.AreEqual(csv.ColTypes[1], typeof(string));

            csv.AddRecord(1, 3, "abc");  //Col 2 does not exist on purpose
            csv.AddRecord(1, 2, "a");
            csv.AddRecord(2, 2, "ab");
            csv.AddRecord(3, 2, "a");

            csv.AddRecord(1, 4, "26-10-2021 14:43:58");
            Assert.AreEqual(csv.ColTypes[4], typeof(DateTime));
            csv.AddRecord(2, 4, null);
            Assert.AreEqual(csv.ColTypes[4], typeof(DateTime));
            csv.AddRecord(3, 4, string.Empty);
            Assert.AreEqual(csv.ColTypes[4], typeof(DateTime));
            csv.AddRecord(4, 4, "");
            Assert.AreEqual(csv.ColTypes[4], typeof(DateTime));
        }

        [TestMethod]
        public void AddRow()
        {
            var csv = new Csv("Number,Text");
            csv.AddRow(1, "a");
            Assert.AreEqual(csv.ColTypes[1], typeof(int));
            Assert.AreEqual(csv.ColTypes[2], typeof(string));
            Assert.AreEqual(csv.GetRecord(1, 1), 1);
            Assert.AreEqual(csv.GetRecord(1, 2), "a");
            csv.AddRow(2);
            Assert.AreEqual(csv.GetRecord(2, 1), 2);
            csv.AddRow("b");
            Assert.AreEqual(csv.GetRecord(3, 1), "b");
            Assert.AreEqual(csv.ColTypes[1], typeof(string));
        }

        [TestMethod]
        public void AddNullIntRecord()
        {
            var csv = new Csv("Number");
            csv.AddRecord(1, 1, "");
            csv.AddRecord(2, 1, 1);
            csv.AddRecord(3, 1, null);
            Assert.AreEqual(csv.ColTypes[1], typeof(int));
        }

        [TestMethod]
        public void AddDecimal()
        {
            var cultureInfo =  CultureInfo.InvariantCulture;
            decimal d1 =  1.4M;
            decimal.TryParse(d1.ToString(cultureInfo.NumberFormat), NumberStyles.Any, cultureInfo.NumberFormat, out decimal d1ToStringToDecimal);
            Assert.AreEqual(d1, d1ToStringToDecimal);

            string d2 = "800.41549497473432";
            decimal d2AsDecimal = decimal.Parse(d2, cultureInfo);

            var csv = new Csv("a,b");
            csv.AddRecord(1, 1, d1);
            csv.AddRecord(1, 2, d2);
            csv.AddRecord(1, 3, null);
            Assert.AreEqual(csv.ColTypes[1], typeof(decimal));
            Assert.AreEqual(csv.ColTypes[2], typeof(decimal));
            Assert.AreEqual(csv.GetRecord(1,1), d1);
            Assert.AreEqual(csv.GetRecord(1, 2), d2AsDecimal);
        }

        [TestMethod]
        public void Culture()
        {
            var invariantCultureCsv = new Csv().AddRow("5,2");
            var danishCulture = new Csv().Culture("da-DK").AddRow("5,2");
            Assert.AreEqual(invariantCultureCsv.GetRecord(1, 1), 52m);
            Assert.AreEqual(danishCulture.GetRecord(1, 1), 5.2m);
        }

        [TestMethod]
        public void RemoveColumn()
        {
            var csv = new Csv();
            csv.AddHeader(1, "A");
            csv.AddHeader(2, "B");
            csv.AddRecord(1, 1, "a");
            csv.AddRecord(1, 2, "b");

            csv.RemoveColumn(2);
            Assert.IsTrue(csv.Headers.First().Value.Equals("A"));
            Assert.IsTrue(csv.ColTypes.First().Value.Equals(typeof(string)));
            Assert.IsTrue(csv.ColCount == 1);
            Assert.IsTrue(csv.Records.First().Value.Equals("a"));

            csv = new Csv();
            csv.AddHeader(1, "A");
            csv.AddHeader(2, "B");
            csv.AddRecord(1, 1, "a");
            csv.AddRecord(1, 2, "b");
            csv.AddRecord(1, 3, "c");  //There are no header on perpous

            csv.RemoveColumn(1);
            Assert.IsTrue(csv.Headers.First().Value.Equals("B"));
            Assert.IsTrue(csv.ColCount == 2);
            Assert.IsTrue(csv.Records.First().Value.Equals("b"));
        }

        [TestMethod]
        public void RemoveColumn2()
        {
            var csv = new Csv();
            csv.AddHeader(2, "A");  //Starts with 2 on purpose
            csv.AddHeader(3, "B");
            csv.AddHeader(4, "C");
            csv.AddHeader(5, "D");
            csv.AddRecord(1, 2, "a");
            csv.AddRecord(1, 3, "b");
            csv.AddRecord(1, 4, "c");
            //1,5 is missing on purpose
            csv.AddRecord(1, 6, "no_Name");
            csv.RemoveColumn(3);
            Assert.IsTrue(csv.ColLimit == (2, 5));
            Assert.IsTrue(csv.RowLimit == (1, 1));
            Assert.IsTrue(csv.Records.Count == 3);
            Assert.IsTrue(csv.Records[(1, 2)].ToString() == "a");
            Assert.IsTrue(csv.Records[(1, 3)].ToString() == "c");
            Assert.IsTrue(csv.Records[(1, 5)].ToString() == "no_Name");
        }

        [TestMethod]
        public void RemoveEmptyColumns()
        {
            var csv = new Csv();
            csv.AddHeader(2, "A");  //Starts with 2 on purpose
            csv.AddHeader(3, "B");
            csv.AddHeader(4, "C");
            csv.AddHeader(5, "D");
            csv.AddRecord(1, 2, "a");
            csv.AddRecord(1, 3, "");
            csv.AddRecord(1, 4, "c");
            //1,5 is missing on purpose
            csv.AddRecord(1, 6, "no_Name");
            csv.RemoveEmptyColumns();
            Assert.IsTrue(csv.ColCount == 3);
            Assert.IsTrue(csv.ColMaxLengths.Count == 3);
            Assert.IsTrue(csv.RowCount == 1);
            Assert.IsTrue(csv.ColLimit == (1, 3));
            Assert.IsTrue(csv.RowLimit == (1, 1));
            Assert.IsTrue(csv.Records[(1, 1)].ToString() == "a");
            Assert.IsTrue(csv.Records[(1, 2)].ToString() == "c");
            Assert.IsTrue(csv.Records[(1, 3)].ToString() == "no_Name");

            //Test if everthing can be null:
            csv = new Csv();
            csv.RemoveEmptyColumns();
        }

        [TestMethod]
        public void RemoveEmptyRows()
        {
            var csv = new Csv("A,B");
            csv.AddRecord(1, 1, "");
            csv.AddRecord(1, 1, null);
            csv.AddRecord(2, 1, "a2");
            csv.AddRecord(2, 2, "b2");
            csv.AddRecord(3, 1, "a3");
            csv.AddRecord(3, 2, null);
            csv.AddRecord(4, 1, "");
            csv.AddRecord(4, 2, "b4");
            csv.RemoveEmptyRows();
            Assert.IsTrue(csv.RowLimit.Max == 3);
            Assert.IsTrue(csv.ColCount == 2);
            Assert.IsTrue(csv.RowCount == 4);
        }

        [TestMethod]
        public void RemoveEmptyRecords()
        {
            var csv = new Csv("A,B");
            csv.AddRecord(1, 1, "a");
            csv.AddRecord(1, 2, "");
            csv.AddRecord(2, 1, null);
            csv.AddRecord(2, 2, string.Empty);
            csv.RemoveEmptyRecords();
            Assert.IsTrue(csv.Records.Count == 1);
            Assert.IsTrue(csv.ColCount == 2);
            Assert.IsTrue(csv.RowCount == 1);
        }

        [TestMethod]
        public void RemoveRows()
        {
            var csv = new Csv("A,B");
            csv.AddRecord(1, 1, null);
            csv.AddRecord(2, 1, "a2");
            csv.AddRecord(2, 2, "b2");
            csv.AddRecord(3, 1, "a3");
            csv.AddRecord(3, 2, "b3");
            csv.AddRecord(4, 1, "a4");
            csv.AddRecord(4, 2, "b4");
            csv.RemoveRows(new int[] { 2, 3 });
            Assert.IsTrue(csv.RowCount == 2);

            csv = new Csv("A,B");
            csv.AddRecord(1, 1, null);
            csv.AddRecord(2, 1, "a2");
            csv.AddRecord(2, 2, "b2");
            csv.AddRecord(3, 1, "a3");
            csv.AddRecord(3, 2, "b3");
            csv.AddRecord(4, 1, "a4");
            csv.AddRecord(4, 2, "b4");
            csv.RemoveRows(new int[] { 2, 3 }, false);
            Assert.IsTrue(csv.RowCount == 1);
        }

        [TestMethod]
        public void RemoveAllRows()
        {
            var csv = new Csv("A");
            csv.AddRecord(1, 1, "a1");
            csv.AddRecord(2, 1, null);
            csv.AddRecord(3, 1, "a2");
            csv.RemoveRows(new int[] { 1, 2, 3 });
            Assert.IsTrue(csv.RowCount == 0);
            Assert.IsTrue(csv.RowLimit == (0, 0));
        }

        [TestMethod]
        public void RemoveRedundantRows()
        {
            var csv = new Csv("A");
            csv.AddRecord(1, 1, "a");
            csv.AddRecord(2, 1, null);
            csv.AddRecord(3, 1, "A");
            csv.AddRecord(4, 1, "b");
            csv.AddRecord(5, 1, "b");
            csv.AddRecord(6, 1, "c");
            csv.RemoveRedundantRows("A", true);
            Assert.IsTrue(csv.RowCount == 4);

            csv = new Csv("A");
            csv.AddRecord(1, 1, "a");
            csv.AddRecord(2, 1, null);
            csv.AddRecord(3, 1, "A");
            csv.AddRecord(4, 1, "b");
            csv.AddRecord(5, 1, "b");
            csv.AddRecord(6, 1, "c");
            csv.RemoveRedundantRows("A", false);
            Assert.IsTrue(csv.RowCount == 5);
        }

        [TestMethod]
        public void ReplaceRecordContent()
        {
            var csv = new Csv("A,B");
            csv.AddRecord(1, 1, "a");
            csv.AddRecord(1, 2, "b");
            csv.AddRecord(2, 1, "a");
            csv.AddRecord(2, 2, "b\nc");
            csv.AddRecord(3, 1, null);
            csv.ReplaceRecordContent("a", "d");
            Assert.IsTrue(csv.Records[(1, 1)].Equals("d"));
            csv.ReplaceRecordContent('\n', '\r');
        }

        [TestMethod]
        public void VerifyUniqueHeaders()
        {
            var csv = new Csv();
            csv.AddHeader(1, "Container");
            csv.AddHeader(2, "Uploaded");
            csv.AddHeader(3, "Container");
            csv.AddHeader(4, "Container");

            Assert.IsTrue(csv.Headers.First().Value.Equals("Container"));
            Assert.IsTrue(csv.Headers.Count(o => o.Value.Equals("Container")) == 1);
            Assert.IsTrue(csv.Headers.Count(o => o.Value.Equals("Container_2")) == 1);
            Assert.IsTrue(csv.Headers.Count(o => o.Value.Equals("Container_3")) == 1);
        }

        [TestMethod]
        public void AddDates()
        {
            Assert.AreEqual(new Csv().AddRow("09-10-2014").ColTypes[1], typeof(DateTime));
            Assert.AreEqual(new Csv().AddRow("26-10-2021 14:43:58").ColTypes[1], typeof(DateTime));
            Assert.AreEqual(new Csv().AddRow("2012-09-09").ColTypes[1], typeof(DateTime));
            Assert.AreEqual(new Csv().AddRow("2012-09-09 14:23:21").ColTypes[1], typeof(DateTime));
            Assert.AreEqual(new Csv().AddRow("2012-09-20T00:00:00").ColTypes[1], typeof(DateTime));
        }

        [TestMethod]
        public void VerifyCsvParsing()
        {
            var csv = new Csv();
            csv.AddRecord(1, 1, "21");
            csv.AddRecord(1, 2, "21.1");
            csv.AddRecord(1, 3, "true");
            csv.AddRecord(1, 4, "26-10-2021 14:43:58");
            csv.AddRecord(1, 5, "test");
            csv.AddHeader(1, "Container");  //Ending with adding rows to provoke an eventual error

            Assert.IsTrue(csv.Headers.First().Value.Equals("Container"));
            Assert.IsTrue(csv.Records.First().Value.Equals(21));
            Assert.IsTrue(csv.ColTypes[1] == typeof(int));
            Assert.IsTrue(csv.ColTypes[2] == typeof(decimal));
            Assert.IsTrue(csv.ColTypes[3] == typeof(bool));
            Assert.IsTrue(csv.ColTypes[4] == typeof(DateTime));
            Assert.IsTrue(csv.ColTypes[5] == typeof(string));
            Assert.IsTrue(csv.ColLimit.Equals((1, 5)));
            Assert.IsTrue(csv.RowLimit.Equals((1, 1)));
        }

        [TestMethod]
        public void VerifyLimits()
        {
            var csv = new Csv();
            csv.AddRecord(1, 1, "");
            Assert.IsTrue(csv.RowLimit == (1, 1) && csv.ColLimit == (1, 1));
            csv.AddRecord(2, 1, "");
            Assert.IsTrue(csv.RowLimit == (1, 2) && csv.ColLimit == (1, 1));
            csv.AddRecord(1, 2, "");
            Assert.IsTrue(csv.RowLimit == (1, 2) && csv.ColLimit == (1, 2));
            csv.AddRecord(3, 2, "");
            Assert.IsTrue(csv.RowLimit == (1, 3) && csv.ColLimit == (1, 2));
            csv.AddRecord(3, 0, "");
            Assert.IsTrue(csv.RowLimit == (1, 3) && csv.ColLimit == (0, 2));
            csv.AddRecord(0, 0, "");
            Assert.IsTrue(csv.RowLimit == (0, 3) && csv.ColLimit == (0, 2));

            csv = new Csv();
            csv.AddHeader(1, "");
            Assert.IsTrue(csv.RowLimit == (0, 0) && csv.ColLimit == (1, 1));
        }
    }
}
