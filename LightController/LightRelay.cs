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
            //Debug.Print("LightRelay::LightRelay()");
            m_rp = new OutputPort(pin, false);
        }

        public void TurnOn()
        {
            //Debug.Print("LightRelay.TurnOn");
            m_rp.Write(true);
        }

        public void TurnOff()
        {
            //Debug.Print("LightRelay.TurnOff");
            m_rp.Write(false);
        }

        public bool IsTurnedOn()
        {
            bool result = m_rp.Read();
            // Debug.Print("LightRelay.IsTurnedOn = " + (result ? "Yes" : "No"));
            return result;
        }

        OutputPort m_rp;
    }
}
