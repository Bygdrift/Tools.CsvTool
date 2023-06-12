using Bygdrift.Tools.CsvTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Linq;

namespace CsvToolTests
{
    [TestClass]
    public class CsvGetTests
    {
        [TestMethod]
        public void GetCsvCopy()
        {
            var csv = new Csv("a").AddRows("Empty");
            var csvCopied = csv.GetCsvCopy();
            csv.AddRecord(1, 1, "Full");
            csv.AddHeader("Test");
            Assert.AreEqual("Empty", csvCopied.GetRecord(1, 1));
            Assert.AreEqual(-1, csvCopied.GetHeader("Test"));
        }

        [TestMethod]
        public void GetColRecordsGrouped()
        {
            var csv = CreateCsv();
            var res = csv.GetColRecordsGrouped<int>("Type", "Id");
            var json = JsonConvert.SerializeObject(res, Formatting.None);
            Assert.AreEqual("{\"Room\":{\"1\":1,\"2\":2,\"4\":4},\"Vehicle\":{\"3\":3,\"6\":6,\"7\":7,\"8\":8,\"9\":9},\"Calendar\":{\"5\":5},\"Asset\":{\"10\":10}}", json);
        }

        [TestMethod]
        public void GetRecordByHeader()
        {
            var csv = CreateCsv();
            Assert.AreEqual(csv.GetRecord(1, "Type"), "Room");
            try
            {
                Assert.AreEqual(csv.GetRecord(1, "A header the doesn´t exist"), 0);
                Assert.Fail("No exception thrown");
            }
            catch (System.Exception e)
            {
                Assert.AreEqual(e.Message, "The headerName 'A header the doesn´t exist' are not in csv");
            }
        }

        [TestMethod]
        public void GetRecordColByLookup()
        {
            var csv = CreateCsv();
            var res = csv.GetColRecords("Type", "Mail", "Room");
            Assert.IsTrue(res.Count == 3);
        }

        [TestMethod]
        public void GetHeadersAsString()
        {
            var csv = new Csv().AddHeader(2, "Data").AddHeader(1, "Id");
            var headers = csv.GetHeadersAsString();
            Assert.AreEqual(headers, "Id, Data");
        }

        [TestMethod]
        public void GetColRecords()
        {
            var csv = CreateCsv();
            var res = csv.GetColRecords("Capacity", true);
            Assert.IsTrue(res.Count == 10);
            var res2 = csv.GetColRecords("Capacity", false);
            Assert.IsTrue(res2.Count == 3);
        }

        [TestMethod]
        public void GetRecordRowByHeader()
        {
            var csv = CreateCsv();

            var res = csv.GetRowsRecords("Mail", true, "andegaarden-1.sal@xyz.dk");
            Assert.IsTrue(res.Count == 1);

            var res1 = csv.GetRowsRecords("Mail", false, "ANDEgaarden-1.sal@xyz.dk");
            Assert.IsTrue(res1.Count == 1);

            var res2 = csv.GetRowsRecords("Mail", true, "ndegaarden-1.sal@xyz.dk");
            Assert.IsTrue(res2.Count == 0);

            var res3 = csv.GetRowsRecords("Building", true, null);
            Assert.IsTrue(res3.Count == 8);

            var res4 = csv.GetRowsRecords("Capacity", true, 6);
            Assert.IsTrue(res4.Count == 1);
            Assert.IsTrue(res4.First().Value.Count == 8);

            var res5 = csv.GetRowsRecords("Capacity", true, null);
            Assert.IsTrue(res5.Count == 7);
        }

        [TestMethod]
        public void GetRowRecords()
        {
            var csv = CreateCsv();
            var res = csv.GetRowRecords(1);
            Assert.AreEqual(8, res.Count);
            Assert.AreEqual(6, res[2]);
        }

        [TestMethod]
        public void GetColRecordsMaxLength()
        {
            var csv = new Csv("A");
            csv.AddRecord(1, 1, null);
            Assert.IsTrue(csv.ColMaxLengths[1] == 0);
            csv.AddRecord(1, 1, 1);
            Assert.IsTrue(csv.ColMaxLengths[1] == 1);
            csv.AddRecord(1, 1, "tt");
            Assert.IsTrue(csv.ColMaxLengths[1] == 2);

            csv = new Csv("A");
            csv.AddRecord(1, 1, 1);
            csv.AddRecord(2, 1, null);
            csv.AddRecord(3, 1, "wws");
            csv.AddRecord(4, 1, 25);
            Assert.IsTrue(csv.ColMaxLengths[1] == 3);
        }
        
