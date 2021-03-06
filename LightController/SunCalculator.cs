﻿﻿// SunCalculator.cs - Calculator for calculating SunRise, SunSet and
// maximum solar radiation of a specific date and time.
//
// Patrick Kalkman  pkalkie@gmail.com
//
// (C) Patrick Kalkman http://www.semanticarchitecture.net
//
/* Ported to NETMF to run on a FEZ Panda 
 * Known issues:
 *   - Does not handle sunrise and sunset properly during winter and summer in polar latitudes
 */
using System;
using GHIElectronics.NETMF.System;

namespace Astronomy
{
    /// <summary>
    /// This class is responsible for calculating sun related parameters such as
    /// SunRise, SunSet and maximum solar radiation of a specific date and time.
    /// </summary>
    public class SunCalculator
    {
        private readonly double longitude;
        private readonly double latituteInRadians;
        private readonly double longituteTimeZone;
        private readonly bool useSummerTime;

        public SunCalculator()
        {
        }

        public SunCalculator(double longitude, double latitude, double longituteTimeZone, bool useSummerTime)
        {
            this.longitude = longitude;
            latituteInRadians = ConvertDegreeToRadian(latitude);
            this.longituteTimeZone = longituteTimeZone;
            this.useSummerTime = useSummerTime;
        }

        public DateTime CalculateSunRise(DateTime dateTime)
        {
            int dayNumberOfDateTime = ExtractDayNumber(dateTime);
            double differenceSunAndLocalTime = CalculateDifferenceSunAndLocalTime(dayNumberOfDateTime);
            double declanationOfTheSun = CalculateDeclination(dayNumberOfDateTime);
            double tanSunPosition = CalculateTanSunPosition(declanationOfTheSun);
            int sunRiseInMinutes = CalculateSunRiseInternal(tanSunPosition, differenceSunAndLocalTime);
            return CreateDateTime(dateTime, sunRiseInMinutes);
        }

        public DateTime CalculateSunSet(DateTime dateTime)
        {
            int dayNumberOfDateTime = ExtractDayNumber(dateTime);
            double differenceSunAndLocalTime = CalculateDifferenceSunAndLocalTime(dayNumberOfDateTime);
            double declanationOfTheSun = CalculateDeclination(dayNumberOfDateTime);
            double tanSunPosition = CalculateTanSunPosition(declanationOfTheSun);
            int sunSetInMinutes = CalculateSunSetInternal(tanSunPosition, differenceSunAndLocalTime);
            return CreateDateTime(dateTime, sunSetInMinutes);
        }

        public double CalculateMaximumSolarRadiation(DateTime dateTime)
        {
            int dayNumberOfDateTime = ExtractDayNumber(dateTime);
            double differenceSunAndLocalTime = CalculateDifferenceSunAndLocalTime(dayNumberOfDateTime);
            int numberOfMinutesThisDay = GetNumberOfMinutesThisDay(dateTime, differenceSunAndLocalTime);
            double declanationOfTheSun = CalculateDeclination(dayNumberOfDateTime);
            double sinSunPosition = CalculateSinSunPosition(declanationOfTheSun);
            double cosSunPosition = CalculateCosSunPosition(declanationOfTheSun);
            double sinSunHeight = sinSunPosition + cosSunPosition * MathEx.Cos(2.0 * Math.PI * (numberOfMinutesThisDay + 720.0) / 1440.0) + 0.08;
            double sunConstantePart = MathEx.Cos(2.0 * Math.PI * dayNumberOfDateTime);
            double sunCorrection = 1370.0 * (1.0 + (0.033 * sunConstantePart));
            return CalculateMaximumSolarRadiationInternal(sinSunHeight, sunCorrection);
        }

        internal double CalculateDeclination(int numberOfDaysSinceFirstOfJanuary)
        {
            return MathEx.Asin(-0.39795 * MathEx.Cos(2.0 * Math.PI * (numberOfDaysSinceFirstOfJanuary + 10.0) / 365.0));
        }

