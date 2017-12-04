using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using ModLoader;
using Steamworks;
using System.IO;

namespace SuperHack
{
    [ModEntryPoint]
    public class SuperHack : MonoBehaviour, ICommandHandler
    {
        ModLoader.ModLoader loader;
        GameManager gameManager;

        MultiplayerManager networkManager;
        P2PPackageHandler packageHandler;

        bool godMode = false;
        bool infiniteAmmo = false;
        bool instantShoot = false;

        int ticksBeforeSend = 60;
        int currentTickCount = 0;

        FieldInfo ammoField;
        FieldInfo shootCounter;

        public HashSet<string> CommandNames => new HashSet<string>()
        {
            "spawn", "godmode", "infammo", "killall", "instshoot", "fly", "kick"
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

            ammoField = typeof(Fighting).GetField("bulletsLeft", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            shootCounter = typeof(Fighting).GetField("counter", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            loader.RunCommand("bind superhack:god F1 godmode");
            loader.RunCommand("bind superhack:inf F2 infammo");
            loader.RunCommand("bind superhack:inst F3 instshoot");

            loader.RunCommand("bind superhack:sp_sgun Alpha1 spawn 11");
            loader.RunCommand("bind superhack:sp_nade Alpha2 spawn 4");
            loader.RunCommand("bind superhack:sp_grav Alpha3 spawn 22");
            loader.RunCommand("bind superhack:sp_laser Alpha4 spawn 40");
            loader.RunCommand("bind superhack:sp_spak Alpha5 spawn 15");
        }

        void OnGUI()
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(Screen.width - 200, 0, 200, 40), $"{GetCurrentPlayer().GetComponent<Fighting>().CurrentWeaponIndex}");
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftShift))
            {
                GetWeaponNearMouse();
            }
            if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftControl))
            {
                DissarmPlayerNearMouse();
            }
            if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift))
            {
                KillPlayerNearMouse();
            }

            if (infiniteAmmo)
            {
                SetAmmoCount(int.MaxValue);
                GetCurrentPlayer().GetComponentInChildren<BlockAnimation>().blockPower = 1.5f;
                GetCurrentPlayer().GetComponent<BlockHandler>().sinceBlockStart = 0f;

                FieldInfo timer = typeof(BlockAnimation).GetField("sinceBlockStart", BindingFlags.NonPublic | BindingFlags.Instance);
                timer.SetValue(GetCurrentPlayer().GetComponentInChildren<BlockAnimation>(), 0f);
            }

            if (instantShoot)
            {
                shootCounter.SetValue(GetCurrentPlayer().GetComponent<Fighting>(), float.PositiveInfinity);
            }

            if (currentTickCount == ticksBeforeSend)
            {
                currentTickCount = 0;

                if (godMode)
                {
                    GetCurrentPlayer().GetComponent<NetworkPlayer>().UnitWasDamaged(float.NegativeInfinity, false, DamageType.Punch);
                }
            }
            currentTickCount++;
        }

        Controller[] ActiveControllers()
        {
            return networkManager.PlayerControllers.Where(x => x != null).ToArray();
        }

        Controller GetCurrentPlayer()
        {
            foreach (Controller player in ActiveControllers())
            {
                if (player.HasControl)
                {
                    return player;
                }
            }

            return null;
        }

        void ToggleGodMode(bool isOn)
        {
            if (godMode == isOn) { return; }
            godMode = isOn;

            if (godMode == true)
            {
                loader.LogLine("[SuperHack] God mode enabled!");
            }
            else
            {
                loader.LogLine("[SuperHack] God mode disabled!");
            }
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

        void KillAllHack()
        {
            foreach (Controller player in gameManager.playersAlive)
            {
                if (!player.HasControl)
                {
                    player.GetComponent<NetworkPlayer>().UnitWasDamaged(0, true);
                }
            }
            loader.LogLine("\n[SuperHack] Killed everyone!");
        }

        void SetAmmoCount(int bulletsLeft)
        {
            Fighting fight = GetCurrentPlayer().GetComponent<Fighting>();

            if (fight.weapon)
            {
                ammoField.SetValue(fight, bulletsLeft);
                fight.weapon.secondsOfUseLeft = (float)bulletsLeft;
            }
        }

        void KillPlayerNearMouse()
        {
            List<Controller> clients = gameManager.playersAlive;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Controller closestPlayer = null;
            float bestDist = float.PositiveInfinity;

            for (byte i = 0; i < clients.Count; i++)
            {
                if (clients[i] == null || clients[i].HasControl) { continue; }

                Vector3 pos = clients[i].transform.GetComponentInChildren<Torso>().transform.position;

                float dx = mousePos.y - pos.y, dy = mousePos.z - pos.z;
                float dist = dx * dx + dy * dy;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    closestPlayer = clients[i];
                }
            }

            if (closestPlayer != null && bestDist <= 9)
            {
                closestPlayer.GetComponent<NetworkPlayer>().UnitWasDamaged(0, true);
                loader.LogLine($"[SuperHack] Sent kill packet to player ID ${closestPlayer.playerID}");
            }
            else
            {
                loader.LogLine("[SuperHack] Could not find player to send packet to");
            }
        }

        void DissarmPlayerNearMouse()
        {
            List<Controller> clients = gameManager.playersAlive;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Controller closestPlayer = null;
            float bestDist = float.PositiveInfinity;

            for (byte i = 0; i < clients.Count; i++)
            {
                if (clients[i] == null || clients[i].HasControl) { continue; }

                Vector3 pos = clients[i].transform.GetComponentInChildren<Torso>().transform.position;

                float dx = mousePos.y - pos.y, dy = mousePos.z - pos.z;
                float dist = dx * dx + dy * dy;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    closestPlayer = clients[i];
                }
            }

            if (closestPlayer != null && bestDist <= 9)
            {
                closestPlayer.GetComponent<NetworkPlayer>().ThrowWeapon(true,
                    closestPlayer.GetComponent<Fighting>().CurrentWeaponIndex, 
                    new Vector3(0, -100, -100), Vector3.up, Vector3.zero);

                loader.LogLine($"[SuperHack] Sent dissarm packet to player ID ${closestPlayer.playerID}");
            }
            else
            {
                loader.LogLine("[SuperHack] Could not find player to send packet to");
            }
        }

        void GetWeaponNearMouse()
        {
            WeaponPickUp[] weapons = FindObjectsOfType<WeaponPickUp>();
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            float minDist = float.PositiveInfinity;
            int minIndex = -1;

            loader.LogLine($"[SuperHack] Found {weapons.Length} weapon pick ups!");

            for (int i = 0; i < weapons.Length; i++)
            {
                Vector3 pos = weapons[i].transform.position;
                if (pos.x == -4.3) { continue;  }

                float dx = mousePos.y - pos.y, dy = mousePos.z - pos.z;
                float dist = dx * dx + dy * dy;

                if (dist < minDist)
                {
                    minDist = dist;
                    minIndex = i;
                }
            }

            if (minIndex != -1)
            {
                networkManager.RequestWeaponPickUp(weapons[minIndex].NetworkSpawnIndex, networkManager.LocalPlayerIndex);
                loader.LogLine($"[SuperHack] Requested weapon pick up (spawn ID {weapons[minIndex].NetworkSpawnIndex})");
            }
            else
            {
                loader.LogLine("[SuperHack] No weapon found!");
            }
        }

        public void OnCommandTyped(string cmd, string[] args)
        {
            switch (cmd)
            {
                case "spawn":
                    if (args.Length != 1 || !byte.TryParse(args[0], out byte id))
                    {
                        loader.LogLine("[SuperHack:spawn] Invalid arguments");
                        return;
                    }

                    Vector3 pos = GetCurrentPlayer().transform.GetComponentInChildren<Torso>().transform.position;
                    GetCurrentPlayer().GetComponent<NetworkPlayer>().ThrowWeapon(false, id, pos, new Vector3(0, 1, 0), new Vector3(0, 0, 0));
                    break;

                case "godmode":
                    if (args.Length == 0) { ToggleGodMode(!godMode); }
                    else { ToggleGodMode((args[0] == "1") ? true : false); }
                    break;

                case "infammo":
                    if (args.Length == 0) { infiniteAmmo = !infiniteAmmo; }
                    else { infiniteAmmo = (args[0] == "1") ? true : false; }

                    if (infiniteAmmo == true) { SetAmmoCount(0); }
                    loader.LogLine(infiniteAmmo ? "Infinite ammo turned ON!" : "Infinite ammo turned OFF!");
                    break;

                case "killall":
                    KillAllHack();
                    break;

                case "instshoot":
                    if (args.Length == 0) { instantShoot = !instantShoot; }
                    else { instantShoot = (args[0] == "1") ? true : false; }

                    loader.LogLine(instantShoot ? "Instant shoot turned ON!" : "Instant shoot turned OFF!");
                    break;

                case "fly":
                    Controller player = GetCurrentPlayer();
                    if (args.Length == 0) { player.canFly = !player.canFly; }
                    else { player.canFly = (args[0] == "1") ? true : false; }

                    loader.LogLine(player.canFly ? "Fly mode turned ON!" : "Fly mode turned OFF!");
                    break;

                case "kick":
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
                    break;
            }
        }

        public string ProvideHelp(string cmd)
        {
           switch (cmd)
            {
                case "spawn":
                    return "Usage: spawn [id]\nSpawns a weapon to use from its weapon ID.";
                case "godmode":
                    return "Usage: godmode (0|1)\nToggles god mode on or off.";
                case "infammo":
                    return "Usage: infammo (0|1)\nToggles infinite ammo on or off.";
                case "killall":
                    return "Usage: killall\nKills all other players in the game.";
                case "instshoot":
                    return "Usage: instshoot (0|1)\nToggles instant shooting on or off.";
                case "fly":
                    return "Usage: fly (0|1)\nToggles flymode on or off.";
                case "kick":
                    return "Usage: kick [Player color]\nKicks the player of the specified color.";
                default: return "";
            }
        }
    }
}
