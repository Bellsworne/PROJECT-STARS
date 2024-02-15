using Godot;
using System;

public class NetworkHelper : Node
{
    [Export] private NodePath _clientUIPath;

    private TEST_CLIENT_UI _clientUI;
    private WebSocketClient _client;

    public override void _Ready()
    {
        if (_clientUIPath != null)
        {
            _clientUI = GetNode<TEST_CLIENT_UI>(_clientUIPath);
            _clientUI.SendMessage += OnSendMessage;
            _clientUI.JoinServer += OnJoinServer;
        }
    }

    public override void _Process(float delta)
    {
        if (_client != null)
        {
            _client.Poll();
        }
    }

    private void OnJoinServer(string address, int port, string username)
    {
        _client = new WebSocketClient();

        _client.Connect("connection_established", this, nameof(OnConnectionEstablished));
        _client.Connect("connection_closed", this, nameof(OnConnectionClosed));
        _client.Connect("data_received", this, nameof(OnDataReceived));

        _client.ConnectToUrl($"ws://{address}:{port}");
    }

    private void OnSendMessage(string message)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
        _client.GetPeer(1).PutPacket(data);
    }

    private void OnConnectionEstablished(string protocol)
    {
        GD.Print("Connected to server");
    }

    private void OnConnectionClosed(bool wasClean)
    {
        GD.Print("Disconnected from server");
    }

    private void OnDataReceived()
    {
        byte[] payload = _client.GetPeer(1).GetPacket();
        string message = System.Text.Encoding.UTF8.GetString(payload);
        GD.Print($"Received message from server: {message}");
        _clientUI.AddMessage(message);
    }
}
