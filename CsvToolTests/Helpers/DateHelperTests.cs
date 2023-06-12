using Bygdrift.Tools.CsvTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvToolTests.Helpers
{
    [TestClass]
    public class DateHelperTests
    {
        [TestMethod]
        public void TestDateParse()
        {
            var csv = new Csv("Date");
            csv.AddRow(
                DateTime.Parse("2022-3-9T03:59:39"),
                "2022-1-1 01:00:00",
                "2022-03-09T03:59:39",
                "2022-03-10T10:01:53+01:00",
                "2022-03-10T10:01:53.000+01:00",
                "2022-06-01T12:00:00",
                "2022-06-01T06:00:00Z"
            //"2022-06-01T12:00:00-06:00"  //Er DateTimeOffset
            );

            foreach (var item in csv.ColTypes)
                Assert.AreEqual(item.Value, typeof(DateTime));
        }

        [TestMethod]
        public void Daylight()
        {
            var time = new DateTime(2022, 6, 1, 12, 0, 0);

            var g = time.ToUniversalTime();

            //var time = new DateTimeOffset(2022, 12, 1, 12, 0, 0, new TimeSpan(2, 0, 0));
            var a = time.ToLocalTime();
            var csv = new Csv(new Config(null, "Romance Standard Time", FormatKind.TimeOffsetUTC));
            var h1 = csv.Config.DateHelper.ToDateTimeOffset(time);
            var h2 = csv.Config.DateHelper.DateTimeToString(time);

            DateTime utc = DateTime.UtcNow;
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
            DateTime localDateTime = TimeZoneInfo.ConvertTime(time, zone);
            var hj = zone.IsDaylightSavingTime(time);

            //var g = csv.DateHelper.ToDateTime(time);
            //var h = time.LocalDateTime;
            //var v1 = new DateTimeOffset(2022, 12, 1, 12, 0, 0, new TimeSpan(2, 0, 0)).LocalDateTime;
            //var v2 = new DateTimeOffset(2022, 6, 1, 12, 0, 0, new TimeSpan(2, 0, 0)).LocalDateTime;
        }

        [TestMethod]
        public void TestDateToString()
        {
            var time = new DateTime(2022, 12, 1, 12, 0, 0);
            DateTime utc = DateTime.UtcNow;
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utc, zone);

            Assert.AreEqual("2022-12-01T12:00:00+01:00", new Csv(new Config(null, "Romance Standard Time", FormatKind.TimeOffsetUTC)).Config.DateHelper.DateTimeToString(time));

            Assert.AreEqual("2022-12-01T12:00:00", new Csv(new Config(null, "Central America Standard Time", FormatKind.Local)).Config.DateHelper.DateTimeToString(time));
            Assert.AreEqual("2022-12-01T06:00:00Z", new Csv(new Config(null, "Central America Standard Time", FormatKind.Universal)).Config.DateHelper.DateTimeToString(time));
            Assert.AreEqual("2022-12-01T12:00:00-06:00", new Csv(new Config(null, "Central America Standard Time", FormatKind.TimeOffsetUTC)).Config.DateHelper.DateTimeToString(time));

            Assert.AreEqual("2022-12-01T12:00:00", new Csv(new Config(null, "Romance Standard Time", FormatKind.Local)).Config.DateHelper.DateTimeToString(time));
            Assert.AreEqual("2022-12-01T13:00:00Z", new Csv(new Config(null, "Romance Standard Time", FormatKind.Universal)).Config.DateHelper.DateTimeToString(time));
            Assert.AreEqual("2022-12-01T12:00:00+01:00", new Csv(new Config(null, "Romance Standard Time", FormatKind.TimeOffsetUTC)).Config.DateHelper.DateTimeToString(time));

            var timeOffset = new DateTimeOffset(2022, 6, 1, 12, 0, 0, new TimeSpan(1, 0, 0));
        }

        [TestMethod]
        public void NowTest()
        {
            var utcNow = new DateTime(2022, 6, 1, 12, 0, 0).ToUniversalTime();
            Assert.AreEqual("2022-06-01T12:00:00+02:00", new Config(null, "Romance Standard Time", FormatKind.Local).DateHelper.Now(utcNow).ToString("yyyy-MM-ddTHH:mm:sszzz"));
            Assert.AreEqual("2022-06-01T10:00:00+00:00", new Config(null, "Romance Standard Time", FormatKind.Universal).DateHelper.Now(utcNow).ToString("yyyy-MM-ddTHH:mm:sszzz"));
            Assert.AreEqual("2022-06-01T12:00:00+02:00", new Config(null, "Romance Standard Time", FormatKind.TimeOffsetDST).DateHelper.Now(utcNow).ToString("yyyy-MM-ddTHH:mm:sszzz"));
            Assert.AreEqual("2022-06-01T12:00:00+01:00", new Config(null, "Romance Standard Time", FormatKind.TimeOffsetUTC).DateHelper.Now(utcNow).ToString("yyyy-MM-ddTHH:mm:sszzz"));
        }
    }
}
