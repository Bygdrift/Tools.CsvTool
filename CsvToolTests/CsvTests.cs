using Bygdrift.Tools.CsvTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvToolTests
{
    [TestClass]
    public class CsvTests
    {
        [TestMethod]
        public void TypeTest()
        {
            var csv = new Csv();

            var date = DateTime.Parse("2021-1-26 14:43:58");
            var dates = new string[] {
                date.ToString("s"),
                date.ToString("yyyy/M/d"),
            };

            foreach (var item in dates)
            {
                if (csv.RecordToType(item, out DateTime val))
                    Assert.IsTrue(date.Date.Equals(val.Date));

                object res = item;
                var type = csv.GetType(ref res).Type;
                Assert.IsTrue(type == typeof(DateTime));
            }
        }

        [TestMethod]
        public void ValutToType()
        {
            var csv = new Csv();
            DateTime? dateNull = null;
            csv.RecordToType(dateNull, out string dateNullVal);
            csv.RecordToType(null, out string nullVal);
            csv.RecordToType("22", out string stringVal);
            csv.RecordToType("22", out int intVal);
            csv.RecordToType("22", out long longVal);
            csv.RecordToType("22", out decimal decVal);
            csv.RecordToType("22", out DateTime dateVal);
            Assert.AreEqual(dateVal, new DateTime());
        }
    }
}