using System;
using Microsoft.SPOT;
using System.Collections;

namespace JFarlette.LightController
{
    class Controller
    {
        public Controller(Config config, ISystemServices ss, IRelay relay)
        {
            m_config = config;
            m_services = ss;
            m_relay = relay;
            m_isDst = config.IsDST;

            ScheduleLights();
        }

        public void Control()
        {
            Debug.Print("Control starting...");
            do
            {
                LightEvent le = (LightEvent)m_events.Dequeue();
                TimeSpan delay = le.DT - m_services.Now;
                Debug.Print("Sleeping until: " + le.DT.ToString(m_config.DATETIME_FORMAT));
                m_services.Sleep(TimeSpanUtility.TimeSpanTotalMilliseconds(delay));
                switch (le.Type)
                {
                    case LightEventType.Scheduled_On:
                    case LightEventType.Sunset_On:
                        Debug.Print("Turning light ON: " + m_services.Now.ToString(m_config.DATETIME_FORMAT));
                        m_relay.TurnOn();
                        break;
                    case LightEventType.Sunrise_Off: 
                    case LightEventType.Scheduled_Off:
                        Debug.Print("Turning light OFF: " + m_services.Now.ToString(m_config.DATETIME_FORMAT));
                        m_relay.TurnOff();
                        break;
                    case LightEventType.Tomorrow:
                        ScheduleLights();
                        break;
                }
            }
            while (true);
        }

        public LightTimes GetLightTimes()
        {
            lock (m_lightTimesLock)
            {
                return m_lightTimes;
            }
        }
        

        public bool IsLightOn()
        {
            return m_relay.IsTurnedOn();
        }

        enum LightEventType
        {
            Scheduled_On,
            Sunrise_Off,
            Sunset_On,
            Scheduled_Off,
            Tomorrow
        }

        class LightEvent
        {
            public LightEvent(DateTime dt, LightEventType type)
            {
                DT = dt;
                Type = type;
            }

            public string ToString(string format)
            {
                return DT.ToString(format) + " - " + Type.ToString();
            }

            public DateTime DT;
            public LightEventType Type;
        }

        /*
         * [ ] is period where the light is on
         * 
         * |012345678901234567890123|012345678901234567890123|012345678901234567890123|
         * |------------------------|------------------------|------------------------|
         * | ]    [ ]         [     | ]    [ ]          [    | ]    [ ]          [    |
         */

        /* Example of shifting sunrise and sunset in polar regions
           
            |123456789012123456789012|
            |------------------------|
            |012345678901234567890123|
            | ]   [                  |
            | ]   [        ][        |
            | ]   [       ]  [       |
            | ]   [      ]    [      |
            | ]   [     ]      [     |
            | ]   [    ]        [    |
            | ]   [   ]          [   |
            | ]   [  ]            [  |
            | ]   [ ]              [ |
            | ]   []                [|
            |[]                      |
            |                        |
            |                        |
            |                        |
            |[]                      |
            | ]   []                [|
            | ]   [ ]              [ |
            | ]   [  ]            [  |
            | ]   [   ]          [   |
            | ]   [    ]        [    |
            | ]   [     ]      [     |
            | ]   [      ]    [      |
            | ]   [       ]  [       |
            | ]   [        ][        |
            | ]   [                  |
         */

        

        // Return an order list of light events for the specified day.  
        private ArrayList LightEventsForDate(LightTimes lt)
        {
            ArrayList les = new ArrayList();

            if (lt.LightsOffPM > lt.LightsOnAM)  
            {
                // Scheduled lights off late night
                if (lt.LightsOnAM < lt.LightsOffAM)
                {
                    les.Add(new LightEvent(lt.LightsOnAM, LightEventType.Scheduled_On));
                }

                if (lt.Sunrise < lt.Sunset)
                {
                    les.Add(new LightEvent(lt.LightsOffAM, LightEventType.Sunrise_Off));
                    les.Add(new LightEvent(lt.LightsOnPM, LightEventType.Sunset_On));
                }

                if (lt.LightsOffPM > lt.LightsOnPM)
                {
                    les.Add(new LightEvent(lt.LightsOffPM, LightEventType.Scheduled_Off));
                }
            }
            else
            {
                // Scheduled lights off early morning
                if (lt.LightsOffPM < lt.LightsOffAM)
                {
                    les.Add(new LightEvent(lt.LightsOffPM, LightEventType.Scheduled_Off));
                }

                if (lt.LightsOnAM < lt.LightsOffAM)
                {
                    les.Add(new LightEvent(lt.LightsOnAM, LightEventType.Scheduled_On));
                }

                if (lt.Sunrise < lt.Sunset)
                {
                    les.Add(new LightEvent(lt.LightsOffAM, LightEventType.Sunrise_Off));
                    les.Add(new LightEvent(lt.LightsOnPM, LightEventType.Sunset_On));
                }
            }
            return les;
        }

        private void ScheduleLights()
        {
            Debug.Print("Scheduling lights: " + m_services.Now.ToString(m_config.DATETIME_FORMAT));

            bool isDstToday = m_config.IsDstInEffect(m_services.Now);
            if (isDstToday && !m_isDst)
            {
                // Spring a head
                m_services.SetLocalTime(m_services.Now + new TimeSpan(1, 0, 0));
                Debug.Print("DST starting.  Time now: " + m_services.Now.ToString(m_config.DATETIME_FORMAT));
                m_isDst = isDstToday;
            }
            else if (m_isDst && !isDstToday)
            {
                // Fall back
                m_services.SetLocalTime(m_services.Now - new TimeSpan(1, 0, 0));
                Debug.Print("DST over.  Time now: " + m_services.Now.ToString(m_config.DATETIME_FORMAT));
                m_isDst = isDstToday;
            }

            lock (m_lightTimesLock)
            {
                m_lightTimes = new LightTimes(m_services.Now, m_config);
            }

            // Get an order list of events for the day
            ArrayList les = LightEventsForDate(m_lightTimes);

            // Iterate over the list looking for previous event and next event based on current time
            int prevIndex = -1;
            for (int i = 0; i < les.Count; i++)
            {
                LightEvent le = (LightEvent)les[i];
                if (le.DT < m_services.Now)
                    prevIndex = i;
                else
                    break;
            }
            // Turn the light on if the previous event was an on event
            if (prevIndex > -1)
            {
                LightEvent prev = (LightEvent)les[prevIndex];
                if (prev.Type == LightEventType.Scheduled_On || prev.Type == LightEventType.Sunset_On)
                {
                    Debug.Print("Turning light ON due to missed event");
                    m_relay.TurnOn();
                }
            }
            // Iterate over the remaining events adding each to the event queue
            if (prevIndex + 1 < les.Count)
            {
                for (int i = prevIndex + 1; i < les.Count; i++)
                {
                    LightEvent le = (LightEvent)les[i];
                    Debug.Print("Enqueing LE: " + le.ToString(m_config.DATETIME_FORMAT));
                    m_events.Enqueue(le);
                }
            }

            // Schedule the event to schedule tomorrows events
            LightEvent scheduleEvent = new LightEvent(m_services.Now.Date + new TimeSpan(1, 0, 0, 0), LightEventType.Tomorrow);
            Debug.Print("Enqueing LE: " + scheduleEvent.ToString(m_config.DATETIME_FORMAT));
            m_events.Enqueue(scheduleEvent);
        }

        IRelay m_relay;

        ISystemServices m_services;

        Config m_config;

        LightTimes m_lightTimes;

        object m_lightTimesLock = new object();

        Queue m_events = new Queue();

        bool m_isDst;
    }
}
