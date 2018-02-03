using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace SPOT.Components.RTC_DS3231
{
    public class RTC_DS3231 : IDisposable
    {
        private I2CDevice I2C;
        I2CDevice.I2CTransaction[] xaction;
        ushort DS3231_Address = 0x68;

        public void Dispose()
        {
            I2C.Dispose();
            xaction = null;
        }

        public RTC_DS3231()
        {
            //Create I2C object      
            I2CDevice.Configuration conf = new I2CDevice.Configuration(DS3231_Address, 100);
            I2C = new I2CDevice(conf);
        }

        public DateTime GetDateTime()
        {
            xaction = new I2CDevice.I2CTransaction[2];
            xaction[0] = I2CDevice.CreateWriteTransaction(new byte[] { 0x00 });
            byte[] ReturnedDateTime = new byte[7];
            xaction[1] = I2CDevice.CreateReadTransaction(ReturnedDateTime);

            if (I2C.Execute(xaction, 1000) == 0)
            {
                new Exception("Failed to send I2C data");
            }
            int sec = bcdToDec(ReturnedDateTime[0]) & 0x7f;
            int min = bcdToDec(ReturnedDateTime[1]);
            int hour = bcdToDec(ReturnedDateTime[2]) & 0x3f;
            int dayofweek = bcdToDec(ReturnedDateTime[3]);
            int dayofmonth = bcdToDec(ReturnedDateTime[4]);
            int month = bcdToDec(ReturnedDateTime[5]);
            int year = bcdToDec(ReturnedDateTime[6]) + 2000;

            DateTime dt = new DateTime(year, month, dayofmonth, hour, min, sec);
            return dt;
        }

        public void SetDateTime(DateTime datetime)
        {
            xaction = new I2CDevice.I2CWriteTransaction[1];
            byte[] sb = new byte[8] { 0x00,
                                   decToBcd(datetime.Second),
                                   decToBcd(datetime.Minute),
                                   decToBcd(datetime.Hour),
                                   decToBcd((int)datetime.DayOfWeek),
                                   decToBcd(datetime.Day),  
                                   decToBcd(datetime.Month),
                                   decToBcd(datetime.Year - 2000)  
                                   };

            xaction[0] = I2CDevice.CreateWriteTransaction(sb);

            if (I2C.Execute(xaction, 1000) == 0)
            {
                new Exception("Failed to send I2C data");
            }
        }
        private byte decToBcd(int val)
        {
            return (byte)((val / 10 * 16) + (val % 10));
        }
        private byte bcdToDec(byte val)
        {
            return (byte)((val / 16 * 10) + (val % 16));
        }
    }
}