        private static int ExtractDayNumber(DateTime dateTime)
        {
            return dateTime.DayOfYear;
        }

        private static DateTime CreateDateTime(DateTime dateTime, int timeInMinutes)
        {
            int hour = timeInMinutes / 60;
            int minute = timeInMinutes - (hour * 60);
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, 00);
        }

        private static int CalculateSunRiseInternal(double tanSunPosition, double differenceSunAndLocalTime)
        {
            int sunRise = (int)(720.0 - 720.0 / Math.PI * MathEx.Acos(-tanSunPosition) - differenceSunAndLocalTime);
            sunRise = LimitSunRise(sunRise);
            return sunRise;
        }


        private static int CalculateSunSetInternal(double tanSunPosition, double differenceSunAndLocalTime)
        {
            int sunSet = (int)(720.0 + 720.0 / Math.PI * MathEx.Acos(-tanSunPosition) - differenceSunAndLocalTime);
            sunSet = LimitSunSet(sunSet);
            return sunSet;
        }

        private double CalculateTanSunPosition(double declanationOfTheSun)
        {
            double sinSunPosition = CalculateSinSunPosition(declanationOfTheSun);
            double cosSunPosition = CalculateCosSunPosition(declanationOfTheSun);
            double tanSunPosition = sinSunPosition / cosSunPosition;
            tanSunPosition = LimitTanSunPosition(tanSunPosition);
            return tanSunPosition;
        }

        private double CalculateCosSunPosition(double declanationOfTheSun)
        {
            return MathEx.Cos(latituteInRadians) * MathEx.Cos(declanationOfTheSun);
        }

        private double CalculateSinSunPosition(double declanationOfTheSun)
        {
            return MathEx.Sin(latituteInRadians) * MathEx.Sin(declanationOfTheSun);
        }

        private double CalculateDifferenceSunAndLocalTime(int dayNumberOfDateTime)
        {
            double ellipticalOrbitPart1 = 7.95204 * MathEx.Sin((0.01768 * dayNumberOfDateTime) + 3.03217);
            double ellipticalOrbitPart2 = 9.98906 * MathEx.Sin((0.03383 * dayNumberOfDateTime) + 3.46870);

            double differenceSunAndLocalTime = ellipticalOrbitPart1 + ellipticalOrbitPart2 + (longitude - longituteTimeZone) * 4;

            if (useSummerTime)
                differenceSunAndLocalTime -= 60;
            return differenceSunAndLocalTime;
        }

        private static double LimitTanSunPosition(double tanSunPosition)
        {
            if (((int)tanSunPosition) < -1)
            {
                tanSunPosition = -1.0;
            }
            if (((int)tanSunPosition) > 1)
            {
                tanSunPosition = 1.0;
            }
            return tanSunPosition;
        }

        private static int LimitSunSet(int sunSet)
        {
            if (sunSet > 1439)
            {
                sunSet -= 1439;
            }
            return sunSet;
        }

        private static int LimitSunRise(int sunRise)
        {
            if (sunRise < 0)
            {
                sunRise += 1440;
            }
            return sunRise;
        }

        private static double ConvertDegreeToRadian(double degree)
        {
            return degree * Math.PI / 180;
        }

        private static double Abs(double d)
        {
            return (d < 0 ? d * -1 : d);
        }

        private static double CalculateMaximumSolarRadiationInternal(double sinSunHeight, double sunCorrection)
        {
            double maximumSolarRadiation;
            if ((sinSunHeight > 0.0) && Abs(0.25 / sinSunHeight) < 50.0)
            {
                maximumSolarRadiation = sunCorrection * sinSunHeight * MathEx.Exp(-0.25 / sinSunHeight);
            }
            else
            {
                maximumSolarRadiation = 0;
            }
            return maximumSolarRadiation;
        }

        private static int GetNumberOfMinutesThisDay(DateTime dateTime, double differenceSunAndLocalTime)
        {
            return dateTime.Hour * 60 + dateTime.Minute + (int)differenceSunAndLocalTime;
        }
    }
}