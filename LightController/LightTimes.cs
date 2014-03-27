using Astronomy;
using System;

namespace JFarlette.LightController
{
    class LightTimes
    {
        public LightTimes(DateTime dt, Config config)
        {
            // Assuming SunCalculator is returning high noon for sunrise 
            // and sunset in the winter
            SunCalculator sc = new SunCalculator(config.SiteCoords.Longitude,
                                    config.SiteCoords.Latitude,
                                    config.UTCOffset * 15,
                                    config.IsDstInEffect(dt));

            Sunrise = sc.CalculateSunRise(dt);
            LightsOffAM = Sunrise + config.SunriseOffset;

            Sunset = sc.CalculateSunSet(dt);
            LightsOnPM = Sunset + config.SunsetOffset;

            LightsOnAM = dt.Date + config.AMLightsOn;

            LightsOffPM = dt.Date + config.PMLightsOff;
        }

        public DateTime Sunrise;
        public DateTime Sunset;
        public DateTime LightsOnAM;
        public DateTime LightsOffAM;
        public DateTime LightsOnPM;
        public DateTime LightsOffPM;
    }
}
