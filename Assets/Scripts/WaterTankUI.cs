using CE.Domain;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaterTankUI : MonoBehaviour
{
    [SerializeField] private Esp32WebSocketClient websocketClient;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI waterLevelText;
    [SerializeField] private TextMeshProUGUI temperatureText;
    [SerializeField] private TextMeshProUGUI humidityText;
    [SerializeField] private TextMeshProUGUI hIndexText;
    [SerializeField] private TextMeshProUGUI connectionStatusText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Slider pumpSwitch;

    private Settings _settings;

    [SerializeField] private float refreshInterval = 1.0f;

    private void Start()
    {
        // Subscribe to WebSocket events
        websocketClient.OnWaterLevelUpdate += UpdateWaterLevel;
        websocketClient.OnTemperatureUpdate += UpdateTemperature;
        websocketClient.OnPumpStatusUpdate += UpdatePumpStatus;
        websocketClient.OnConnectionChanged += UpdateConnectionStatus;
        websocketClient.OnSettingsUpdate += OnSettingsUpdate;

        StartCoroutine(RequestStatusRoutine());
    }

    private IEnumerator RequestStatusRoutine()
    {
        while (true) // Loop forever
        {
            yield return new WaitForSeconds(refreshInterval);
            RequestStatus();
        }
        // ReSharper disable once IteratorNeverReturns
        // Intended behavior
    }

    private async void RequestStatus()
    {
        try
        {
            await websocketClient.RequestStatus();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void UpdateWaterLevel(float distance)
    {
        waterLevelText.text = $"Distancia al Agua: {distance:F1} cm";
        progressBar.value = (_settings.heightCm - distance) / _settings.maxLevelCm * 100.0f;
    }

    private void UpdateTemperature(WeatherSample sample)
    {
        temperatureText.text = $"Temperatura: {sample.temperatureC:F1}°C";
        humidityText.text = $"Humedad Relativa: {sample.humidity:F1}%";
        hIndexText.text = $"Sensación Térmica: {sample.heatIndexC:F1}°C";
    }

    private void UpdatePumpStatus(PumpStatus sample)
    {
        pumpSwitch.value = sample.isOn ? 1f : 0f;
    }

    private void UpdateConnectionStatus(bool isConnected)
    {
        connectionStatusText.text = isConnected ? "Conectado" : "Desconectado";
        connectionStatusText.color = isConnected ? Color.green : Color.red;
    }

    private void OnSettingsUpdate(Settings settings)
    {
        _settings = settings;
    }

    public async void OnPumpToggleChanged()
    {
        try
        {
            await websocketClient.SetPumpState(pumpSwitch.value > .0f);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    void OnDestroy()
    {
        websocketClient.OnWaterLevelUpdate -= UpdateWaterLevel;
        websocketClient.OnTemperatureUpdate -= UpdateTemperature;
        websocketClient.OnPumpStatusUpdate -= UpdatePumpStatus;
        websocketClient.OnConnectionChanged -= UpdateConnectionStatus;
        websocketClient.OnSettingsUpdate -= OnSettingsUpdate;
    }
}