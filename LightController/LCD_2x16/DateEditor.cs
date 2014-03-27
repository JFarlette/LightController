using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT;
using System;

namespace JFarlette.LightController.LCD_2x16
{
    static class DateEditor
    {
        class DateField
        {
            public DateField(string name, FieldValueDelegate fieldValueDel, SetFieldValueDelegate setFieldValueDel, MinValueDelegate minValueDel, MaxValueDelegate maxValueDel)
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

            public delegate int MaxValueDelegate(DateTime dt);
            MaxValueDelegate m_maxValueDelegate;
            public int MaxValue(DateTime dt)
            {
                return m_maxValueDelegate(dt);
            }
        };

        static DateField[] DateFields = new DateField[3] {
            new DateField("Year ", 
                delegate(DateTime dt) { return dt.Year; }, 
                delegate(ref DateTime dt, int value) 
                {
                    int day = System.Math.Min(dt.Day, DateTime.DaysInMonth(value, dt.Month));
                    dt = new DateTime(value, dt.Month, day, dt.Hour, dt.Minute, dt.Second); 
                }, 
                delegate() { return 2013; },
                delegate(DateTime dt) { return 2100; }),

            new DateField("Month", 
                delegate(DateTime dt) { return dt.Month; }, 
                delegate(ref DateTime dt, int value) 
                {
                    int day = System.Math.Min(dt.Day, DateTime.DaysInMonth(dt.Year, value));
                    dt = new DateTime(dt.Year, value, day, dt.Hour, dt.Minute, dt.Second); 
                }, 
                delegate() { return 1; },
                delegate(DateTime dt) { return 12; }),

            new DateField("Day  ", 
                delegate(DateTime dt) { return dt.Day; }, 
                delegate(ref DateTime dt, int value) { dt = new DateTime(dt.Year, dt.Month, value, dt.Hour, dt.Minute, dt.Second); }, 
                delegate() { return 1; },
                delegate(DateTime dt) { return DateTime.DaysInMonth(dt.Year, dt.Month); }),

        };

        public static DateTime Edit(DateTime dt, string name, string dateFormat)
        {
            int fieldIndex = 0;
            int fieldValueOriginal = DateFields[fieldIndex].FieldValue(dt);
            int fieldValueNew = fieldValueOriginal;
            int minValue = DateFields[fieldIndex].MinValue();
            int maxValue = DateFields[fieldIndex].MaxValue(dt);

            FEZ_Shields.KeypadLCD.Clear();
            PrintField(name, fieldIndex);
            PrintValue(dt, dateFormat);
            FEZ_Shields.KeypadLCD.Keys key;
            while ((key = FEZ_Shields.KeypadLCD.WaitKeyPress()) != FEZ_Shields.KeypadLCD.Keys.Select)
            {
                if (key == FEZ_Shields.KeypadLCD.Keys.Left || key == FEZ_Shields.KeypadLCD.Keys.Right)
                {
                    if (key == FEZ_Shields.KeypadLCD.Keys.Left)
                    {
                        fieldIndex -= 1;
                        if (fieldIndex < 0) fieldIndex = DateFields.Length - 1;
                    }
                    else // right
                    {
                        fieldIndex += 1;
                        if (fieldIndex == DateFields.Length) fieldIndex = 0;
                    }
                    // Show the selected field in the UI
                    PrintField(name, fieldIndex);

                    // extract the field value
                    DateField df = DateFields[fieldIndex];
                    fieldValueNew = fieldValueOriginal = df.FieldValue(dt);
                    minValue = df.MinValue();
                    maxValue = df.MaxValue(dt);
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
                    DateFields[fieldIndex].SetFieldValue(ref dt, fieldValueNew);
                    PrintValue(dt, dateFormat);
                    fieldValueOriginal = fieldValueNew;
                }
            }
            Debug.Print("Final date string: " + dt.ToString(dateFormat));
            return dt;
        }

        private static void PrintValue(DateTime dt, string dateFormat)
        {
            FEZ_Shields.KeypadLCD.SetCursor(1, 0);
            FEZ_Shields.KeypadLCD.Print(dt.ToString(dateFormat));
        }

        private static void PrintField(string name, int fieldIndex)
        {
            FEZ_Shields.KeypadLCD.SetCursor(0, 0);
            FEZ_Shields.KeypadLCD.Print(name + ": " + DateFields[fieldIndex].Name());
        }
    }
}
