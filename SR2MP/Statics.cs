using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SR2MP
{
    public static class Statics
    {
        public static bool IsMultiplayer = true;
        public static string SecondPlayerName = "None";
        public static bool Host;
        public static bool FriendInGame;
        public static bool JoinedTheGame;
        public static bool HandlePacket;
        public static Dictionary<int, string> PlayerList = new Dictionary<int, string>();

        public static string TCPState = "<color=red>No</color>";
        public static string UDPState = "<color=red>No</color>";
        public static string Message;
    }
}
