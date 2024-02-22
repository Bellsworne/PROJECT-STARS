using Godot;
using System;
using System.Collections.Generic;

public class NetworkHelper : Node
{
    public static int ID;
    public string Username;

    public event Action<int, Transform2D> PlayerTransformSyncRecieved;

    [Export] private NodePath _clientUIPath;
    [Export] private PackedScene _playerScene;
    private TEST_CLIENT_UI _clientUI;
    private WebSocketClient _client;
    private Dictionary<int, string> _connectedUsers = new Dictionary<int, string>();

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

    public void SendPacketToServer(Packet packet)
    {
        var data = Packet.CreatePacket(packet);
        _client.GetPeer(1).PutPacket(data);
    }

    private void OnJoinServer(string address, int port, string username)
    {
        _client = new WebSocketClient();

        _client.Connect("connection_established", this, nameof(OnConnectionEstablished));
        _client.Connect("connection_closed", this, nameof(OnConnectionClosed));
        _client.Connect("data_received", this, nameof(OnDataReceived));

        _client.ConnectToUrl($"ws://{address}:{port}");

        Username = _clientUI.Username;
    }

    private void OnSendMessage(string message)
    {
        
        var packet = new Packet("Chat", new List<object> { Username, message });
        var data = Packet.CreatePacket(packet);

        GD.Print($"Sending message to server: {packet}");

        _client.GetPeer(1).PutPacket(data);
    }

    private void OnConnectionEstablished(string protocol)
    {
        _clientUI.LogMessageToChat($"Connected to server, logging in with {Username}.");

        Packet packet = new Packet("Login", new List<object> {Username});
        _client.GetPeer(1).PutPacket(Packet.CreatePacket(packet));
    }

    private void OnConnectionClosed(bool wasClean)
    {
        _clientUI.LogMessageToChat("Disconnected from server");
    }

    private void OnDataReceived()
    {
        byte[] payload = _client.GetPeer(1).GetPacket();
        string message = System.Text.Encoding.UTF8.GetString(payload);
        
        var (action, payloads) = Packet.JsonToActionPayloads(message);

        GD.Print($"Received message from server: {message}");

        switch (action)
        {
            case "Chat": // Recieving a Chat message
                OnChatReceived((string)payloads[0], (string)payloads[1]);
                break;
            case "SendUserData": // Server is sending the UID for this client
                ID = int.Parse((string)payloads[0]);
                _clientUI.LogMessageToChat($"My ID is {ID}");
                SpawnPlayer(ID);
                break;
            case "UserDisconnected": // User is disconnecting
                _clientUI.LogMessageToChat($"User disconnected: {_connectedUsers[int.Parse((string)payloads[0])]}");
                _clientUI.RemoveConnectedUser(_connectedUsers[int.Parse((string)payloads[0])]);
                _connectedUsers.Remove(int.Parse((string)payloads[0]));
                break;
            case "AddUser": // Add a user
                _clientUI.LogMessageToChat($"User connected: {payloads[1]}");
                _connectedUsers.Add(int.Parse((string)payloads[0]), (string)payloads[1]);
                _clientUI.AddConnectedUser((string)payloads[1]);
                SpawnPlayer(int.Parse((string)payloads[0]));
                break;
            case "PlayerTransformSync": // Sync player transforms
                GD.Print($"Player transform sync: {payloads[0]}");
                // int id = int.Parse((string)payloads[0]);
                // Transform2D transform = (Transform2D)payloads[1];
                // PlayerTransformSyncRecieved?.Invoke(id, transform);
                // todo: implement this
                break;
            default:
                _clientUI.LogErrorToChat($"Packet error: Unknown action '{action}'");
                break;
        }
    }

    private void OnChatReceived(string username, string message)
    {
        _clientUI.AddMessage($"{username}: {message}");
    }

    private void SpawnPlayer(int id)
    {
        var player = _playerScene.Instance() as Player;
        player.ID = id;
        player.Name = id.ToString();
        GetTree().Root.GetNode("MAIN").GetNode("World").AddChild(player);
    }
}
