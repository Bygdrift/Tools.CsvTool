using Bygdrift.Tools.CsvTool.TimeStacking;
using Bygdrift.Tools.CsvTool.TimeStacking.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CsvToolTests.Helpers
{
    [TestClass]
    public class SpanArrayTests
    {
        [TestMethod]
        public void TestPerHour()
        {
            var arr = SpanArray.PerHour(new DateTime(2021, 12, 31, 18, 10, 0), new DateTime(2022, 1, 1, 9, 27, 0));
            Assert.AreEqual(9, arr.Count);

            arr = SpanArray.PerHour(new DateTime(2021, 12, 31, 18, 17, 0), new DateTime(2022, 1, 1, 9, 27, 0), new DateTime(2021, 12, 31, 22, 10, 0), new DateTime(2022, 1, 1, 9,19, 0));
            Assert.AreEqual(11, arr.Count);
            Assert.AreEqual(new DateTime(2021, 12, 31, 22, 0, 0), arr[0].From);

            arr = SpanArray.PerHour(new DateTime(22, 1, 1, 1, 0, 0), new DateTime(22, 1, 1, 2, 10, 0));
            Assert.AreEqual(2, arr.Count);

            arr = SpanArray.PerHour(new DateTime(22, 1, 1, 1, 0, 0), new DateTime(22, 1, 1, 1, 0, 0));
            Assert.AreEqual(1, arr.Count);

            arr = SpanArray.PerHour(new DateTime(22, 1, 1, 1, 0, 0), new DateTime(22, 1, 1, 11, 0, 0), 8, 10, null);
            Assert.AreEqual(2, arr.Count);

            arr = SpanArray.PerHour(new DateTime(22, 1, 1, 1, 0, 0), new DateTime(22, 1, 1, 11, 0, 0), 8, 10, new List<object> { "A", "B" });
            Assert.AreEqual(4, arr.Count);
        }

        [TestMethod]
        public void TestPerDay()
        {
            var a = SpanArray.PerDay(new DateTime(22, 1, 1, 1, 0, 0), new DateTime(22, 1, 1, 1, 10, 0));
            var b = SpanArray.PerDay(new DateTime(22, 1, 1, 19, 30, 0), new DateTime(22, 1, 3, 1, 10, 0));
            Assert.AreEqual(a.Count, 1);
            Assert.AreEqual(b.Count, 3);
        }

        [TestMethod]
        public void TestPerMonth()
        {
            var a = SpanArray.PerMonth(new DateTime(22, 1, 1, 1, 0, 0), new DateTime(22, 1, 1, 1, 10, 0));
            var b = SpanArray.PerMonth(new DateTime(22, 1, 1, 19, 30, 0), new DateTime(22, 3, 3, 1, 10, 0));
            Assert.AreEqual(a.Count, 1);
            Assert.AreEqual(b.Count, 3);
        }
    }
}
