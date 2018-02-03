using GHIElectronics.NETMF.FEZ;

namespace JFarlette.LightController.LCD_2x16
{
    static class BooleanEditor
    {
        public static bool Edit(bool b, string name)
        {
            return Edit(b, name, "Yes", "No ");
        }

        public static bool Edit(bool b, string name, string trueName, string falseName)
        {
            FEZ_Shields.KeypadLCD.Clear();
            PrintName(name);
            PrintValue(b, trueName, falseName);
            FEZ_Shields.KeypadLCD.Keys key;
            while ((key = FEZ_Shields.KeypadLCD.WaitKeyPress()) != FEZ_Shields.KeypadLCD.Keys.Select)
            {
                b = !b;
                PrintValue(b, trueName, falseName);
            }

            return b;
        }

        private static void PrintName(string name)
        {
            FEZ_Shields.KeypadLCD.SetCursor(0, 0);
            FEZ_Shields.KeypadLCD.Print(name);
        }

        private static void PrintValue(bool b, string trueName, string falseName)
        {
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            FEZ_Shields.KeypadLCD.Print("                ");
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            FEZ_Shields.KeypadLCD.Print(b ? trueName : falseName);
        }
    }
}
