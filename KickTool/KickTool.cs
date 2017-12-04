using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModLoader;
using Steamworks;

namespace KickTool
{
    [ModEntryPoint]
    public class KickTool : MonoBehaviour, ICommandHandler
    {
        ModLoader.ModLoader loader;
        GameManager gameManager;

        MultiplayerManager networkManager;
        P2PPackageHandler packageHandler;

        public HashSet<string> CommandNames => new HashSet<string>()
        {
            "kick"
        };

        Dictionary<string, Color> playerCols = new Dictionary<string, Color>()
        {
            ["red"] = new Color(0.838f, 0.335f, 0.302f, 1f),
            ["green"] = new Color(0.339f, 0.544f, 0.288f, 1f),
            ["blue"] = new Color(0.333f, 0.449f, 0.676f, 1f),
            ["yellow"] = new Color(0.846f, 0.549f, 0.280f, 1f)
        };

        void Start()
        {
            loader = gameObject.GetComponent<ModLoader.ModLoader>();
            gameManager = gameObject.GetComponent<GameManager>();
            networkManager = FindObjectOfType<MultiplayerManager>();
            packageHandler = FindObjectOfType<P2PPackageHandler>();
        }

        bool ColorEquals(Color c1, Color c2)
        {
            bool rEq = Mathf.RoundToInt(1000 * c1.r) == Mathf.RoundToInt(1000 * c2.r),
                 gEq = Mathf.RoundToInt(1000 * c1.g) == Mathf.RoundToInt(1000 * c2.g),
                 bEq = Mathf.RoundToInt(1000 * c1.b) == Mathf.RoundToInt(1000 * c2.b),
                 aEq = Mathf.RoundToInt(1000 * c1.a) == Mathf.RoundToInt(1000 * c2.a);

            return rEq && gEq && bEq && aEq;
        }

        void KickPlayer(string color)
        {
            ConnectedClientData tgtClient = null;
            Color tgtCol = playerCols[color];

            foreach (ConnectedClientData client in networkManager.ConnectedClients)
            {
                Color col = client.PlayerObject.GetComponentInChildren<SpriteRenderer>().GetComponent<SetColorWhenDamaged>().startColor;
                if (ColorEquals(tgtCol, col))
                {
                    tgtClient = client;
                    break;
                }
            }
            if (tgtClient == null)
            {
                loader.LogLine($"[SuperHack] There is no player of color {color}");
            }

            byte[] data = new byte[1] { 0 };
            packageHandler.SendP2PPacketToUser(tgtClient.ClientID, data, P2PPackageHandler.MsgType.KickPlayer, EP2PSend.k_EP2PSendReliable, 0);
            loader.LogLine($"Kicked player {tgtClient.PlayerName} ({color})");
        }

        public void OnCommandTyped(string cmd, string[] args)
        {
            if (cmd == "kick")
            {
                if (args.Length == 0)
                {
                    loader.LogLine("[SuperHack] Invalid arguments");
                    return;
                }
                string color = args[0].ToLower();
                if (playerCols.ContainsKey(color))
                {
                    KickPlayer(color);
                }
                else
                {
                    loader.LogLine("[SuperHack] No such color");
                }
            }
        }

        public string ProvideHelp(string cmd)
        {
            if (cmd == "kick")
            {
                return "Usage: kick [Player color]\nKicks the player of the specified color.";
            }
            return null;
        }
    }
}