        [TestMethod]
        public void GetRowRecordsFirstMatch()
        {
            var csv = CreateCsv();
            
            var res = csv.GetRowRecordsFirstMatch("Mail", "Byraadssalen-moedecenter@xyz.dk", true, true);
            Assert.AreEqual(0, res.Count);
            
            res = csv.GetRowRecordsFirstMatch("Mail", "Byraadssalen-moedecenter@xyz.dk", true, false);
            Assert.AreEqual(8, res.Count);
            
            res = csv.GetRowRecordsFirstMatch("x", "Byraadssalen-moedecenter@xyz.dk", true, false);
            Assert.IsNull(res);
            
            res = csv.GetRowRecordsFirstMatch("Mail", "x", true, false);
            Assert.AreEqual(0, res.Count);
            
            res = new Csv().GetRowRecordsFirstMatch("Mail", "x", true, false);
            Assert.IsNull(res);
        }

        [TestMethod]
        public void GetRowRecordsMatch()
        {
            var csv = CreateCsv();
            var res = csv.GetRowRecordsMatch("Type", "Room");
            var json = JsonConvert.SerializeObject(res, Formatting.None);
            Assert.AreEqual("{\"1\":{\"1\":\"andegaarden-1.sal@xyz.dk\",\"2\":6,\"3\":\"Trollesmindealle 27\",\"4\":\"1. sal\",\"5\":\"Rådhus\",\"6\":\"Andergården\",\"7\":\"Room\",\"8\":1},\"2\":{\"1\":\"byraadssalen-moedecenter@xyz.dk\",\"2\":30,\"3\":\"Trollesmindealle 27\",\"4\":\"Stuen\",\"5\":\"Rådhus\",\"6\":\"Byrådssalen\",\"7\":\"Room\",\"8\":2},\"4\":{\"1\":\"BocenterMoedelokale3@xyz.dk\",\"2\":3,\"3\":\"Ukendt\",\"4\":null,\"5\":null,\"6\":\"Bocenter - Mødelokale 3\",\"7\":\"Room\",\"8\":4}}", json);
        }

        [TestMethod]
        public void TryGetColId()
        {
            var csv = new Csv("A, B");
            csv.AddRecord(1, 1, 1);

            Assert.IsTrue(csv.TryGetColId("A", out int res));
            Assert.IsTrue(res == 1);
            Assert.IsTrue(csv.TryGetColId("B", out int res2));
            Assert.IsTrue(res2 == 2);
            Assert.IsFalse(csv.TryGetColId("v", out int res3));
            Assert.IsTrue(res3 == 0);
            Assert.IsFalse(csv.TryGetColId(null, out int res4));
            Assert.IsTrue(res4 == 0);
        }

        [TestMethod]
        public void TryGetRowFirstId()
        {
            var csv = new Csv("A");
            csv.AddRecord(1, 1, null);
            csv.AddRecord(2, 1, "t");
            csv.AddRecord(3, 1, "TT");
            csv.AddRecord(4, 1, "tt");
            csv.AddRecord(5, 1, 1);

            Assert.IsTrue(csv.TryGetRowFirstId("A", "tt", true, out int res));
            Assert.IsTrue(res == 4);
            Assert.IsTrue(csv.TryGetRowFirstId("A", "tt", false, out int res2));
            Assert.IsTrue(res2 == 3);

            Assert.IsFalse(csv.TryGetRowFirstId("v", "tt", true, out int res3));
            Assert.IsTrue(res3 == 0);

            Assert.IsFalse(csv.TryGetRowFirstId("A", "ttt", true, out int res4));
            Assert.IsTrue(res4 == 0);
        }

        private static Csv CreateCsv()
        {
            var csv = new Csv("Mail,Capacity,Location,Level,Building,Name,Type,Id");
            csv.AddRows(new[] {
                "andegaarden-1.sal@xyz.dk,6,\"Trollesmindealle 27\",1. sal,Rådhus,Andergården,Room,1",
                "byraadssalen-moedecenter@xyz.dk,30,Trollesmindealle 27,Stuen,Rådhus,Byrådssalen,Room,2",
                "BilABA97100@xyz.dk,,Ukendt,,,Ældre og sundhed - Bil BA 97100,Vehicle,3",
                "BocenterMoedelokale3@xyz.dk,3,Ukendt,,,Bocenter - Mødelokale 3,Room,4",
                "Bocentrets_faelleskalender@xyz.dk,,Ukendt,,,Bocenter-fælleskalender,Calendar,5",
                "BSS-Bil-CH16136@xyz.dk,,Ukendt,,,BSS Bil CH16136,Vehicle,6",
                "ByogMiljo-Bil3@xyz.dk,,Ukendt,,,By og Miljø - Bil 3,Vehicle,7",
                "byogmiljobil5@xyz.dk,,Ukendt,,,By og Miljø - Bil 5,Vehicle,8",
                "byogmiljobil6@xyz.dk,,Ukendt,,,By og Miljø - Bil 6,Vehicle,9",
                "ByogMiljo-Stor_Projektor@xyz.dk,,Ukendt,,,By og Miljø – Stor Projektor,Asset,10",
            });
            return csv;
        }
    }
}