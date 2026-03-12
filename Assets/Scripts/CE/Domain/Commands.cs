using System;

namespace CE.Domain
{
    [Serializable]
    public class PumpCommand
    {
        public string command;
        public bool state;
    }

    [Serializable]
    public class StatusCommand
    {
        public string command;
    }
}