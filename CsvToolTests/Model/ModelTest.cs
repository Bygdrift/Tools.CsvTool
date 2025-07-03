
using CsvTool.Attributes;

namespace CsvToolTests.Model
{
    public class ModelTest
    {
        public int Number { get; set; }

        public string Text { get; set; }

        [CsvIgnore]
        public string ValueToIgnore { get; set; }
    }
}
