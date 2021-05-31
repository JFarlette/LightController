using System;
using NetMf.CommonExtensions;


namespace JFarlette.LightController
{
    static class TimeSpanUtility
    {
        static public string FormatTimespanSHHMM(TimeSpan ts)
        {
            int hr = System.Math.Abs(ts.Hours);
            int min = System.Math.Abs(ts.Minutes);

            return StringUtility.Format("{0}{1}{2}:{3}{4}",
                                        ts.Ticks < 0 ? "-" : "",
                                        hr < 10 ? "0" : "", hr,
                                        min < 10 ? "0" : "", min);
        }

        static public string FormatTimespanSHHMMSS(TimeSpan ts)
        {
            int hr = System.Math.Abs(ts.Hours);
            int min = System.Math.Abs(ts.Minutes);
            int sec = System.Math.Abs(ts.Seconds);
            int millis = System.Math.Abs(ts.Milliseconds);

            return StringUtility.Format("{0}{1}{2}:{3}{4}:{5}{6}.{7}",
                                        ts.Ticks < 0 ? "-" : "",
                                        hr < 10 ? "0" : "", hr,
                                        min < 10 ? "0" : "", min,
                                        sec < 10 ? "0" : "", sec,
                                        millis);
        }

        static public int TimeSpanTotalMilliseconds(TimeSpan ts)
        {
            int ms = ts.Days * 24 * 60 * 60 * 1000 +
                      ts.Hours * 60 * 60 * 1000 +
                      ts.Minutes * 60 * 1000 +
                      ts.Seconds * 1000 +
                      ts.Milliseconds;
            return ms;
        }
    }
}
