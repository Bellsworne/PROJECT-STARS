using Godot;
using System;
using System.Collections.Generic;

namespace DedicatedServer
{
    public class Server : Node
    {
        private int _port = 9999;
        private WebSocketServer _server;
        private List<int> _connectedPeerIDs = new List<int>();
        private Dictionary<int, string> _connectedUsers = new Dictionary<int, string>();

        public override void _Ready()
        {
            _server = new WebSocketServer();

            _server.Connect("client_connected", this, nameof(OnClientConnected));
            _server.Connect("client_disconnected", this, nameof(OnClientDisconnected));
            _server.Connect("data_received", this, nameof(OnDataReceived));

            _server.Listen(_port);
            GD.Print($"Server listening on port {_port}");
        }

        public override void _Process(float delta)
        {
            _server.Poll();
        }

        private void OnClientConnected(int id, string protocol)
        {
            GD.Print($"Peer {id} connected");
            _connectedPeerIDs.Add(id);
        }

        private void OnClientDisconnected(int id, bool wasCleanClose)
        {
            GD.Print($"Peer {id} disconnected, clean: {wasCleanClose}");
            _connectedPeerIDs.Remove(id);
            _connectedUsers.Remove(id);

            foreach (int peerID in _connectedPeerIDs)
            {
                _server.GetPeer(peerID).PutPacket(new Packet(Action.UserDisonnected, id.ToString()).ToBytes());
            }
        }

        private void OnDataReceived(int id)
        {
            string data = System.Text.Encoding.UTF8.GetString(_server.GetPeer(id).GetPacket());

            GD.Print($"Data received from peer {id}: {data}");
            
            Packet packet = PacketFactory.FromJson(data);

            if (packet != null)
            {
                switch (packet.Action)
                {
                    case Action.Chat: // User is sending a Chat message, send it to ALL clients
                        GD.Print($"Chat message from peer {id}: {packet.Payloads[0]}: {packet.Payloads[1]}");
                        foreach (int peerID in _connectedPeerIDs)
                        {
                            _server.GetPeer(peerID).PutPacket(packet.ToBytes());
                        }
                        break;

                    case Action.Login: // User is logging in, send them their unique ID
                        GD.Print($"User {id}:{packet.Payloads[0]} logged in");
                        _connectedUsers.Add(id, packet.Payloads[0]);
                        _server.GetPeer(id).PutPacket(new Packet(Action.SendUserData, id.ToString()).ToBytes());
                        
                        foreach (int peerID in _connectedPeerIDs)
                        {
                            _server.GetPeer(peerID).PutPacket(new Packet(Action.AddUser, id.ToString(), _connectedUsers[id]).ToBytes());
                        }
                        break;


                    default:
                        GD.Print($"Unknown action: {packet.Action}");
                        break;
                }
            }
        }
    }
}
