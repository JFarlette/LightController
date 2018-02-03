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
                                            2 * 1000,  // Wait in ms to let the controller start
                                            7 * 1000); // Update frequency in ms

            MonitorKeyPad();

            s_screenUpdate.Dispose();
            s_screenUpdate = null;
        }

        /* In automatic mode: Implements three monitor screens for viewing the status of the controller:
         
            ----------------|
            2013-01-23 13:44|
            Sun: Up  Lt: Off|
            ----------------|
            Sunrise: 05:45  |
            Lt: 05:00-06:15 |
            ----------------|
            Sunset: 16:45   |
            Lt: 17:05-01:00 |
            ----------------|
            Press Up twice  |
            to reprogram    |
            ----------------|
          
          In manual mode: Implements one monitor screen for viewing the status of the controller:
         
            ----------------|
            2013-01-23 13:44|
            Lights: On      |
            ----------------|
            Press Up twice  |
            to reprogram    |
            ----------------|
         
        */
        enum AutoStatusScreens
        {
            CurrentStatus,
            Sunrise,
            Sunset,
            Reprogram,
            TotalScreens
        }

        enum ManualStatusScreens
        {
            CurrentStatus,
            Reprogram,
            TotalScreens
        }

        static private void MonitorKeyPad()
        {
            // Monitor and dim the LCD when idle
            // Enable the LCD when it is dim and a key is pressed
            // Break out of the loop if the Right key is hit twice in 2 seconds
            Debug.Print("Starting keypad monitor loop");
            do
            {
                FEZ_Shields.KeypadLCD.Keys k = FEZ_Shields.KeypadLCD.PauseForAnyKey(2 * 60 * 1000);
                if (k == FEZ_Shields.KeypadLCD.Keys.None)
                {
                    if (FEZ_Shields.KeypadLCD.IsBacklightOn())
                    {
                        Debug.Print("Turning Backlight Off");
                        FEZ_Shields.KeypadLCD.ShutBacklightOff();
                    }
                }
                else
                {
                    if (!(FEZ_Shields.KeypadLCD.IsBacklightOn())) // Need extra brackets here - compiler bug?
                    {
                        Debug.Print("Turning Backlight On");
                        FEZ_Shields.KeypadLCD.TurnBacklightOn();
                    }
                    if (k == FEZ_Shields.KeypadLCD.Keys.Up)
                    {
                        k = FEZ_Shields.KeypadLCD.PauseForAnyKey(2 * 1000);
                        if (k == FEZ_Shields.KeypadLCD.Keys.Up)
                            Debug.Print("Breaking keypad monitor loop");
                            break;
                    }
                }
            }
            while (true);
        }

        static void UpdateMonitorScreen(object sender)
        {
            if (!FEZ_Shields.KeypadLCD.IsBacklightOn())
                return;

            
            
            if (s_config.IsModeManual)
            {
                switch (s_manualStcreenIndex)
                {
                    default:
                    case ManualStatusScreens.CurrentStatus:
                        FEZ_Shields.KeypadLCD.Clear();
                        FEZ_Shields.KeypadLCD.SetCursor(0, 0);
                        FEZ_Shields.KeypadLCD.Print(s_schedulingResources.Now.ToString(Config.DATETIME_FORMAT));
                        FEZ_Shields.KeypadLCD.SetCursor(1, 0);
                        string light = s_controller.IsLightOn() ? "On" : "Off";
                        FEZ_Shields.KeypadLCD.Print(StringUtility.Format("Lights: {0}", light));
                        break;

                    case ManualStatusScreens.Reprogram:
                        FEZ_Shields.KeypadLCD.Clear();
                        FEZ_Shields.KeypadLCD.SetCursor(0, 0);
                        FEZ_Shields.KeypadLCD.Print(reprogramMsgLine1);
                        FEZ_Shields.KeypadLCD.SetCursor(1, 0);
                        FEZ_Shields.KeypadLCD.Print(reprogramMsgLine2);
                        break;
                };

                s_manualStcreenIndex += 1;
                if (s_manualStcreenIndex == ManualStatusScreens.TotalScreens)
                    s_manualStcreenIndex = ManualStatusScreens.CurrentStatus;
            }
            else
            {
                LightTimes lt = s_controller.GetLightTimes();

                Debug.Print(String.Concat("UpdateMonitorScreen: ", s_autoScreenIndex.ToString()));

                switch (s_autoScreenIndex)
                {
                    default:
                    case AutoStatusScreens.CurrentStatus:
                        byte row = 0;
                        byte col = 0;
                        FEZ_Shields.KeypadLCD.Clear();
                        FEZ_Shields.KeypadLCD.SetCursor(row++, col);
                        FEZ_Shields.KeypadLCD.Print(s_schedulingResources.Now.ToString(Config.DATETIME_FORMAT));
                        FEZ_Shields.KeypadLCD.SetCursor(row, col);
                        string sun = s_schedulingResources.Now >= lt.Sunrise && s_schedulingResources.Now <= lt.Sunset ? "Up" : "Dn";
                        string light = s_controller.IsLightOn() ? "On" : "Off";
                        FEZ_Shields.KeypadLCD.Print(StringUtility.Format("Sun: {0}  Lt: {1}", sun, light));
                        break;

                    case AutoStatusScreens.Sunrise:
                        PrintLightEvent("Sunrise: ", lt.Sunrise, lt.LightsOnAM, lt.LightsOffAM);
                        break;

                    case AutoStatusScreens.Sunset:
                        PrintLightEvent("Sunset: ", lt.Sunset, lt.LightsOnPM, lt.LightsOffPM);
                        break;

                    case AutoStatusScreens.Reprogram:
                        FEZ_Shields.KeypadLCD.Clear();
                        FEZ_Shields.KeypadLCD.SetCursor(0, 0);
                        FEZ_Shields.KeypadLCD.Print(reprogramMsgLine1);
                        FEZ_Shields.KeypadLCD.SetCursor(1, 0);
                        FEZ_Shields.KeypadLCD.Print(reprogramMsgLine2);
                        break;
                };

                s_autoScreenIndex += 1;
                if (s_autoScreenIndex == AutoStatusScreens.TotalScreens)
                    s_autoScreenIndex = AutoStatusScreens.CurrentStatus;
            }
        }

        static private void PrintLightEvent(string name, DateTime eventTime, DateTime onTime, DateTime offTime)
        {
            byte row = 0;
            byte col = 0;
            FEZ_Shields.KeypadLCD.Clear();
            FEZ_Shields.KeypadLCD.SetCursor(row++, col);
            FEZ_Shields.KeypadLCD.Print(name);
            FEZ_Shields.KeypadLCD.Print(eventTime.ToString(Config.TIME_FORMAT));
            FEZ_Shields.KeypadLCD.SetCursor(row, col);
            FEZ_Shields.KeypadLCD.Print("Lt: ");
            if (onTime < offTime || onTime - offTime > new TimeSpan(12, 0, 0))
            {
                FEZ_Shields.KeypadLCD.Print(onTime.ToString(Config.TIME_FORMAT));
                FEZ_Shields.KeypadLCD.Print("-");
                FEZ_Shields.KeypadLCD.Print(offTime.ToString(Config.TIME_FORMAT));
            }
            else
            {
                FEZ_Shields.KeypadLCD.Print("Disabled");
            }
        }

        const string reprogramMsgLine1 = "Press Up twice";
        const string reprogramMsgLine2 = "to reprogram";

        static Controller s_controller;
        static Config s_config;
        static ISystemServices s_schedulingResources;

        static Timer s_screenUpdate;

        static AutoStatusScreens s_autoScreenIndex = AutoStatusScreens.CurrentStatus;
        static ManualStatusScreens s_manualStcreenIndex = ManualStatusScreens.CurrentStatus;
    }
}
