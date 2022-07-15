using System.Collections.Generic;

namespace Bygdrift.Tools.CsvTool
{
    /// <summary>
    /// Features to update a CSV with Merge of another csv
    /// </summary>
    public class CsvUpdate
    {
        /// <summary>
        /// The Csv
        /// </summary>
        public Csv Csv { get; private set; }
        private Dictionary<string, int> compositeKeys;
        private readonly bool removeDuplicates;
        private readonly string[] headers;
        private readonly bool filterOnPrimaryKeys;

        /// <param name="csvToUpdate"></param>
        /// <param name="primaryKeys">Headers that makes up a composite key that gets tested to be unique on each row. There can only be one composite key, so duplicates will be removed.</param>
        public CsvUpdate(Csv csvToUpdate, params string[] primaryKeys)
        {
            removeDuplicates = true;
            this.headers = primaryKeys;
            var compositKeysAndCsv = GetCompositeKeysAndCsv(csvToUpdate, removeDuplicates);
            Csv = compositKeysAndCsv.Csv;
            compositeKeys = compositKeysAndCsv.CompositeKeys;
            filterOnPrimaryKeys = true;
        }

        /// <param name="csvToUpdate"></param>
        /// <param name="removeDuplicates">If there are duplicates in any merged csv, the duplicates will be removed</param>
        /// <param name="headers">For each row, get the data that has these headers. Look in the csvToUpdate, to see if the data already are there. If not, then add the row.</param>
        public CsvUpdate(Csv csvToUpdate, bool removeDuplicates, params string[] headers)
        {
            this.removeDuplicates = removeDuplicates;
            this.headers = headers;
            var compositKeysAndCsv = GetCompositeKeysAndCsv(csvToUpdate, removeDuplicates);
            Csv = compositKeysAndCsv.Csv;
            compositeKeys = compositKeysAndCsv.CompositeKeys;
            filterOnPrimaryKeys = false;
        }

        /// <summary>
        /// Csv to merge into this csv
        /// </summary>
        /// <param name="csvToMerge"></param>
        public void Merge(Csv csvToMerge)
        {
            if (csvToMerge == null)
                return;

            if (Csv == null)
            {
                var compositKeysAndCsv = GetCompositeKeysAndCsv(csvToMerge, removeDuplicates);
                Csv = compositKeysAndCsv.Csv;
                compositeKeys = compositKeysAndCsv.CompositeKeys;
                return;
            }

            var compositeKeysAndCsv = GetCompositeKeysAndCsv(csvToMerge, removeDuplicates);
            var rowAdded = Csv.RowLimit.Max;
            if (filterOnPrimaryKeys)  //If row exists, then update. If not, then add. Remove all duplicates.
            {
                foreach (var item in compositeKeysAndCsv.CompositeKeys)
                {
                    var recordRow = compositeKeysAndCsv.Csv.GetRowRecords(item.Value);
                    if (compositeKeys.ContainsKey(item.Key))
                    {
                        var origRow = compositeKeys[item.Key];
                        Csv.AddRow(origRow, recordRow);
                    }
                    else
                    {
                        Csv.AddRow(++rowAdded, recordRow);
                        compositeKeys.Add(item.Key, item.Value);
                    }
                }
            }
            else  //If row does not exist, then add. If RemoveDuplicate, then remove duplicate.
            {
                foreach (var item in compositeKeysAndCsv.CompositeKeys)
                {
                    if (!compositeKeys.ContainsKey(item.Key))
                    {
                        var recordRow = compositeKeysAndCsv.Csv.GetRowRecords(item.Value);
                        Csv.AddRow(++rowAdded, recordRow);
                        compositeKeys.Add(item.Key, item.Value);
                    }
                }
            }

        }

        private (Csv Csv, Dictionary<string, int> CompositeKeys) GetCompositeKeysAndCsv(Csv csv, bool removeDuplicates)
        {
            if (removeDuplicates)
            {
                var res = GetCompoisteKeysAndCsvWithsDuplicatesRemoved(csv);
                return (res.Csv, res.CompositeKeys);
            }
            else
            {
                var compositeKeysRes = GetCompositeKeys(csv, headers);
                return (csv, compositeKeysRes);
            }
        }

        private (Csv Csv, Dictionary<string, int> CompositeKeys) GetCompoisteKeysAndCsvWithsDuplicatesRemoved(Csv csv)
        {
            if (csv == null)
                return default;

            Csv csvRes;
            Dictionary<string, int> compositeKeysRes;

            csvRes = new Csv();
            csvRes.AddHeaders(csv.Headers);
            compositeKeysRes = new Dictionary<string, int>();
            var cols = new List<int>();
            foreach (var header in headers)
                if (csv.TryGetColId(header, out int col))
                    cols.Add(col);

            var separator = headers.Length > 1 ? "," : "";
            int rowAdded = 1;
            for (int row = csv.RowLimit.Min; row <= csv.RowLimit.Max; row++)
            {
                var rowVal = "";
                foreach (var col in cols)
                    rowVal += (csv.Records.TryGetValue((row, col), out object val) ? val.ToString() : "") + separator;

                if (compositeKeysRes.TryAdd(separator.Length > 0 ? rowVal.TrimEnd(',') : rowVal, rowAdded))
                {
                    var recordRow = csv.GetRowRecords(row);
                    csvRes.AddRow(rowAdded++, recordRow);
                }
            }
            return (csvRes, compositeKeysRes);
        }

        /// <summary>
        /// Builds a combination of all values from headers
        /// </summary>
        /// <returns>Key: All data combined with a comma as separator. Value: Row number</returns>
        private static Dictionary<string, int> GetCompositeKeys(Csv csv, params string[] headers)
        {
            if (csv == null)
                return default;

            var res = new Dictionary<string, int>();
            var cols = new List<int>();
            foreach (var header in headers)
                if (csv.TryGetColId(header, out int col))
                    cols.Add(col);

            var separator = headers.Length > 1 ? "," : "";
            for (int row = csv.RowLimit.Min; row <= csv.RowLimit.Max; row++)
            {
                var rowVal = "";
                foreach (var col in cols)
                    rowVal += (csv.Records.TryGetValue((row, col), out object val) ? val.ToString() : "") + separator;

                res.TryAdd(separator.Length > 0 ? rowVal.TrimEnd(',') : rowVal, row);
            }

            return res;
        }
    }
}
