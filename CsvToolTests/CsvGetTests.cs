using Bygdrift.Tools.CsvTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CsvToolTests
{
    [TestClass]
    public class CsvGetTests
    {
        [TestMethod]
        public void GetRecordColByLookup()
        {
            var csv = CreateCsv();
            var res = csv.GetColRecords("Type", "Mail", "Room", true);
            Assert.IsTrue(res.Count == 3);
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

            var res1 = csv.GetRowsRecords("Mail", true, "ANDEgaarden-1.sal@xyz.dk");
            Assert.IsTrue(res1.Count == 1);

            var res2 = csv.GetRowsRecords("Mail", true, "ndegaarden-1.sal@xyz.dk");
            Assert.IsTrue(res2.Count == 0);

            var res3 = csv.GetRowsRecords("Building", true, null);
            Assert.IsTrue(res3.Count == 8);
            
            var res4 = csv.GetRowsRecords("Capacity", true, 6);
            Assert.IsTrue(res4.Count == 1);
            Assert.IsTrue(res4.First().Value.Count == 7);
            
            var res5 = csv.GetRowsRecords("Capacity", true, null);
            Assert.IsTrue(res5.Count == 7);
        }

        [TestMethod]
        public void GetRowRecords()
        {
            var csv = CreateCsv();
            var res = csv.GetRowRecords(1);
            Assert.AreEqual(7, res.Count);
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

            Assert.IsTrue(csv.TryGetRowFirstId("A", "tt", false, out int res));
            Assert.IsTrue(res == 4);
            Assert.IsTrue(csv.TryGetRowFirstId("A", "tt", true, out int res2));
            Assert.IsTrue(res2 == 3);

            Assert.IsFalse(csv.TryGetRowFirstId("v", "tt", true, out int res3));
            Assert.IsTrue(res3 == 0);

            Assert.IsFalse(csv.TryGetRowFirstId("A", "ttt", true, out int res4));
            Assert.IsTrue(res4 == 0);

        }

        private static Csv CreateCsv()
        {
            var csv = new Csv("Mail,Capacity,Location,Level,Building,Name,Type");
            csv.AddRows(new[] {
                "andegaarden-1.sal@xyz.dk,6,Trollesmindealle 27,1. sal,Rådhus,Andergården,Room",
                "byraadssalen-moedecenter@xyz.dk,30,Trollesmindealle 27,Stuen,Rådhus,Byrådssalen,Room",
                "BilABA97100@xyz.dk,,Ukendt,,,Ældre og sundhed - Bil BA 97100,Vehicle",
                "BocenterMoedelokale3@xyz.dk,3,Ukendt,,,Bocenter - Mødelokale 3,Room",
                "Bocentrets_faelleskalender@xyz.dk,,Ukendt,,,Bocenter-fælleskalender,Calendar",
                "BSS-Bil-CH16136@xyz.dk,,Ukendt,,,BSS Bil CH16136,Vehicle",
                "ByogMiljo-Bil3@xyz.dk,,Ukendt,,,By og Miljø - Bil 3,Vehicle",
                "byogmiljobil5@xyz.dk,,Ukendt,,,By og Miljø - Bil 5,Vehicle",
                "byogmiljobil6@xyz.dk,,Ukendt,,,By og Miljø - Bil 6,Vehicle",
                "ByogMiljo-Stor_Projektor@xyz.dk,,Ukendt,,,By og Miljø – Stor Projektor,Asset",
            });
            return csv;
        }
    }
}
