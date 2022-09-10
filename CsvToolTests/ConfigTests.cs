using Bygdrift.Tools.CsvTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvToolTests
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void Culture()
        {
            var invariantCultureCsv = new Csv().AddRow("5,2");
            Assert.AreEqual(invariantCultureCsv.GetRecord(1, 1), 52d);

            var danishCulture = new Csv(new Config("da-DK")).AddRow("5,2");
            Assert.AreEqual(danishCulture.GetRecord(1, 1), 5.2d);
        }
    }
}
