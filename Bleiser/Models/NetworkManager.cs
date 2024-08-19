using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;
using System.Net.Sockets;

namespace Bleiser.Models
{
    public class MoveMessage
    {
        public int FromIndex { get; set; }
        public int ToIndex { get; set; }
        public bool IsRedTurn { get; set; }
    }
    public class NetworkManager: INetEventListener
    {
        private NetManager _netManager;
        private NetPeer _peer;
        private NetDataWriter _writer;

        public event Action<string> OnMessageReceived;
        public event Action OnServerConnected;
        public event Action OnClientConnected;

        public NetworkManager()
        {
            _netManager = new NetManager(this);
            _writer = new NetDataWriter();
        }

        public void StartServer(int port)
        {
            _netManager.Start(port);
            Console.WriteLine($"Server started on port {port}");
        }

        public void StartClient(string address, int port)
        {
            _netManager.Start();
            _peer = _netManager.Connect(address, port, "checkers");
            Console.WriteLine($"Connecting to server at {address}:{port}");
        }

        public void PollEvents()
        {
            _netManager.PollEvents();
        }

        public void SendMessage(string message)
        {
            _writer.Reset();
            _writer.Put(message);
            if (_peer == null)
            {
                Console.WriteLine("Peer is null");
                return;
            }
            _peer.Send(_writer, DeliveryMethod.ReliableOrdered);
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            request.Accept();
            OnServerConnected?.Invoke();
            Console.WriteLine("Connection Request Received");
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Console.WriteLine($"Network error: {socketError}");
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            Console.WriteLine($"Latency update for peer {peer.Address}: {latency}ms");
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            string message = reader.GetString();
            OnMessageReceived?.Invoke(message);
            Console.WriteLine("Data Received");
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnPeerConnected(NetPeer peer)
        {
            _peer = peer;
            OnClientConnected?.Invoke();
            Console.WriteLine("Peer Connected");
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine($"Peer disconnected: {peer.Address}, Reason: {disconnectInfo.Reason}");
        }
    }
}

