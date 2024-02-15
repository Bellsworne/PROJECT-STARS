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

        public override void _Ready()
        {
            _server = new WebSocketServer();

            _server.Connect("peer_connected", this, nameof(OnPeerConnected));
            _server.Connect("peer_disconnected", this, nameof(OnPeerDisconnected));
            _server.Connect("data_received", this, nameof(OnDataReceived));

            _server.Listen(_port);
            GD.Print($"Server listening on port {_port}");
        }

        public override void _Process(float delta)
        {
            _server.Poll();
        }

        private void OnPeerConnected(int id)
        {
            GD.Print($"Peer {id} connected");
            _connectedPeerIDs.Add(id);
        }

        private void OnPeerDisconnected(int id)
        {
            GD.Print($"Peer {id} disconnected");
            _connectedPeerIDs.Remove(id);
        }

        private void OnDataReceived(int id)
        {
            byte[] payload = _server.GetPeer(id).GetPacket();
            string message = System.Text.Encoding.UTF8.GetString(payload);
            GD.Print($"Received message from {id}: {message}");

            foreach (int peerID in _connectedPeerIDs)
            {
                if (peerID != id)
                {
                    _server.GetPeer(peerID).PutPacket(payload);
                }
            }
        }
    }
}
