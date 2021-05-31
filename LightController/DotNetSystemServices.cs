using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;
using SPOT.Components.RTC_DS3231;

namespace JFarlette.LightController
{
    class DotNetSystemServices : ISystemServices
    {
        RTC_DS3231 m_rtc = new RTC_DS3231();

        public void SetLocalTime(DateTime dt)
        {
            // Set the current time
            m_rtc.SetDateTime(dt);
            Utility.SetLocalTime(dt);
        }

        public DateTime Now { get { return m_rtc.GetDateTime(); } }

        public void Sleep(int ms)
        {
            Thread.Sleep(ms);
        }
    }
}
