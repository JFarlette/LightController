using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;

namespace JFarlette.LightController
{
    class LightRelay : IRelay
    {
        public LightRelay(Cpu.Pin pin)
        {
            // led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di4, state == LightState.On);
        }

        public void TurnOn()
        {
            m_state = LightState.On;
        }

        public void TurnOff()
        {
            m_state = LightState.Off;
        }

        public bool IsTurnedOn()
        {
            return m_state == LightState.On;
        }

        enum LightState
        {
            On, Off
        }

        LightState m_state = LightState.Off;

    }
}
