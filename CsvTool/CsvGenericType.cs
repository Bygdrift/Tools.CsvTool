using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bygdrift.Tools.CsvTool
{

    public partial class Csv
    {
        /// <summary>
        /// Add an item
        /// </summary>
        public Csv Add<T>(T item) where T : class
        {
            var row = RowLimit.Max + 1;
            var g = item.GetType();
            var kk = g.GetElementType();
            var k = g.GetConstructor(new Type[] { typeof(T) });
            var jf = g.GetFields();


            var j = g.GetProperties();
            
            foreach (var prop in g.GetProperties())
                AddRecord(row, prop.Name, prop.PropertyType, prop.GetValue(item, null));

            return this;
        }

        /// <summary>
        /// Add a range of items
        /// </summary>
        public Csv AddRange<T>(IEnumerable<T> collection) where T : class
        {
            var row = RowLimit.Max + 1;
            foreach (var item in collection)
            {
                foreach (var prop in item.GetType().GetProperties())
                    AddRecord(row, prop.Name, prop.PropertyType, prop.GetValue(item, null));

                row++;
            }

            return this;
        }

        private int AddRecord(int row, string headerName, Type type, object value)
        {
            int col;

            var header = Headers.SingleOrDefault(o => o.Value.Equals(headerName));
            if (header.Value != null)
                col = header.Key;
            else
            {
                col = ColLimit.Max + 1;
                Headers.Add(col, headerName);
            }

            if (ColTypes == null || !ColTypes.ContainsKey(col))
                ColTypes.Add(col, type);
            else
                ColTypes[col] = type;

            if (!ColMaxLengths.ContainsKey(col))
                ColMaxLengths.Add(col, 0);

            if (Records == null || Records.ContainsKey((row, col)))
                Records.Add((row, col), value);
            else
                Records[(row, col)] = value;

            if (_colLimit.Min > col || _colLimit.Min == null)
                _colLimit.Min = col;
            if (_colLimit.Max < col || _colLimit.Max == null)
                _colLimit.Max = col;
            if (_rowLimit.Min > row || _rowLimit.Min == null)
                _rowLimit.Min = row;
            if (_rowLimit.Max < row || _rowLimit.Max == null)
                _rowLimit.Max = row;

            return col;
        }
    }
}
