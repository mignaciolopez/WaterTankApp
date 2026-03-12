namespace CE.Domain
{
    public enum ErrorSeverity : byte
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Critical = 3
    };

    public enum ErrorType : byte
    {
        ServiceStuck = 0,
        ServiceCrash = 1,
        WiFiDisconnect = 2,
        LowMemory = 3,
        StackOverflow = 4,
        QueueEmpty = 5,
        QueueFull = 6,
        SensorFailure = 7,
        ConfigError = 8,
        TaskDeleted = 9,
        WaterLevel = 10,
        PumpTimeout = 11,
        Unknown = 255,
    };
}