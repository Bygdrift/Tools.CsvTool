# Bygdrift.Tools.Csv

A tool to import or construct, modify, filter and export csv files.

The tool is build to work fast with large amounts of data.

It's available on NuGet as `Install-Package Bygdrift.CsvTools`

It is written to work with dynamic data so you don't have to write properties or poco's.

## Get started

When working with rows and columns, the notation is like in Excel with the row first and then the column - called R1C1 notation:

|    | C1  | C2  |
|----|-----|-----|
| R1 | 1,1 | 1,2 |
| R2 | 2,1 | 2,2 |

You can however start with 0,0 or 5,8, but it's good practice to start with 1,1.

Create CSV with this data in four different ways:

| Id | Name   |
|----|--------|
| A  | Anders |
| B  | Bo     |

```c#
//1: Each header and record is defined with row and column index:
Csv csv1 = new Csv()
            .AddHeader(1, "Id").AddHeader(2, "Name")
            .AddRecord(1, 1, "A").AddRecord(1, 2, "Anders")
            .AddRecord(2, 1, "B").AddRecord(2, 2, "Bo");

//2: Only records are added and the headers gets created if they not already are in the csv:
Csv csv2 = new Csv()
            .AddRecord(1, "Id", "A").AddRecord(1, "Name", "Anders")
            .AddRecord(2, "Id", "B").AddRecord(2, "Name", "Bo");


//3: Headers are initiated when creating new Csv in one comma separated string:
Csv csv3 = new Csv("Id, Name").AddRow("A", "Anders").AddRow("B","Bo");

//4: With AddRows, you can paste in comma separated string and each string will become a row:
Csv csv4 = new Csv("Id, Name").AddRows("A, Anders", "B, Bo");

```

