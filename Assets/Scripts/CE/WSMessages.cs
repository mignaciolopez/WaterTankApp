using System;
using CE.Domain;

namespace CE
{
    [Serializable]
    public class WebSocketMessage
    {
        public string header;
        public string payload;
    }

    [Serializable]
    public class DistanceMessage
    {
        public string header;
        public float payload;
    }

    [Serializable]
    public class WeatherMessage
    {
        public string header;
        public WeatherSample payload;
    }

    [Serializable]
    public class PumpStatusMessage
    {
        public string header;
        public PumpStatus payload;
    }

    [Serializable]
    public class SettingsMessage
    {
        public string header;
        public Settings payload;
    }
}