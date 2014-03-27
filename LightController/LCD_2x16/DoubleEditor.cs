using GHIElectronics.NETMF.FEZ;
using System;
using NetMf.CommonExtensions;

namespace JFarlette.LightController.LCD_2x16
{
    static public class DoubleEditor
    {
        static char[] DecideChars(char[] buffer, int index, int end)
        {
            char[] chars;
            if (index == 0)
                chars = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '-' };
            else if (index < (end - 1))
            {
                bool foundDec = CheckBufferForDecimal(buffer, index, end);
                if (foundDec)
                    chars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                else
                    chars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' };
            }
            else
            {
                bool foundDec = CheckBufferForDecimal(buffer, index, end);
                if (foundDec)
                    chars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ' };
                else
                    chars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ' ' };
            }

            return chars;
        }

        private static bool CheckBufferForDecimal(char[] buffer, int index, int end)
        {
            bool foundDec = false;
            for (int i = 0; i < end; i++)
            {
                if (i != index && buffer[i] == '.')
                {
                    foundDec = true;
                    break;
                }
            }
            return foundDec;
        }

        static int IndexInChars(char c, char[] chars)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                if (c == chars[i])
                {
                    return i;
                }
            }
            return 0;  // ????
        }

        public static double Edit(double d, string name)
        {
            char[] buffer = new char[16];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = ' ';
            }
            char[] x = d.ToString().ToCharArray();
            int end = Math.Min(x.Length, buffer.Length);
            Array.Copy(x, buffer, end);

            int index = 0;
            
            FEZ_Shields.KeypadLCD.Clear();
            PrintName(name, buffer, index);
            PrintValue(buffer);
            
            FEZ_Shields.KeypadLCD.Keys key;
            while ((key = FEZ_Shields.KeypadLCD.WaitKeyPress()) != FEZ_Shields.KeypadLCD.Keys.Select)
            {
                if (key == FEZ_Shields.KeypadLCD.Keys.Up || key == FEZ_Shields.KeypadLCD.Keys.Down)
                {
                    char[] chars = DecideChars(buffer, index, end);
                    char c = buffer[index];
                    int i = IndexInChars(c, chars); 
                    if (key == FEZ_Shields.KeypadLCD.Keys.Up)
                    {
                        i += 1;
                        if (i == chars.Length)
                        {
                            i = 0;
                        }
                    }
                    else // Down
                    {
                        if (i == 0)
                            i = chars.Length - 1;
                        else
                            i -= 1;
                    }
                    c = chars[i];
                    buffer[index] = c;
                    
                    if (index == end) end = Math.Min(end + 1, buffer.Length);
                    else if (index == end - 1 && c == ' ') end -= 1;

                    PrintName(name, buffer, index);
                    PrintValue(buffer);
                }
                else // Left or right
                {
                    if (key == FEZ_Shields.KeypadLCD.Keys.Left)
                    {
                        index -= 1;
                        if (index < 0) index = Math.Min(end, buffer.Length-1);
                    }
                    else // Right
                    {
                        index += 1;
                        if (index > end || index >= buffer.Length) index = 0;
                    }
                    PrintName(name, buffer, index);
                }
            }
            return double.Parse(new string(buffer));
        }

        private static void PrintName(string name, char[] buffer, int index)
        {
            FEZ_Shields.KeypadLCD.SetCursor(0, 0);
            string s = StringUtility.Format("{0}[{1}]={2}", name, (index + 1), buffer[index]);
            FEZ_Shields.KeypadLCD.Print(s);
            FEZ_Shields.KeypadLCD.Print(new string(' ', 16 - s.Length));
        }

        private static void PrintValue(char[] d)
        {
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            FEZ_Shields.KeypadLCD.Print(new string(d));
        }
    }
}
