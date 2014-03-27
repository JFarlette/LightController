using System;

using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT.Hardware;
using JFarlette.LightController.LCD_2x16;

namespace JFarlette.LightController
{
    static class Program
    {
        public static void Main()
        {
            //Test();
            StartController();
        }

        static void StartController()
        {
            FEZ_Shields.KeypadLCD.Initialize();

            LightRelay LR = new LightRelay((Cpu.Pin)FEZ_Pin.Digital.Di4);

            Config config = new Config();

            Setup_Lcd_2x16.DoSetup(config);

            DotNetSystemServices system = new DotNetSystemServices();

            system.SetLocalTime(config.StartingDateTime);

            Controller lc = new Controller(config, system, LR);

            Monitor_Lcd_2x16.DoMonitor(lc, config, system);

            lc.Control();
        }
        
        static void Test()
        {
            LightRelay LR = new LightRelay((Cpu.Pin)FEZ_Pin.Digital.Di4);

            Config config = new Config();

            TestSystemServices system = new TestSystemServices();

            system.SetLocalTime(config.StartingDateTime);

            Controller lc = new Controller(config, system, LR);
            lc.Control();
        }
    }

}
