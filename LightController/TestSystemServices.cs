using System;
using Microsoft.SPOT;

namespace JFarlette.LightController
{
    class TestSystemServices : ISystemServices
    {
        public void SetLocalTime(DateTime dt)
        {
            m_now = dt;
        }

        DateTime ISystemServices.Now
        {
            get { return m_now; }
        }

        void ISystemServices.Sleep(int ms)
        {
            m_now = m_now.AddMilliseconds(ms);
        }

        private DateTime m_now;
    }
}
