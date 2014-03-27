using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT;
using System;

namespace JFarlette.LightController.LCD_2x16
{
    static public class TimespanEditor
    {

        class TimeField
        {
            public TimeField(string name, FieldValueDelegate fieldValueDel, SetFieldValueDelegate setFieldValueDel, MinValueDelegate minValueDel, MaxValueDelegate maxValueDel)
            {
                m_name = name;
                m_fieldValueDelegate = fieldValueDel;
                m_setFieldValueDelegate = setFieldValueDel;
                m_minValueDelegate = minValueDel;
                m_maxValueDelegate = maxValueDel;
            }

            private string m_name;

            public string Name() { return m_name; }

            public delegate int FieldValueDelegate(TimeSpan ts);
            FieldValueDelegate m_fieldValueDelegate;
            public int FieldValue(TimeSpan ts)
            {
                return m_fieldValueDelegate(ts);
            }

            public delegate void SetFieldValueDelegate(ref TimeSpan ts, int value);
            SetFieldValueDelegate m_setFieldValueDelegate;
            public void SetFieldValue(ref TimeSpan ts, int value)
            {
                m_setFieldValueDelegate(ref ts, value);
            }

            public delegate int MinValueDelegate(bool allowNegative);
            MinValueDelegate m_minValueDelegate;
            public int MinValue(bool allowNegative)
            {
                return m_minValueDelegate(allowNegative);
            }

            public delegate int MaxValueDelegate(bool allowNegative);
            MaxValueDelegate m_maxValueDelegate;
            public int MaxValue(bool allowNegative)
            {
                return m_maxValueDelegate(allowNegative);
            }
        };
    
        static TimeField[] TimeFields = new TimeField[2] {
            new TimeField("Hr  ", 
                delegate(TimeSpan ts) { return ts.Hours; }, 
                delegate(ref TimeSpan ts, int hours) 
                {
                    ts = new TimeSpan(hours, ts.Minutes, ts.Seconds);
                }, 
                // Min value
                delegate(bool allowNegative) 
                { 
                    if (allowNegative)
                    {
                        return -24;
                    }
                    else // Postive only
                    {
                        return 0;
                    }
                },
                // Max value
                delegate(bool allowNegative) 
                { 
                    return 24;
                }),

            new TimeField("Min", 
                delegate(TimeSpan ts) { return ts.Minutes; }, 
                delegate(ref TimeSpan ts, int minutes) 
                {
                    ts = new TimeSpan(ts.Hours, minutes, ts.Seconds);
                }, 
                // Min value
                delegate(bool allowNegative) 
                { 
                    if (allowNegative)
                    {
                        return -59;
                    }
                    else // Postive only
                    {
                        return 0;
                    }
                },
                // Max value
                delegate(bool allowNegative) 
                {
                    return 59;
                })

        };

        public static TimeSpan Edit(TimeSpan ts, string name, bool allowNegative)
        {
            int fieldIndex = 0;
            int fieldValueOriginal = TimeFields[fieldIndex].FieldValue(ts);
            int fieldValueNew = fieldValueOriginal;
            int minValue = TimeFields[fieldIndex].MinValue(allowNegative);
            int maxValue = TimeFields[fieldIndex].MaxValue(allowNegative);

            FEZ_Shields.KeypadLCD.Clear();
            PrintField(name, fieldIndex);
            PrintValue(ts);
            FEZ_Shields.KeypadLCD.Keys key;
            while ((key = FEZ_Shields.KeypadLCD.WaitKeyPress()) != FEZ_Shields.KeypadLCD.Keys.Select)
            {
                if (key == FEZ_Shields.KeypadLCD.Keys.Left || key == FEZ_Shields.KeypadLCD.Keys.Right)
                {
                    if (key == FEZ_Shields.KeypadLCD.Keys.Left)
                    {
                        fieldIndex -= 1;
                        if (fieldIndex < 0) fieldIndex = TimeFields.Length - 1;
                    }
                    else // right
                    {
                        fieldIndex += 1;
                        if (fieldIndex == TimeFields.Length) fieldIndex = 0;
                    }
                    // Show the selected field in the UI
                    PrintField(name, fieldIndex);

                    // extract the field value
                    TimeField df = TimeFields[fieldIndex];
                    fieldValueNew = fieldValueOriginal = df.FieldValue(ts);
                    minValue = df.MinValue(allowNegative);
                    maxValue = df.MaxValue(allowNegative);
                }
                else // Up or Down
                {
                    if (key == FEZ_Shields.KeypadLCD.Keys.Up) // Increment
                    {
                        fieldValueNew += 1;
                        if (fieldValueNew > maxValue) fieldValueNew = minValue;
                    }
                    else // Down -> Decrement
                    {
                        fieldValueNew -= 1;
                        if (fieldValueNew < minValue) fieldValueNew = maxValue;
                    }

                    TimeFields[fieldIndex].SetFieldValue(ref ts, fieldValueNew);
                    PrintValue(ts);
                    fieldValueOriginal = fieldValueNew;
                }
            }
            Debug.Print("Final timespan string: " + ts.ToString());
            return ts;
        }

        private static void PrintField(string name, int fieldIndex)
        {
            FEZ_Shields.KeypadLCD.SetCursor(0, 0);
            FEZ_Shields.KeypadLCD.Print(name + ": " + TimeFields[fieldIndex].Name());
        }

        private static void PrintValue(TimeSpan ts)
        {
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            string s = ts.ToString();
            string x = new string(' ', 16 - s.Length);
            FEZ_Shields.KeypadLCD.Print(s);
            FEZ_Shields.KeypadLCD.Print(x);
        }
    }
}
