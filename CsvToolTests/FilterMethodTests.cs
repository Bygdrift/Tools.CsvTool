using Bygdrift.Tools.CsvTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvToolTests
{
    [TestClass]
    public class FilterMethodTests
    {

        [TestMethod]
        public void Filter()
        {
            var csv = CreateCsv();
            var res = csv.FilterRows("Type", true, "Room");
            Assert.IsTrue(res.ColLimit == (1, 7));
            Assert.IsTrue(res.RowLimit == (1, 3));
            Assert.AreEqual(res.ColTypes[1], csv.ColTypes[1]);

            //var csv2 = new Csv("a, b, c").AddRow(null, 1, null);
            //var csv2Filtered = csv2.FilterRows("b", true, 1);


        }

        private static Csv CreateCsv()
        {
            var csv = new Csv("Mail,Capacity,Location,Level,Building,Name,Type");
            csv.AddRows(new[] {
                "andegaarden-1.sal@xyz.dk,6,Trollesmindealle 27,1. sal,Rådhus,Andergården,Room",
                "byraadssalen-moedecenter@xyz.dk,30,Trollesmindealle 27,Stuen,Rådhus,Byrådssalen,Room",
                "BilABA97100@xyz.dk,,Ukendt,,,Ældre og sundhed - Bil BA 97100,Vehicle",
                "BocenterMoedelokale3@xyz.dk,3,Ukendt,,,Bocenter - Mødelokale 3,Room",
                "Bocentrets_faelleskalender@xyz.dk,,Ukendt,,,Bocenter-fælleskalender,Calendar",
                "BSS-Bil-CH16136@xyz.dk,,Ukendt,,,BSS Bil CH16136,Vehicle",
                "ByogMiljo-Bil3@xyz.dk,,Ukendt,,,By og Miljø - Bil 3,Vehicle",
                "byogmiljobil5@xyz.dk,,Ukendt,,,By og Miljø - Bil 5,Vehicle",
                "byogmiljobil6@xyz.dk,,Ukendt,,,By og Miljø - Bil 6,Vehicle",
                "ByogMiljo-Stor_Projektor@xyz.dk,,Ukendt,,,By og Miljø – Stor Projektor,Asset",
            });
            return csv;
        }
    }
}
