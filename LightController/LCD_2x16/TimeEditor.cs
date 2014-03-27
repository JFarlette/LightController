using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT;
using System;

namespace JFarlette.LightController.LCD_2x16
{
    static class TimeEditor
    {
        class TimeField
        {
            public TimeField(string name, 
                FieldValueDelegate fieldValueDel, SetFieldValueDelegate setFieldValueDel, 
                MinValueDelegate minValueDel, MaxValueDelegate maxValueDel)
            {
                m_name = name;
                m_fieldValueDelegate = fieldValueDel;
                m_setFieldValueDelegate = setFieldValueDel;
                m_minValueDelegate = minValueDel;
                m_maxValueDelegate = maxValueDel;
            }

            private string m_name;

            public string Name() { return m_name; }

            public delegate int FieldValueDelegate(DateTime dt);
            FieldValueDelegate m_fieldValueDelegate;
            public int FieldValue(DateTime dt)
            {
                return m_fieldValueDelegate(dt);
            }

            public delegate void SetFieldValueDelegate(ref DateTime dt, int value);
            SetFieldValueDelegate m_setFieldValueDelegate;
            public void SetFieldValue(ref DateTime dt, int value)
            {
                m_setFieldValueDelegate(ref dt, value);
            }

            public delegate int MinValueDelegate();
            MinValueDelegate m_minValueDelegate;
            public int MinValue()
            {
                return m_minValueDelegate();
            }

            public delegate int MaxValueDelegate();
            MaxValueDelegate m_maxValueDelegate;
            public int MaxValue()
            {
                return m_maxValueDelegate();
            }
        };

        static TimeField[] TimeFields = new TimeField[2] {
            new TimeField("Hr  ", 
                delegate(DateTime dt) { return dt.Hour; }, 
                delegate(ref DateTime dt, int hour) 
                {
                    dt = new DateTime(dt.Year, dt.Month, dt.Day, hour, 
                                        dt.Minute, dt.Second); 
                }, 
                delegate() { return 0; },
                delegate() { return 23; }),

            new TimeField("Min", 
                delegate(DateTime dt) { return dt.Minute; }, 
                delegate(ref DateTime dt, int minute) 
                {
                    dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 
                                        minute, dt.Second); 
                }, 
                delegate() { return 0; },
                delegate() { return 59; }),

        };

        public static DateTime Edit(DateTime dt, string name, string timeFormat)
        {
            int fieldIndex = 0;
            int fieldValueOriginal = TimeFields[fieldIndex].FieldValue(dt);
            int fieldValueNew = fieldValueOriginal;
            int minValue = TimeFields[fieldIndex].MinValue();
            int maxValue = TimeFields[fieldIndex].MaxValue();

            FEZ_Shields.KeypadLCD.Clear();
            PrintField(name, fieldIndex);
            PrintValue(dt, timeFormat);
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
                    fieldValueNew = fieldValueOriginal = df.FieldValue(dt);
                    minValue = df.MinValue();
                    maxValue = df.MaxValue();
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
                    TimeFields[fieldIndex].SetFieldValue(ref dt, fieldValueNew);
                    PrintValue(dt, timeFormat);
                    fieldValueOriginal = fieldValueNew;
                }
            }
            Debug.Print("Final time string: " + dt.ToString(timeFormat));
            return dt;
        }

        private static void PrintField(string name, int fieldIndex)
        {
            FEZ_Shields.KeypadLCD.SetCursor(0, 0);
            FEZ_Shields.KeypadLCD.Print(name + ": " + TimeFields[fieldIndex].Name());
        }

        private static void PrintValue(DateTime dt, string timeFormat)
        {
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            FEZ_Shields.KeypadLCD.Print(dt.ToString(timeFormat));
        }
    }
}
