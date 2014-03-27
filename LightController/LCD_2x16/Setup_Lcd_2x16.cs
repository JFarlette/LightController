using System;
using GHIElectronics.NETMF.FEZ;

namespace JFarlette.LightController.LCD_2x16
{
    static class Setup_Lcd_2x16
    {
        static void DoIntroScreens()
        {
            FEZ_Shields.KeypadLCD.Clear();
            FEZ_Shields.KeypadLCD.SetCursor(0, 0);
            FEZ_Shields.KeypadLCD.Print("Setup: Press");
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            FEZ_Shields.KeypadLCD.Print("Select to accept");
            FEZ_Shields.KeypadLCD.PauseForAnyKey(5000);

            FEZ_Shields.KeypadLCD.Clear();
            FEZ_Shields.KeypadLCD.SetCursor(0, 0);
            FEZ_Shields.KeypadLCD.Print("Left/Right to");
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            FEZ_Shields.KeypadLCD.Print("select a field");
            FEZ_Shields.KeypadLCD.PauseForAnyKey(5000);

            FEZ_Shields.KeypadLCD.Clear();
            FEZ_Shields.KeypadLCD.SetCursor(0, 0);
            FEZ_Shields.KeypadLCD.Print("Up/Down to");
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            FEZ_Shields.KeypadLCD.Print("increment value");
            FEZ_Shields.KeypadLCD.PauseForAnyKey(5000);
        }

        static void DoFinishScreen()
        {
            FEZ_Shields.KeypadLCD.Clear();
            FEZ_Shields.KeypadLCD.SetCursor(0, 0);
            FEZ_Shields.KeypadLCD.Print("Setup complete");
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            FEZ_Shields.KeypadLCD.Print("Starting...");
            FEZ_Shields.KeypadLCD.PauseForAnyKey(5000);
        }

        static public void DoSetup(Config config)
        {
            DoIntroScreens();

            config.StartingDateTime = DateEditor.Edit(config.StartingDateTime, "Date", config.DATE_FORMAT);
            config.StartingDateTime = TimeEditor.Edit(config.StartingDateTime, "Time", config.TIME_FORMAT);

            config.UTCOffset = (sbyte)IntEditor.Edit(config.UTCOffset, "UTC Offset", -12, 13);
            config.IsDST = BooleanEditor.Edit(config.IsDST, "DST in effect?");

            config.SiteCoords.Latitude = DoubleEditor.Edit(config.SiteCoords.Latitude, "Latitude");
            config.SiteCoords.Longitude = DoubleEditor.Edit(config.SiteCoords.Longitude, "Longitude");

            DateTime today = config.StartingDateTime.Date;

            DateTime lightsOn = today + config.AMLightsOn;
            lightsOn = TimeEditor.Edit(lightsOn, "AM Light On", config.TIME_FORMAT);
            config.AMLightsOn = lightsOn.TimeOfDay;

            config.SunriseOffset = TimespanEditor.Edit(config.SunriseOffset, "Sunrise Adj", allowNegative: true);

            config.SunsetOffset = TimespanEditor.Edit(config.SunsetOffset, "Sunset Adj", allowNegative: true);

            DateTime lightsOff = today + config.PMLightsOff;
            lightsOff = TimeEditor.Edit(lightsOff, "PM Light Off", config.TIME_FORMAT);
            config.PMLightsOff = lightsOff.TimeOfDay;

            DoFinishScreen();
        }
    }
}
