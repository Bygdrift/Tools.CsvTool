using System;

namespace CsvTool.Attributes
{
    /// <summary>
    /// Used when Csv.AddModel runs through properies. If a property is decorated with [CsvIgnore] it wil not be written into the csv.
    /// </summary>
    public class CsvIgnore: Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public CsvIgnore(){}
    }
}
