using System;
using System.Collections;

namespace JFarlette.LightController
{
    class Config
    {
        public DateTime StartingDateTime = new DateTime(2014, 01, 02, 9, 0, 0);
            // Date and local time entered in setup 

        public SByte UTCOffset = -8;
            // Timezone offset from UTC: -8 is Pacific

        public bool IsDST = false;
            // Daylight Savings in effect?

        public TimeSpan SunsetOffset = new TimeSpan(0, 20, 0);
            // Offset from true sun set that should be considered sunset

        public TimeSpan SunriseOffset = new TimeSpan(0, -20, 0);
            // Offset from true sun rise that should be considered sunrise

        public TimeSpan AMLightsOn = new TimeSpan(6, 0, 0);
            // Time span from start of day until lights should be turned on in morning
            // If later than sunrise lights will not be turned on 

        public TimeSpan PMLightsOff = new TimeSpan(0, 30, 0);
            // Time span from start of day until lights should be turned off in evening
            // If before noon (eg 1 AM) indicates early next morning
            // If earlier than sunset lights will not be turned on


        public struct Coordinates
        {
            public Coordinates(double lat, double lng)
            {
                Latitude = lat;
                Longitude = lng;
            }

            public double Latitude;
            public double Longitude;
        }

        public Coordinates SiteCoords = new Coordinates(lat: 49.95121990866204, lng: -122.16796875);
            // Coordinates of the site.  Default coordinates: Vancouver Canada

        public string DATETIME_FORMAT = "yyyy-MM-dd HH:mm";
            // Format current date and time should be displayed in

        public string DATE_FORMAT = "yyyy-MM-dd";
            // Format current date should be displayed in

        public string TIME_FORMAT = "HH:mm";
            // Format times should be displayed in

        struct DstDate
        {
            public DstDate(DateTime start, DateTime end)
            {
                Start = start;
                End = end;
            }
            public bool IsDstInEffect(DateTime date)
            {
                return Start <= date && date < End;
            }

            DateTime Start;
            DateTime End;
        }
        static Hashtable DstDates;

        static Config()
        {
            DstDates = new Hashtable();
            DstDates.Add(2016, new DstDate(new DateTime(2016, 3, 13), new DateTime(2016, 11, 6)));
            DstDates.Add(2017, new DstDate(new DateTime(2017, 3, 12), new DateTime(2017, 11, 5)));
            DstDates.Add(2018, new DstDate(new DateTime(2018, 3, 11), new DateTime(2018, 11, 4)));
            DstDates.Add(2019, new DstDate(new DateTime(2019, 3, 10), new DateTime(2019, 11, 3)));
            DstDates.Add(2020, new DstDate(new DateTime(2020, 3, 8), new DateTime(2020, 11, 1)));
            DstDates.Add(2021, new DstDate(new DateTime(2021, 3, 14), new DateTime(2021, 11, 7)));
            DstDates.Add(2022, new DstDate(new DateTime(2022, 3, 13), new DateTime(2022, 11, 6)));
            DstDates.Add(2023, new DstDate(new DateTime(2023, 3, 12), new DateTime(2023, 11, 5)));
            DstDates.Add(2024, new DstDate(new DateTime(2024, 3, 10), new DateTime(2024, 11, 3)));
            DstDates.Add(2025, new DstDate(new DateTime(2025, 3, 9), new DateTime(2025, 11, 2)));
            DstDates.Add(2026, new DstDate(new DateTime(2026, 3, 8), new DateTime(2026, 11, 1)));
            DstDates.Add(2027, new DstDate(new DateTime(2027, 3, 14), new DateTime(2027, 11, 7)));
            DstDates.Add(2028, new DstDate(new DateTime(2028, 3, 12), new DateTime(2028, 11, 5)));
            DstDates.Add(2029, new DstDate(new DateTime(2029, 3, 11), new DateTime(2029, 11, 4)));
        }

        public bool IsDstInEffect(DateTime date)
        {
            if (!DstDates.Contains(date.Year)) return false;
            else
            {
                DstDate dd = (DstDate)DstDates[date.Year];
                return dd.IsDstInEffect(date);
            }
        }

    }
}
