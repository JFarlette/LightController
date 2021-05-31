using System;
using System.Threading;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT.Hardware;
using JFarlette.LightController.LCD_2x16;
using GHIElectronics.NETMF.Hardware;

using Microsoft.SPOT;


namespace JFarlette.LightController
{
    static class Program
    {
        public static void Main()
        {
            //TimeSpanTest();
            // RelayTest();
            //ControllerTest();
            StartController();

        }

        static void StartController()
        {
            FEZ_Shields.KeypadLCD.Initialize();

            LightRelay LR = new LightRelay((Cpu.Pin)FEZ_Pin.Digital.IO44);

            ISystemServices system = new DotNetSystemServices();
            Debug.Print("Date & Time: " + system.Now.ToString(Config.DATETIME_DEBUG_FORMAT));

            Config config = new Config(system.Now);

            bool useDefaultSetup = system.Now.Year > 2000;
            do
            {
                if (!useDefaultSetup)
                {
                    Setup_Lcd_2x16.DoSetup(config, system);
                }
                useDefaultSetup = false;

                Controller lc = new Controller(config, system, LR);
                
                lc.Start();

                Monitor_Lcd_2x16.DoMonitor(lc, config, system);

                lc.Abort();

                config.UpdateStartingDateTime(system.Now);
            }
            while (true);
        }

        
        
        static void ControllerTest()
        {
            FEZ_Shields.KeypadLCD.Initialize();

            LightRelay LR = new LightRelay((Cpu.Pin)FEZ_Pin.Digital.IO44);

            Config config = new Config();

            ISystemServices system = new TestSystemServices();

            system.SetLocalTime(config.StartingDateTime);

            Controller lc = new Controller(config, system, LR);
            lc.Start();
        }

     
        static void RelayTest()
        {
            //LightRelay LR = new LightRelay((Cpu.Pin)FEZ_Pin.Digital.IO14);
            LightRelay LR = new LightRelay((Cpu.Pin)FEZ_Pin.Digital.IO44);
            while (true)
            {
                if (LR.IsTurnedOn())
                    LR.TurnOff();
                else
                    LR.TurnOn();
                Thread.Sleep(2000);
            }
        }

        static void TimeSpanTest()
        {
            DateTime dt1 = new DateTime(2018, 10, 08, 5, 4, 8);
            DateTime dt2 = new DateTime(2018, 10, 08, 5, 30, 0);
            TimeSpan ts = dt2 - dt1;
            Debug.Print("dt2: " + dt2.ToString() + " - dt1: " + dt1.ToString() + " = " + ts.ToString());

        }
    }

}
