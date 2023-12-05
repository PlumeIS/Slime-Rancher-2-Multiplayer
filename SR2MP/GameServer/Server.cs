using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using MelonLoader;
using SR2MP;

namespace GameServer
{
    class Server
    {
        public static int MaxPlayers { get; private set; }

        public static Dictionary<int, String> playerList = new Dictionary<int, string>();
        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            MelonLogger.Msg("Server: Starting server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            udpListener = new UdpClient(Port+1);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            MelonLogger.Msg($"Server: Server started on port {Port}.");
        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            MelonLogger.Msg($"Server: Incoming connection from {_client.Client.RemoteEndPoint}...");

            int cliendId = GetEmptyClient();
            if (cliendId != 0)
            {
                clients[cliendId].tcp.Connect(_client);
                return;
            }
            else
            {
                MelonLogger.Msg($"Server: {_client.Client.RemoteEndPoint} failed to connect: Server full!");
            }
            // for (int i = 1; i <= MaxPlayers; i++)
            // {
            //     if (clients[i].tcp.socket == null)
            //     {
            //         clients[i].tcp.Connect(_client);
            //         return;
            //     }
            // }

            MelonLogger.Msg($"Server: {_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static int GetEmptyClient()
        {
            int clientId = 0;
            foreach (KeyValuePair<int,Client> client in clients.Reverse())
            {
                if (!client.Value.isConnected)
                {
                    clientId = client.Key;
                }
            }

            return clientId;
        }

        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                {
                    return;
                }

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();

                    if (_clientId == 0)
                    {
                        return;
                    }

                    if (clients[_clientId].udp.endPoint == null)
                    {
                        clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        clients[_clientId].udp.HandleData(_packet);
                    }
                }
            }
            catch (Exception _ex)
            {
                MelonLogger.Msg($"Server: Error receiving UDP data: {_ex}");
            }
        }

        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                MelonLogger.Msg($"Server: Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }

        public static void sendPlayerListUpdate()
        {
            Statics.PlayerList = new Dictionary<int, string>();
            foreach (KeyValuePair<int,Client> client in clients)
            {
                String playerName;
                playerList.TryGetValue(client.Key, out playerName);
                if (playerName == null)
                {
                    playerName = "NULL_PLAYER_NAME_FOR_WAITING";
                }
                SendData.SendPlayerList(client.Key, playerName);
                Statics.PlayerList[client.Key] = playerName;
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)Packets.Welcome, ServerHandle.WelcomeReceived },
                { (int)Packets.Disconnect, ServerHandle.DisconnectReceived},
                { (int)Packets.UDP, ServerHandle.UDPTestReceived },
                { (int)Packets.Message, ServerHandle.TCPDataReceived },
                { (int)Packets.Movement, ServerHandle.UDPDataReceived },
                { (int)Packets.Animations, ServerHandle.UDPDataReceived },
                { (int)Packets.CameraAngle, ServerHandle.UDPDataReceived },
                { (int)Packets.VacconeState, ServerHandle.TCPDataReceived },
                { (int)Packets.GameMode, ServerHandle.TCPDataReceived },
                { (int)Packets.PlayerList, ServerHandle.TCPDataReceived},
                { (int)Packets.Time, ServerHandle.UDPDataReceived },
                { (int)Packets.SaveRequest, ServerHandle.TCPDataReceived },
                { (int)Packets.Save, ServerHandle.TCPDataReceived },
                { (int)Packets.LandPlotUpgrade, ServerHandle.TCPDataReceived },
                { (int)Packets.LandPlotReplace, ServerHandle.TCPDataReceived },
                { (int)Packets.Sleep, ServerHandle.TCPDataReceived },
                { (int)Packets.Currency, ServerHandle.TCPDataReceived },
                { (int)Packets.Actors, ServerHandle.UDPDataReceived }
            };
            MelonLogger.Msg("Server: Initialized packets.");
        }
    }
}
