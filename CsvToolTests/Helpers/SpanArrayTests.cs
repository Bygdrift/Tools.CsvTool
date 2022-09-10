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
            var arr = SpanArray.PerHour(new DateTime(22, 1, 1, 1, 0, 0), new DateTime(22, 1, 1, 2, 10, 0));
            Assert.AreEqual(arr.Count, 2);

            arr = SpanArray.PerHour(new DateTime(22, 1, 1, 1, 0, 0), new DateTime(22, 1, 1, 1, 0, 0));
            Assert.AreEqual(arr.Count, 1);

            arr = SpanArray.PerHour(new DateTime(22, 1, 1, 1, 0, 0), new DateTime(22, 1, 1, 11, 0, 0), null, 8, 10);
            Assert.AreEqual(arr.Count, 2);

            arr = SpanArray.PerHour(new DateTime(22, 1, 1, 1, 0, 0), new DateTime(22, 1, 1, 11, 0, 0), new List<object> { "A", "B" }, 8, 10);
            Assert.AreEqual(arr.Count, 4);
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
