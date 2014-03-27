using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.System;

namespace FEZ_Panda_Application1
{
    class Calculator
    {
        static private double AdjustToRange(double value, double min, double max)
        {
            if (value < min)
                value += max;
            if (value > max)
                value -= max;
            return value;
        }

        static private double Radians(double degrees)
        {
            return (System.Math.PI / 180) * degrees;
        }

        static private double Degrees(double radians)
        {
            return (180 / System.Math.PI) * radians;
        }

        static public DateTime GetSunSet(int dayOfYear)
        {
            /*
             Source:
	            Almanac for Computers, 1990
	            published by Nautical Almanac Office
	            United States Naval Observatory
	            Washington, DC 20392
             
             Inputs:
	            day, month, year:      date of sunrise/sunset
	            latitude, longitude:   location for sunrise/sunset
	            zenith:                Sun's zenith for sunrise/sunset
	                offical      = 90 degrees 50'
	                civil        = 96 degrees
	                nautical     = 102 degrees
	                astronomical = 108 degrees
	
	         NOTE: longitude is positive for East and negative for West
             NOTE: the algorithm assumes the use of a calculator with the
             trig functions in "degree" (rather than "radian") mode. Most
             programming languages assume radian arguments, requiring back
             and forth convertions. The factor is 180/pi. So, for instance,
             the equation RA = atan(0.91764 * tan(L)) would be coded as RA
             = (180/pi)*atan(0.91764 * tan((pi/180)*L)) to give a degree
             answer with a degree input for L.
            */

            double zenith = 96;
            double latitude = 49.23475;
            double longitude = -123.00808;
            double localOffsetFromUTC = -8;

            // 1. first calculate the day of the year
            // N1 = floor(275 * month / 9)
            // N2 = floor((month + 9) / 12)
            // N3 = (1 + floor((year - 4 * floor(year / 4) + 2) / 3))
            // N = N1 - (N2 * N3) + day - 30
            int N = dayOfYear;

            //2. convert the longitude to hour value and calculate an approximate time
            //    lngHour = longitude / 15
            double lngHour = longitude / 15;

            //    if rising time is desired:
            //      t = N + ((6 - lngHour) / 24)
            //    if setting time is desired:
            //      t = N + ((18 - lngHour) / 24)
            double t = N + ((18 - lngHour) / 24);

            //3. calculate the Sun's mean anomaly
            //    M = (0.9856 * t) - 3.289
            double M = (0.9856 * t) - 3.289;
            
            // 4. calculate the Sun's true longitude
	        // L = M + (1.916 * sin(M)) + (0.020 * sin(2 * M)) + 282.634
	        // NOTE: L potentially needs to be adjusted into the range [0,360) by adding/subtracting 360
            double L = AdjustToRange(M + (1.916 * MathEx.Sin(Radians(M))) + (0.020 * MathEx.Sin(Radians(2 * M))) + 282.634, 0, 360);

            // 5a. calculate the Sun's right ascension
            //    RA = atan(0.91764 * tan(L))
            //    NOTE: RA potentially needs to be adjusted into the range [0,360) by adding/subtracting 360
            double RA = AdjustToRange(Degrees(MathEx.Atan(0.91764 * MathEx.Tan(Radians(L)))), 0, 360);

            // 5b. right ascension value needs to be in the same quadrant as L
            // Lquadrant  = (floor( L/90)) * 90
            // RAquadrant = (floor(RA/90)) * 90
            // RA = RA + (Lquadrant - RAquadrant)
            double Lquadrant = MathEx.Floor(L / 90) * 90;
	        double RAquadrant = MathEx.Floor(RA/90) * 90;
            RA = RA + (Lquadrant - RAquadrant);

            // 5c. right ascension value needs to be converted into hours
	        // RA = RA / 15
            RA = RA / 15;

            // 6. calculate the Sun's declination
	        // sinDec = 0.39782 * sin(L)
	        // cosDec = cos(asin(sinDec))
            double sinDec = 0.39782 * MathEx.Sin(Radians(L));
            double cosDec = MathEx.Cos(MathEx.Asin(sinDec));

            // 7a. calculate the Sun's local hour angle
	        // cosH = (cos(zenith) - (sinDec * sin(latitude))) / (cosDec * cos(latitude))

            double cosH = (MathEx.Cos(Radians(zenith)) - (sinDec * MathEx.Sin(Radians(latitude)))) / (cosDec * MathEx.Cos(Radians(latitude)));

            // if (cosH >  1) 
            //    the sun never rises on this location (on the specified date)
            // if (cosH < -1)
            //    the sun never sets on this location (on the specified date)

            // 7b. finish calculating H and convert into hours
            // if if rising time is desired:
	        //     H = 360 - acos(cosH)
	        // if setting time is desired:
	        //    H = acos(cosH)
            double H = Degrees(MathEx.Acos(cosH));

            // 	H = H / 15
            H = H / 15;

            // 8. calculate local mean time of rising/setting
            // T = H + RA - (0.06571 * t) - 6.622
            double T = H + RA - (0.06571 * t) - 6.622;

            // 9. adjust back to UTC
            // UT = T - lngHour
            // NOTE: UT potentially needs to be adjusted into the range [0,24) by adding/subtracting 24
            double UT = AdjustToRange(T - lngHour, 0, 24);

            // 10. convert UT value to local time zone of latitude/longitude
        	// localT = UT + localOffset
            double localT = UT + localOffsetFromUTC;

            int Hour = (int)(localT / 60);
            double remainder  = localT - Hour;
            int Min =  (int)(remainder / 60);
            remainder = remainder - Min;
            int Sec = (int)(remainder);

            return DateTime.Today.Add(new TimeSpan(Hour, Min, Sec));
        }

        static public DateTime GetSunRise(int dayOfYear)
        {
            return DateTime.Today.Add(new TimeSpan(24 + 7, 0, 0));
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
