using MelonLoader;
using System;
using System.Collections.Generic;
using System.Text;
using SR2MP;

namespace GameServer
{
    class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

            MelonLogger.Msg($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
            Server.playerList[_fromClient] = _username;
            if (_fromClient != _clientIdCheck)
            {
                MelonLogger.Msg($"Server: Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
            Server.sendPlayerListUpdate();
        }

        public static void DisconnectReceived(int _fromClient, Packet _packet)
        {
            Server.clients[_fromClient].Disconnect();
            Server.playerList[_fromClient] = null;
            MelonLogger.Msg($"Server: Received disconnect packet from ID: {_fromClient}.");
            Server.sendPlayerListUpdate();
        }

        public static void UDPTestReceived(int _fromClient, Packet _packet)
        {
            string _msg = _packet.ReadString();

            MelonLogger.Msg($"Server: Received packet via UDP. Contains message: {_msg}");
        }

        public static void TCPDataReceived(int _fromClient, Packet _packet)
        {
            ServerSend.SendData(_fromClient, _packet, 0);
        }

        public static void UDPDataReceived(int _fromClient, Packet _packet)
        {
            ServerSend.SendData(_fromClient, _packet, 1);
        }
    }
}
