using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace JFarlette.LightController
{
    class DotNetSystemServices : ISystemServices
    {
        public void SetLocalTime(DateTime dt)
        {
            // Set the current time
            Utility.SetLocalTime(dt);
        }

        public DateTime Now { get { return DateTime.Now; } }

        public void Sleep(int ms)
        {
            Thread.Sleep(ms);
        }
    }
}
