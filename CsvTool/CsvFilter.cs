using System.Linq;

namespace Bygdrift.Tools.CsvTool
{
    public partial class Csv
    {
        /// <param name="headerName">The header to filter on</param>
        /// <param name="values">If a value is presented in the looked up column, the row will be included</param>
        /// <param name="ignoreCase"></param>
        /// <returns>A a csv with all records that are filered</returns>
        public Csv FilterRows(string headerName, bool ignoreCase, params object[] values)
        {
            var res = new Csv();
            res.AddHeaders(Headers);

            var row = 1;
            foreach (var lookupValue in values.Distinct())
                foreach (var recordRows in GetRowsRecords(headerName, ignoreCase, lookupValue))
                    res.AddRow(row++, recordRows.Value);

            return res;
        }
    }
}
