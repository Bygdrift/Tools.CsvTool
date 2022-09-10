using Bygdrift.Tools.CsvTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvToolTests.Helpers
{
    [TestClass]
    public class DateCheckerTests
    {
        [TestMethod]
        public void TestDate()
        {
            var csv = new Csv("Date");
            csv.AddRow(DateTime.Parse("2022-3-9T03:59:39"));
            csv.AddRow("2022-03-09T03:59:39");
            csv.AddRow("2022-03-10T10:01:53.000+01:00");
            csv.AddRow("2022-03-10T10:01:53.000+01:00");

            foreach (var item in csv.ColTypes)
                Assert.AreEqual(item.Value, typeof(DateTime));

        }
    }
}
