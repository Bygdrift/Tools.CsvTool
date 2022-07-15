using Bygdrift.Tools.Csv;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvTests
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
                date.ToString("d/M/yyyy"),
                date.ToString("yyyy/M/d"),
                date.ToString("dd-MM-yyyy HH:mm:ss"),
                date.ToString("d-M-yyyy HH:mm:ss"),
                date.ToString("dd-MM-yyyy"),
                date.ToString("d-M-yyyy"),
                date.ToString("dd/MM/yyyy HH:mm:ss"),
                date.ToString("d/M/yyyy HH:mm:ss"),
                date.ToString("dd/MM/yyyy"),
            };

            foreach (var item in dates)
            {
                if (csv.RecordToType(item, out DateTime val))
                    Assert.IsTrue(date.Date.Equals(val.Date));

                object res = item;
                var type = csv.GetType(ref res).Type;
                Assert.IsTrue(type == typeof(DateTime));


                //var a = DateTime.TryParse(item, out DateTime date1);
                //Assert.IsTrue(a);

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
        }
    }
}