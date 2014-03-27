using System;

namespace JFarlette.LightController
{
    interface ISystemServices
    {
        void SetLocalTime(DateTime dt);
        DateTime Now { get; }
        void Sleep(int ms);

    }
}
