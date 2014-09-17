using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;

namespace JFarlette.LightController
{
    class LightRelay : IRelay
    {
        // Is m_state needed?  Can m_rp.Read() be used for for IsTurnedOn?
        public LightRelay(Cpu.Pin pin)
        {
            m_rp = new OutputPort(pin, false/*m_state == LightState.On*/);
        }

        public void TurnOn()
        {
            //m_state = LightState.On;
            m_rp.Write(true);
        }

        public void TurnOff()
        {
            //m_state = LightState.Off;
            m_rp.Write(false);
        }

        public bool IsTurnedOn()
        {
            //return m_state == LightState.On;
            return m_rp.Read();
        }

        //enum LightState
        //{
        //    On, Off
        //}

        //LightState m_state = LightState.Off;
        OutputPort m_rp;
    }
}
