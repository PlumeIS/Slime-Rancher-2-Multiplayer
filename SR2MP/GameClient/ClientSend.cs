using GameServer;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using SR2MP;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    public static void SendTCPData(Packet _packet)
    {
        // MelonLogger.Log("Sending TCP Data with " + Client.instance.tcp.socket.Client.RemoteEndPoint);
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    public static void SendUDPData(Packet _packet)
    {
        // MelonLogger.Log("Sending UDP Data with " + Client.instance.udp.socket.Client.RemoteEndPoint);
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)Packets.Welcome))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(Environment.UserName);

            SendTCPData(_packet);
            Statics.TCPState = "<color=green>OK</color>";
        }
    }

    public static void DisconnectReceived()
    {
        using (Packet _packet = new Packet((int)Packets.Disconnect))
        {
            SendTCPData(_packet);
        }
    }

    public static void UDPTestReceived()
    {
        using (Packet _packet = new Packet((int)Packets.UDP))
        {
            _packet.Write("Received a UDP packet.");

            SendUDPData(_packet);
            Statics.UDPState = "<color=green>OK</color>";
        }
    }
    #endregion
}
