using Bygdrift.Tools.CsvTool;
using DocumentFormat.OpenXml.Office2013.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CsvToolTests
{
    [TestClass]
    public class CsvSetTests_Class
    {
        [TestMethod]
        public void First()
        {
            var csv = new Csv();
            //csv.Add("ww");

            csv.Add(new CsvSetTests_Class_First(1, "Test"));

            var range = new List<CsvSetTests_Class_First>
            {
                new CsvSetTests_Class_First(2, "Test2"),
                new CsvSetTests_Class_First(3, "Test3"),
            };

            csv.AddRange(range);

        }
    }

    public class CsvSetTests_Class_First
    {

        public CsvSetTests_Class_First(int number, string text)
        {
            Number = number;
            Text = text;
        }

        public int Number { get; set; }

        public string Text { get; set; }

    }
}
