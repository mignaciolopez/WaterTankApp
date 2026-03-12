using System;

namespace CE.Domain
{
    [Serializable]
    public class PumpStatus
    {
        public bool isOnCooldown;
        public bool isOn;
        public long timeStampOn;
        public long timeStampCooldown;
    }

    [Serializable]
    public class WeatherSample
    {
        public float humidity;
        public float temperatureC;
        public float heatIndexC;
    }

    [Serializable]
    public class Settings
    {
        // Tank Water Distance levels measured from bottom to top
        public ushort heightCm                    ;  //Water tank height, max distance to measure
        public ushort criticalLevelCm             ;  //Critical water level to START pumping even during Nighttime.
        public ushort minLevelCm                  ;  //Minimum water level to START pumping
        public ushort maxLevelCm                  ;  //Maximum water level to STOP pumping

        // Measure delays
        public ushort radarDelayS                 ;  //Delay between distance measures
        public ushort weatherDelayS               ;  //Delay between weather updates
        public ushort medianWindow                ;  //Number of measures to median

        // Watchdog settings
        public ushort watchdogCheckIntervalS      ;  //Delay between health checks
        public ushort wifiReconnectTimeoutS       ;  //WiFi reconnect attempt timeout
        public ushort maxServiceRestarts          ;  //Max service restart attempts before ESP restart
        public ushort minFreeHeapThresholdKb      ;  //Minimum free heap (KB) before warning
        public ushort maxErrorReports             ;  //Max error reports to store in SPIFFS

        // Security Settings
        public ushort pumpMaxTimeOnM              ;  //Max time in minutes Pump can be On
        public ushort pumpCooldownTimeM           ;  //Time in minutes Pump needs to cool down after a max Time On triggered.

        // Distance
        public ushort filteredDistanceOffsetCm    ;  //Distance offset in cm
    }
}