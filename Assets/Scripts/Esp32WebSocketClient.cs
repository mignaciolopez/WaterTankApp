using CE.Domain;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CE;
using UnityEngine;

public class Esp32WebSocketClient : MonoBehaviour
{
    [SerializeField] private string esp32IP = "192.168.1.30"; // Change to your ESP32 IP
    [SerializeField] private int port = 80;
    [SerializeField] private float reconnectDelay = 5f;

    private ClientWebSocket _ws;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isRunning;

    // Events for data updates
    public event Action<float>         OnWaterLevelUpdate;
    public event Action<WeatherSample> OnTemperatureUpdate;
    public event Action<PumpStatus>    OnPumpStatusUpdate;
    public event Action<bool>          OnConnectionChanged;
    public event Action<Settings>      OnSettingsUpdate;

    public bool IsConnected => _ws?.State == WebSocketState.Open;

    private async void Start()
    {
        try
        {
            _isRunning = true;
            await ConnectWebSocket();
        }
        catch (Exception e)
        {
            _isRunning = false;
            Debug.LogError($"WebSocket connection failed: {e.Message}");
        }
    }

    private async Task ConnectWebSocket()
    {
        while (_isRunning)
        {
            try
            {
                _ws = new ClientWebSocket();
                _cancellationTokenSource = new CancellationTokenSource();

                var uri = $"ws://{esp32IP}:{port}/ws";
                Debug.Log($"Connecting to {uri}...");

                await _ws.ConnectAsync(new Uri(uri), _cancellationTokenSource.Token);

                Debug.Log("WebSocket Connected!");
                OnConnectionChanged?.Invoke(true);

                // Request initial status
                await RequestStatus();

                // Start receiving messages
                _ = ReceiveMessages();

                return; // Exit reconnection loop on success
            }
            catch (Exception e)
            {
                Debug.LogError($"WebSocket connection failed: {e.Message}");
                OnConnectionChanged?.Invoke(false);

                await Task.Delay(TimeSpan.FromSeconds(reconnectDelay));
            }
        }
    }

    private async Task ReceiveMessages()
    {
        var buffer = new byte[4096];

        try
        {
            while (_ws.State == WebSocketState.Open && _isRunning)
            {
                var result = await _ws.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    _cancellationTokenSource.Token
                );

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Schedule message handling on Unity main thread
                    UnityMainThreadDispatcher.Instance.Enqueue(() => HandleMessage(message));
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("WebSocket closed by server");
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"WebSocket receive error: {e.Message}");
        }

        OnConnectionChanged?.Invoke(false);

        // Attempt reconnection
        if (_isRunning)
        {
            await Task.Delay(TimeSpan.FromSeconds(reconnectDelay));
            await ConnectWebSocket();
        }
    }

    private void HandleMessage(string message)
    {
        Debug.Log($"Received: {message}");

        try
        {
            // First, parse to get the header
            var msg = JsonUtility.FromJson<WebSocketMessage>(message);

            if (msg == null || string.IsNullOrEmpty(msg.header))
            {
                Debug.LogWarning("Received message with no header");
                return;
            }

            switch (msg.header)
            {
                case "distance":
                {
                    var data = JsonUtility.FromJson<DistanceMessage>(message);
                    OnWaterLevelUpdate?.Invoke(data.payload);
                    break;
                }
                case "weather":
                {
                    var data = JsonUtility.FromJson<WeatherMessage>(message);
                    OnTemperatureUpdate?.Invoke(data.payload);
                    break;
                }
                case "pumpStatus":
                {
                    var data = JsonUtility.FromJson<PumpStatusMessage>(message);
                    OnPumpStatusUpdate?.Invoke(data.payload);
                    break;
                }
                case "settings":
                {
                    var data = JsonUtility.FromJson<SettingsMessage>(message);
                    OnSettingsUpdate?.Invoke(data.payload);
                    break;
                }
                default:
                    Debug.LogWarning($"Unknown message type: {msg.header}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing message: {e.Message}");
        }
    }

    // Send commands to ESP32
    public async Task SetPumpState(bool state)
    {
        var command = new PumpCommand { command = "setPump", state = state };
        await SendMessage(JsonUtility.ToJson(command));
    }

    public async Task RequestStatus()
    {
        var command = new StatusCommand { command = "getStatus" };
        await SendMessage(JsonUtility.ToJson(command));
    }

    private new async Task SendMessage(string message)
    {
        if (_ws?.State == WebSocketState.Open)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send message: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("WebSocket is not connected");
        }
    }

    private async void OnApplicationQuit()
    {
        try
        {
            _isRunning = false;

            if (_ws?.State == WebSocketState.Open)
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Application closing", CancellationToken.None);
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _ws?.Dispose();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error closing WebSocket: {e.Message}");
        }
    }
}