using Bygdrift.Tools.CsvTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvToolTests
{
    [TestClass]
    public class CsvValueTests
    {
        [TestMethod]
        public void TestDouble()
        {
            var val = 3.3407572383233015E-05.ToString();
            var csv = new Csv("Data").AddRecord(1, 1, val);
            var a = csv.ColTypes[1];
        }

    }
}
