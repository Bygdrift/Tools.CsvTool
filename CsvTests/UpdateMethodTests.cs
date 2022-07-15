using Bygdrift.Tools.Csv;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvTests
{
    [TestClass]
    public class UpdateMethodTests
    {
        [TestMethod]
        public void Update1()
        {
            var updater = new CsvUpdate(GetCsvToUpdate(), false, "Id");
            updater.Merge(GetCsvToMerge());
            var merged = updater.Csv;
            Assert.IsTrue(merged.RowLimit.Max == 3);
            var lastRow = merged.GetRowRecords(3);
            Assert.IsTrue(lastRow.TryGetValue(3, out object lastRowValue));
            Assert.IsTrue((string)lastRowValue == "c");
        }

        [TestMethod]
        public void Update2()
        {
            var updater = new CsvUpdate(GetCsvToUpdate(), false, "Id", "Date");
            updater.Merge(GetCsvToMerge());
            var merged = updater.Csv;
            Assert.AreEqual(merged.RowLimit.Max, 3);
            var lastRow = merged.GetRowRecords(3);
            Assert.IsTrue(lastRow.TryGetValue(3, out object lastRowValue));
            Assert.AreEqual((string)lastRowValue, "c");
        }

        [TestMethod]
        public void Update3()
        {
            var updater = new CsvUpdate(GetCsvToUpdate(), false, "Date");
            updater.Merge(GetCsvToMerge());
            var merged = updater.Csv;
            Assert.IsTrue(merged.RowLimit.Max == 2);
        }

        [TestMethod]
        public void TestAddingPrimaryKey()
        {
            var updater = new CsvUpdate(GetCsvToUpdate(), "Id");
            updater.Merge(GetCsvToMerge());
            var merged = updater.Csv;
            Assert.IsTrue(merged.RowLimit.Max == 3);
            Assert.IsTrue(merged.Records[(2, 3)].Equals("Updated"));
        }

        [TestMethod]
        public void RemoveDuplicates()
        {
            var csvToUpdate = GetCsvToUpdate();
            Assert.IsTrue(csvToUpdate.RowLimit.Equals((1, 2)));

            //Add an extra row
            csvToUpdate.AddRecord(3, 2, new DateTime(2021, 1, 1));
            Assert.IsTrue(csvToUpdate.RowLimit.Equals((1, 3)));

            var csvToMerge = GetCsvToMerge();

            var updater = new CsvUpdate(csvToUpdate, true, "Date");
            updater.Merge(csvToMerge);
            var merged = updater.Csv;
            Assert.IsTrue(merged.RowLimit.Equals((1, 2)));
        }

        private static Csv GetCsvToUpdate()
        {
            var csv = new Csv("Id, Date, Data")
            .AddRecord(1, 1, 1)
            .AddRecord(1, 2, new DateTime(2021, 1, 1))
            .AddRecord(1, 3, "a")
            .AddRecord(2, 1, 2)
            .AddRecord(2, 2, new DateTime(2021, 1, 2))
            .AddRecord(2, 3, "b");
            return csv;
        }

        private static Csv GetCsvToMerge()
        {
            var csv = new Csv("Id, Date, Data")
            .AddRecord(1, 1, 2)
            .AddRecord(1, 2, new DateTime(2021, 1, 2))
            .AddRecord(1, 3, "Updated")
            .AddRecord(2, 1, 3)
            .AddRecord(2, 2, new DateTime(2021, 1, 1))
            .AddRecord(2, 3, "c");
            return csv;
        }
    }
}
