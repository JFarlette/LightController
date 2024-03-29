using System;
using System.Collections;

namespace JFarlette.LightController
{
    class Config
    {
        public bool IsModeManual = false;
        public bool IsLightOn = false;

        public DateTime StartingDateTime = new DateTime(2017, 09, 1, 22, 0, 0);
            // Date and local time entered in setup 

        public SByte UTCOffset = -8;
            // Timezone offset from UTC: -8 is Pacific

        public bool IsDST = true;
            // Daylight Savings in effect?

        public TimeSpan SunsetOffset = new TimeSpan(0, 5, 0);
            // Offset from true sun set that should be considered sunset

        public TimeSpan SunriseOffset = new TimeSpan(0, -5, 0);
            // Offset from true sun rise that should be considered sunrise

        public TimeSpan AMLightsOn = new TimeSpan(5, 30, 0);
            // Time span from start of day until lights should be turned on in morning
            // If later than sunrise lights will not be turned on 

        public TimeSpan PMLightsOff = new TimeSpan(0, 30, 0);
            // Time span from start of day until lights should be turned off in evening
            // If before noon (eg 1 AM) indicates early next morning
            // If earlier than sunset lights will not be turned on

        public Config(DateTime dt)
        {
            // Try to guess IsDST value based on current date and time
            IsDST = IsDstInEffect(dt);
        }

        public Config()
        {
        }

        public void UpdateStartingDateTime(DateTime startingDT)
        {
            StartingDateTime = startingDT;
            IsDST = IsDstInEffect(startingDT);
        }

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

        public static string DATETIME_FORMAT = "yyyy-MM-dd HH:mm";
            // Format current date and time should be displayed in

        public static string DATETIME_DEBUG_FORMAT = "yy-MM-dd HH:mm:ss.fff";
            // include seconds 

        public static string DATE_FORMAT = "yyyy-MM-dd";
            // Format current date should be displayed in

        public static string TIME_FORMAT = "HH:mm";
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
            DstDates.Add(2023, new DstDate(new DateTime(2023, 3, 12), new DateTime(2023, 11, 5)));
            DstDates.Add(2024, new DstDate(new DateTime(2024, 3, 10), new DateTime(2024, 11, 3)));
            DstDates.Add(2025, new DstDate(new DateTime(2025, 3, 9), new DateTime(2025, 11, 2)));
            DstDates.Add(2026, new DstDate(new DateTime(2026, 3, 8), new DateTime(2026, 11, 1)));
            DstDates.Add(2027, new DstDate(new DateTime(2027, 3, 14), new DateTime(2027, 11, 7)));
            DstDates.Add(2028, new DstDate(new DateTime(2028, 3, 12), new DateTime(2028, 11, 5)));
            DstDates.Add(2029, new DstDate(new DateTime(2029, 3, 11), new DateTime(2029, 11, 4)));
            DstDates.Add(2030, new DstDate(new DateTime(2030, 3, 10), new DateTime(2030, 11, 3)));
            DstDates.Add(2031, new DstDate(new DateTime(2031, 3, 9), new DateTime(2031, 11, 2)));
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
