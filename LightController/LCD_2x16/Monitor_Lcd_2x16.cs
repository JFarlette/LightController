using System;
using System.Threading;
using Microsoft.SPOT;
using GHIElectronics.NETMF.FEZ;
using NetMf.CommonExtensions;

namespace JFarlette.LightController.LCD_2x16
{
    // Displays the status of the controller on a 2x16 LCD with integrated key pad through the FEZ_Shields.KeypadLCD driver
    static class Monitor_Lcd_2x16
    {
        static public void DoMonitor(Controller controller, Config config, ISystemServices sr)
        {
            s_controller = controller;
            s_config = config;
            s_schedulingResources = sr;

            s_screenUpdate = new Timer(new TimerCallback(UpdateMonitorScreen),
                                            null,
                                            2 * 1000,  // Wait 2 seconds to let the controller start
                                            5 * 1000); // Update every 5 seconds

            s_monitorThread = new Thread(new ThreadStart(DimKeyPad));
            s_monitorThread.Start();
        }

        /* Implements three monitor screens for viewing the status of the controller:
            ----------------|
            2013-01-23 13:44|
            Sun: Up  Lt: Off|
            ----------------|
            Sunrise: 05:45  |
            Lt: 05:00-06:15 |
            ----------------|
            Sunset: 16:45   |
            Lt: 17:05-01:00 |
            -----------------
        */
        enum StatusScreens
        {
            CurrentStatus,
            Sunrise,
            Sunset,
            TotalScreens
        }

        static private void DimKeyPad()
        {
            // Monitor and dim the LCD when idle
            do
            {
                FEZ_Shields.KeypadLCD.Keys k = FEZ_Shields.KeypadLCD.PauseForAnyKey(2 * 60 * 1000);
                if (k == FEZ_Shields.KeypadLCD.Keys.None)
                {
                    FEZ_Shields.KeypadLCD.ShutBacklightOff();
                }
                else
                {
                    FEZ_Shields.KeypadLCD.TurnBacklightOn();
                }
            }
            while (true);
        }

        static void UpdateMonitorScreen(object sender)
        {
            // Debug.Print("UpdateMonitorScreen: " + m_schedulingResources.Now.ToString(m_config.DATETIME_FORMAT));

            LightTimes lt = s_controller.GetLightTimes();

            switch (s_monitorScreenIndex)
            {
                default:
                case StatusScreens.CurrentStatus:
                    byte row = 0;
                    byte col = 0;
                    FEZ_Shields.KeypadLCD.Clear();
                    FEZ_Shields.KeypadLCD.SetCursor(row++, col);
                    FEZ_Shields.KeypadLCD.Print(s_schedulingResources.Now.ToString(s_config.DATETIME_FORMAT));
                    FEZ_Shields.KeypadLCD.SetCursor(row, col);
                    string sun = s_schedulingResources.Now >= lt.Sunrise && s_schedulingResources.Now <= lt.Sunset ? "Up" : "Dn";
                    string light = s_controller.IsLightOn() ? "On" : "Off";
                    FEZ_Shields.KeypadLCD.Print(StringUtility.Format("Sun: {0}  Lt: {1}", sun, light));
                    break;

                case StatusScreens.Sunrise:
                    PrintLightEvent("Sunrise: ", lt.Sunrise, lt.LightsOnAM, lt.LightsOffAM);
                    break;

                case StatusScreens.Sunset:
                    PrintLightEvent("Sunset: ", lt.Sunset, lt.LightsOnPM, lt.LightsOffPM);
                    break;
            };

            s_monitorScreenIndex += 1;
            if (s_monitorScreenIndex == StatusScreens.TotalScreens)
                s_monitorScreenIndex = StatusScreens.CurrentStatus;
        }

        static private void PrintLightEvent(string name, DateTime eventTime, DateTime onTime, DateTime offTime)
        {
            byte row = 0;
            byte col = 0;
            FEZ_Shields.KeypadLCD.Clear();
            FEZ_Shields.KeypadLCD.SetCursor(row++, col);
            FEZ_Shields.KeypadLCD.Print(name);
            FEZ_Shields.KeypadLCD.Print(eventTime.ToString(s_config.TIME_FORMAT));
            FEZ_Shields.KeypadLCD.SetCursor(row, col);
            FEZ_Shields.KeypadLCD.Print("Lt: ");
            if (onTime < offTime || onTime - offTime > new TimeSpan(12, 0, 0))
            {
                FEZ_Shields.KeypadLCD.Print(onTime.ToString(s_config.TIME_FORMAT));
                FEZ_Shields.KeypadLCD.Print("-");
                FEZ_Shields.KeypadLCD.Print(offTime.ToString(s_config.TIME_FORMAT));
            }
            else
            {
                FEZ_Shields.KeypadLCD.Print("Disabled");
            }
        }

        static Controller s_controller;
        static Config s_config;
        static ISystemServices s_schedulingResources;

        static Thread s_monitorThread;
        static Timer s_screenUpdate;

        static StatusScreens s_monitorScreenIndex = StatusScreens.CurrentStatus;
    }
}
