using GameServer;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SR2MP
{
    public class CustomLobby : MonoBehaviour
    {
        public static CustomLobby Instance;

        //Stuff
        private static bool _allowToHostServer = true;
        private static bool _allowToConnectServer = true;
        private static bool _hostingServer = false;
        private static bool _connectingServer = false;
        private int maxPlayer = 2;

        public void CustomMenu()
        {
            GUIStyle style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(25f, 35f, 55f, 25f), "ip:", style);
            GUI.Label(new Rect(25f, 65f, 55f, 25f), "port:", style);
            GUI.Label(new Rect(25f, 95f, 105f, 25f), "maxPlayer:", style);
            style.alignment = TextAnchor.LowerCenter;

            if (_allowToHostServer && _allowToConnectServer)
            {
                
                Client.instance.ip = GUI.TextField(new Rect(65f, 35f, 100f, 25f), Client.instance.ip);
                Client.instance.port = int.Parse(GUI.TextField(new Rect(65f, 65f, 100f, 25f), Client.instance.port.ToString()));
                maxPlayer = int.Parse(GUI.TextField(new Rect(115f, 95f, 50f, 25f), maxPlayer.ToString()));
            }
            else
            {
                GUI.Label(new Rect(65f, 35f, 100f, 25f), Client.instance.ip);
                GUI.Label(new Rect(65f, 65f, 100f, 25f), Client.instance.port.ToString());
                GUI.Label(new Rect(65f, 95f, 100f, 25f), maxPlayer.ToString());
            }

            if (_allowToHostServer)
            {
                if (GUI.Button(new Rect(15f, 125f, 150f, 25f), "Host server"))
                {
                    ServerInit.Start(maxPlayer, Client.instance.port);
                    Client.instance.ConnectToServer();
                    _allowToHostServer = false;
                    _allowToConnectServer = false;
                    _hostingServer = true;
                    Statics.Host = true;
                }
            }
            else if (_hostingServer)
            {
                GUI.Label(new Rect(15f, 125f, 150f, 25f), "Server hosting...");
            }
            else
            { GUI.Label(new Rect(15f, 125f, 150f, 25f), "Host Server"); }

            if (_allowToConnectServer)
            {
                if (GUI.Button(new Rect(15f, 155f, 150f, 25f), "Connect to server"))
                {
                    Client.instance.ConnectToServer();
                    _allowToHostServer = false;
                    _allowToConnectServer = false;
                    _connectingServer = true;
                }
            }
            else if (_connectingServer)
            {
                GUI.Label(new Rect(15f, 155f, 150f, 25f), "Connected");
            }
            else
            {
                GUI.Label(new Rect(15f, 155f, 150f, 25f), "Connect to server");
            }

            //GUI.Label(new Rect(15f, 125f, 150f, 25f), "Connected friend:");
            //GUI.Label(new Rect(15f, 155f, 150f, 25f), GlobalStuff.SecondPlayerName);

            //if (GlobalStuff.SecondPlayerName != "None")
            {
                string inGame = Statics.FriendInGame ? "<color=green>YES</color>" : "<color=red>NO</color>";
                GUI.Label(new Rect(15f, 185f, 150f, 25f), $"Friend in game: {inGame}");

                if (!Statics.JoinedTheGame)
                {
                    if (Statics.FriendInGame)
                    {
                        if (!SRSingleton<SystemContext>.Instance.SceneLoader.CurrentSceneGroup.IsGameplay)
                        {
                            if (GUI.Button(new Rect(40f, 215f, 100f, 25f), "Join"))
                            {
                                Statics.JoinedTheGame = true;
                                SendData.RequestSave();
                            }
                        }
                    }
                }
            }
            if (_hostingServer)
            {
                GUI.Label(new Rect(15f, 245f, 150f, 25f), "TCP Port: <color=yellow>" + Client.instance.port + "</color>");
                GUI.Label(new Rect(15f, 275f, 150f, 25f), "UDP Port: <color=yellow>" + (Client.instance.port+1) + "</color>");
            }

            if (_connectingServer)
            {
                GUI.Label(new Rect(15f, 245f, 150f, 25f), "TCP: <color=yellow>" + Client.instance.port + "</color> State: " + Statics.TCPState);
                GUI.Label(new Rect(15f, 275f, 150f, 25f), "UDP: <color=yellow>" + (Client.instance.port + 1) + "</color> State: " + Statics.UDPState);
                if (GUI.Button(new Rect(15f, 305f, 150f, 25f), "Disconnect"))
                {
                    Client.Disconnect();
                }
            }
        }

        void Start()
        {
            Instance = this;
            CreateCustomClientManager();
        }

        public static void ResetConnectionState()
        {
            _allowToHostServer = true;
            _allowToConnectServer = true;
            _hostingServer = false;
            _connectingServer = false;

            Statics.JoinedTheGame = false;
            Statics.FriendInGame = false;
        }

        public static void CreateCustomClientManager()
        {
            var clientManager = new GameObject("ClientManager");
            DontDestroyOnLoad(clientManager);
            clientManager.AddComponent<Client>();
            clientManager.AddComponent<ThreadManager>();
        }
    }
}