Inside a Csv class, you can see each column type. From previous example, both Id and Name would be a string, but if it had been: `new Csv("Id, Name").AddRows("1, Anders", "2, Bo")`, then Id would be an `int` and it can be shown with `csv.ColTypes[1];`. It has been implemented so data from csv, easily can be transferred into a database with my other GitHub project [Bygdrift.Warehouse](https://github.com/Bygdrift/Warehouse).

## How to import

Import data and convert to csv:

```c#
//Add a csv into another csv:
Csv csv = new Csv("Id, Name").AddRows("A, Anders").AddCsv(new Csv("Id, Name").AddRows("B, Bo"));

Csv csv = new Csv().AddCsvFile(filePath);

Csv csv = new Csv().AddCsvStream(stream);

Csv csv = new Csv().AddDataTable(dataTable);

Csv csv = new Csv().AddExcelFile(filePath, paneNumber, rowStart, colStart);

Csv csv = new Csv().AddExcelStream(stream, paneNumber, rowStart, colStart);

Csv csv = new Csv().AddExpandoObjects(expandoObjects);

Csv csv = new Csv().AddJson(jsonString);
```

## Build csv:

The csv:
| age |         date        | name  |
|-----|---------------------|-------|
|  21 | 26-09-2021 11:46:51 | Peter |


```c#
//Create a csv that already has two columns with the headers age and B
var csv = new Csv("age, date");

//Add a third column
csv.AddHeader(2, "name");

//Add one row:
csv.AddRecord(1, 1, "21");
csv.AddRecord(1, 2, "26-09-2021 11:46:51");
csv.AddRecord(1, 3, "Peter");

//Make tests:
Assert.IsTrue(csv.Records.First().Value.ToString() == "21");
Assert.IsTrue(csv.ColTypes[1] == typeof(long));
Assert.IsTrue(csv.ColTypes[2] == typeof(DateTime));
Assert.IsTrue(csv.ColTypes[3] == typeof(string));
Assert.IsTrue(csv.RowCount == 1);
Assert.IsTrue(csv.ColCount == 3);
Assert.IsTrue(csv.RowLimit == (1, 1));  //The min an max row limit
Assert.IsTrue(csv.ColLimit == (1, 3));  //The min an max column limit

//Remove a colum:
csv.RemoveColumn(2);
```


## Filtering:

```c#
//Filter a column and get a csv, that only contains rows that has a column with the name "Peter"
Csv res = csv.FilterRows("name", "Peter");
//Can also filter on multiple values in a column:
Csv res = csv.FilterRows("name", "Anders", "Peter", "Bo");
```

## Get data from csv
Get methods like getting a specific row, record or column:

```c#
//Get a record:
object record = csv.GetRecord(1, 1);

//Get a row (return dictionary where key = colId as int and value = the record value as object):
Dictionary<int, object> row = csv.GetRowRecords(1);

//Get all rows as a a dictionary:
Dictionary<int, Dictionary<int, object>> rows = csv.GetRowsRecords();

//Get specific rows where header = name and the headers content = age:
Dictionary<int, Dictionary<int, object>> rows = csv.GetRowsRecords("name", "age");

//Try get a col index:
csv.TryGetColId("name", out int colIndex);  // colIndex = 3

//Get a col (return dictionary where key = rowId as int and value = the record value as object):
Dictionary<int, object> col = csv.GetColRecords(1);
Dictionary<int, object> col = csv.GetColRecords("name");

//Get colum 'age', where column 'name' equals 'Peter'
Dictionary<int, object> col = csv.GetColRecords("name", "age", "Peter");  
```

## Culture:

```c#
//In Denmark a decimal is written like '5,2'.
//Here with invariant culture that gives decimal '52':
Assert.AreEqual(new Csv().AddRow("5,2").GetRecord(1,1), 52m);
//Here with danish culture that gives decimal '5.2'
Assert.AreEqual(new Csv().Culture("da-DK").AddRow("5,2").GetRecord(1, 1), 5.2m);
```

## Export:

Examples on how to export csv to different formats:

```c#
//Export to a csv-file:
csv.ToCsvFile(filepath);
//Or as a stream:
Stream stream = csv.ToCsvStream();

//As a DataTable *):
DataTable table = csv.ToDataTable();

//As an Expandoobject:
IEnumerable<Dictionary<string, object>> res = csv.ToExpandoList();
//Or as a dynamic:
dynamic res = csv.ToExpandoList();

//Export csv to excel. No fine features - only pure .xlsx:
csv.ToExcelFile(filepath, paneName);
//Or as a stream:
Stream stream = csv.ToExcelStream(paneName);

//Export csv to excel as a table in a worksheet:
csv.ToExcelFile(filepath, paneName, tableName);
//Or as a stream:
Stream stream = csv.ToExcelStream(paneName, tableName);

//Export to json as a Newtonsoft JArray.
JArray jArray = csv.ToJArray();

//Export csv to json:
string json = csv.ToJson();
```

**) More info from Microsoft about [DataTables](https://docs.microsoft.com/en-us/dotnet/api/system.data.datatable?f1url=%3FappId%3DDev16IDEF1%26l%3DEN-US%26k%3Dk(System.Data.DataTable);k(DevLang-csharp)%26rd%3Dtrue&view=net-6.0)*

## Examples on import and export:

```c#
//Convert a json string to csv:
new Csv().AddJson("[{\"a\":1,\"b\":2},{\"b\":3},{\"a\":4}]");

//Convert a csv file to Excel:
new Csv().AddCsvFile(path).ToExcelFile(Path, paneName);

//Convert a csv file to json:
string json = new Csv().AddCsvFile(path).ToJson();
```

### There are more functions inside.

## Contact

For information or consultant hours, please write to bygdrift@gmail.com.

# Updates

## 1.1.2
Breaking changes: The following methods has changed names: FromCsvFile => AddCsvFile, FromCsvStream => AddCsvStream, FromDataTable => AddDataTable, FromExcelFile => AddExcelFile, FromExcelStream => AddExcelStream, FromExpandoObjects => AddExpandoObjects, FromJson => AddJson.
Now they all can add data to an existing csv like:
```c#
Csv csv = new Csv("Id, Name").AddRows("A, Anders");
Csv csvIn = new Csv("Id, Name").AddRows("B, Bo");
csv.AddCsv(csvIn);
```
And csv equals:
| Id | Name   |
|----|--------|
| A  | Anders |
| B  | Bo     |

## 1.0.2
Added ability to handle Culture with new Csv().Culture();
Better to pinpoint the type of a string date in the CSV. So a column containing ex '2022-11-20' or '20-11-2022' and so on, are now better recognized.

## 1.0.1
Added function so a csv can be merged into another csv: FromCsv(Csv mergedCsv, bool createNewUniqueHeaderIfAlreadyExists)
Added function so a column with the same data, can be added: AddColumn(string headerName, object value, bool createNewUniqueHeaderIfAlreadyExists)

## 1.0.0
This package is now stable and ready for a production version.

## 0.4.3
Added function public Csv AddRecord(int row, string headerName, object value, bool createNewUniqueHeaderIfAlreadyExists = false)
So now it's possible to add a record by giving the header name and if the header name not already exists, it will be created.