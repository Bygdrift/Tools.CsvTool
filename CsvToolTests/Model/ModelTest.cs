
using CsvTool.Attributes;
using System;

namespace CsvToolTests.Model
{
    public class ModelTest
    {
        public int Number { get; set; }

        public string Text { get; set; }

        [CsvIgnore]
        public string ValueToIgnore { get; set; }
    }

    public class ModelTest2
    {
        public int Number { get; set; }

        public string Text { get; set; }
        public DateTime date { get; set; }

        public ModelTest Model { get; set; }

        [CsvIgnore]
        public string ValueToIgnore { get; set; }
    }
}
