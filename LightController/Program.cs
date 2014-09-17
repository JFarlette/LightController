using System;
using System.Threading;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT.Hardware;
using JFarlette.LightController.LCD_2x16;


namespace JFarlette.LightController
{
    static class Program
    {
        public static void Main()
        {
            // RelayTest();
            // ControllerTest();
            StartController();
        }

        static void StartController()
        {
            FEZ_Shields.KeypadLCD.Initialize();

            LightRelay LR = new LightRelay((Cpu.Pin)FEZ_Pin.Digital.IO14);

            Config config = new Config();

            Setup_Lcd_2x16.DoSetup(config);

            DotNetSystemServices system = new DotNetSystemServices();

            system.SetLocalTime(config.StartingDateTime);

            Controller lc = new Controller(config, system, LR);

            Monitor_Lcd_2x16.DoMonitor(lc, config, system);

            lc.Control();
        }
        
        static void ControllerTest()
        {
            LightRelay LR = new LightRelay((Cpu.Pin)FEZ_Pin.Digital.IO14);

            Config config = new Config();

            TestSystemServices system = new TestSystemServices();

            system.SetLocalTime(config.StartingDateTime);

            Controller lc = new Controller(config, system, LR);
            lc.Control();
        }

     
        static void RelayTest()
        {
            LightRelay LR = new LightRelay((Cpu.Pin)FEZ_Pin.Digital.IO14);
            while (true)
            {
                if (LR.IsTurnedOn())
                    LR.TurnOff();
                else
                    LR.TurnOn();
                Thread.Sleep(2000);
            }
        }
    }

}
