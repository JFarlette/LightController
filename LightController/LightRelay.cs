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
            m_rp = new OutputPort(pin, false);
        }

        public void TurnOn()
        {
            m_rp.Write(true);
        }

        public void TurnOff()
        {
            m_rp.Write(false);
        }

        public bool IsTurnedOn()
        {
            return m_rp.Read();
        }

        OutputPort m_rp;
    }
}
