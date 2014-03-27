using GHIElectronics.NETMF.FEZ;

namespace JFarlette.LightController.LCD_2x16
{
    static class IntEditor
    {
        static public int Edit(int n, string name)
        {
            return Edit(n, name, int.MinValue, int.MaxValue);
        }

        static public int Edit(int n, string name, int min, int max)
        {
            FEZ_Shields.KeypadLCD.Clear();
            PrintName(name);
            PrintValue(n);
            FEZ_Shields.KeypadLCD.Keys key;
            while ((key = FEZ_Shields.KeypadLCD.WaitKeyPress()) != FEZ_Shields.KeypadLCD.Keys.Select)
            {
                if (key == FEZ_Shields.KeypadLCD.Keys.Up || key == FEZ_Shields.KeypadLCD.Keys.Right)
                {
                    n = n == max ? min : n + 1;
                }
                else
                {
                    n = n == min ? max : n - 1;
                }
                PrintValue(n);
            }

            return n;
        }

        private static void PrintName(string name)
        {
            FEZ_Shields.KeypadLCD.SetCursor(0, 0);
            FEZ_Shields.KeypadLCD.Print(name);
        }

        private static void PrintValue(int n)
        {
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            string s = n.ToString();
            string x = new string(' ', 16 - s.Length);
            FEZ_Shields.KeypadLCD.Print(s);
            FEZ_Shields.KeypadLCD.Print(x);
        }
    }
}
