//BLACK SHIELD DRIVER

/*
Copyright 2010 GHI Electronics LLC
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.Hardware;
using JFarlette.LightController;

namespace GHIElectronics.NETMF.FEZ
{
    public static partial class FEZ_Shields
    {
        static public class KeypadLCD
        {
            public enum Keys
            {
                Up,
                Down,
                Right,
                Left,
                Select,
                None,
            }

            static OutputPort LCD_RS;
            static OutputPort LCD_E;

            static OutputPort LCD_D4;
            static OutputPort LCD_D5;
            static OutputPort LCD_D6;
            static OutputPort LCD_D7;

            static AnalogIn AnKey;

            static OutputPort BackLight;

            const byte DISP_ON = 0xC;    //Turn visible LCD on
            const byte CLR_DISP = 1;      //Clear display
            const byte CUR_HOME = 2;      //Move cursor home and clear screen memory
            const byte SET_CURSOR = 0x80;   //SET_CURSOR + X : Sets cursor position to X

            public static void Initialize()
            {
                LCD_RS = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di8, false);
                LCD_E = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di9, false);

                LCD_D4 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di4, false);
                LCD_D5 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di5, false);
                LCD_D6 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di6, false);
                LCD_D7 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di7, false);

                AnKey = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An0);

                BackLight = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di10, true);

                LCD_RS.Write(false);

                // 4 bit data communication
                Thread.Sleep(50);

                LCD_D7.Write(false);
                LCD_D6.Write(false);
                LCD_D5.Write(true);
                LCD_D4.Write(true);

                LCD_E.Write(true);
                LCD_E.Write(false);

                Thread.Sleep(50);
                LCD_D7.Write(false);
                LCD_D6.Write(false);
                LCD_D5.Write(true);
                LCD_D4.Write(true);

                LCD_E.Write(true);
                LCD_E.Write(false);

                Thread.Sleep(50);
                LCD_D7.Write(false);
                LCD_D6.Write(false);
                LCD_D5.Write(true);
                LCD_D4.Write(true);

                LCD_E.Write(true);
                LCD_E.Write(false);

                Thread.Sleep(50);
                LCD_D7.Write(false);
                LCD_D6.Write(false);
                LCD_D5.Write(true);
                LCD_D4.Write(false);

                LCD_E.Write(true);
                LCD_E.Write(false);

                SendCmd(DISP_ON);
                SendCmd(CLR_DISP);
            }

            //Sends an ASCII character to the LCD
            static void Putc(byte c)
            {
                LCD_D7.Write((c & 0x80) != 0);
                LCD_D6.Write((c & 0x40) != 0);
                LCD_D5.Write((c & 0x20) != 0);
                LCD_D4.Write((c & 0x10) != 0);
                LCD_E.Write(true); LCD_E.Write(false); //Toggle the Enable Pin

                LCD_D7.Write((c & 0x08) != 0);
                LCD_D6.Write((c & 0x04) != 0);
                LCD_D5.Write((c & 0x02) != 0);
                LCD_D4.Write((c & 0x01) != 0);
                LCD_E.Write(true); LCD_E.Write(false); //Toggle the Enable Pin
                //Thread.Sleep(1);
            }

            //Sends an LCD command
            static void SendCmd(byte c)
            {
                LCD_RS.Write(false); //set LCD to data mode

                LCD_D7.Write((c & 0x80) != 0);
                LCD_D6.Write((c & 0x40) != 0);
                LCD_D5.Write((c & 0x20) != 0);
                LCD_D4.Write((c & 0x10) != 0);
                LCD_E.Write(true); LCD_E.Write(false); //Toggle the Enable Pin

                LCD_D7.Write((c & 0x08) != 0);
                LCD_D6.Write((c & 0x04) != 0);
                LCD_D5.Write((c & 0x02) != 0);
                LCD_D4.Write((c & 0x01) != 0);
                LCD_E.Write(true); LCD_E.Write(false); //Toggle the Enable Pin
                Thread.Sleep(1);
                LCD_RS.Write(true); //set LCD to data mode
            }

            public static void Print(string str)
            {
                for (int i = 0; i < str.Length; i++)
                    Putc((byte)str[i]);
            }

            public static void Clear()
            {
                SendCmd(CLR_DISP);
            }

            public static void CursorHome()
            {
                SendCmd(CUR_HOME);
            }

            public static void SetCursor(byte row, byte col)
            {
                SendCmd((byte)(SET_CURSOR | row << 6 | col));
            }

            public static Keys GetKey()
            {
                int i = AnKey.Read();
                // use this to read values to calibrate
                //while (true)
                //{
                //    i = AnKey.Read();
                //    if (i != 1023)
                //        Debug.Print("Read: " + i.ToString());
                //    Thread.Sleep(300);
                //}
                const int ERROR = 50;
                const int UP = 150;
                const int DOWN = 350;
                const int LEFT = 575;
                const int RIGHT = 0;
                const int SEL = 900;
                const int NONE = 1024;

                if (i > NONE - ERROR)
                    return Keys.None;

                if (i < RIGHT + ERROR)
                    return Keys.Right;

                if (i < UP + ERROR && i > UP - ERROR)
                    return Keys.Up;

                if (i < DOWN + ERROR && i > DOWN - ERROR)
                    return Keys.Down;

                if (i < LEFT + ERROR && i > LEFT - ERROR)
                    return Keys.Left;

                if (i < SEL + ERROR && i > SEL - ERROR)
                    return Keys.Select;

                return Keys.None;
            }

            public static Keys WaitKeyPress()
            {
                Keys k = Keys.None;
                do
                {
                    k = GetKey();
                }
                while (k == Keys.None);
                Thread.Sleep(200);
                return k;
            }

            public static Keys PauseForAnyKey(int timeoutInMs)
            {
                Keys k = Keys.None;
                DateTime start = DateTime.Now;
                TimeSpan delay;
                do
                {
                    k = GetKey();
                    delay = new TimeSpan(DateTime.Now.Ticks - start.Ticks);
                }
                while (k == Keys.None && TimeSpanUtility.TimeSpanTotalMilliseconds(delay) < timeoutInMs);
                if (k != Keys.None)
                    Thread.Sleep(200);
                return k;
            }

            public static void TurnBacklightOn()
            {
                BackLight.Write(true);
            }

            public static void ShutBacklightOff()
            {
                BackLight.Write(false);
            }
        }
    }
}
