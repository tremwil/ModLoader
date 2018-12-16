using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModLoader;
using UnityEngine;
using HumanAPI;
using Harmony;
using System.Reflection;

namespace CPTools
{
    [ModEntryPoint]
    public class CPTools : MonoBehaviour, ICommandHandler
    {
        ModLoader.ModLoader loader;

        public static CPTools Instance { get; private set; }

        bool cpTriggers = true;
        int lastLvlId = 0;

        public void Start()
        {
            Instance = this;     
            loader = gameObject.GetComponent<ModLoader.ModLoader>();

            try
            {
                var harmony = HarmonyInstance.Create("human.cptools");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (TypeLoadException e)
            {
                Debug.Log(e.Message);
                Debug.Log(e.StackTrace);
                loader.Log("\"" + e.Source + "\"");
            }
        }

        void Update()
        {
            if (lastLvlId != Game.instance.currentLevelNumber)
            {
                ToggleCheckpointTriggers(cpTriggers);

                foreach (Human h in Human.all)
                {
                    var spndata = h.GetComponent<SpawnpointData>();
                    if (spndata != null) { spndata.enabled = false; }
                }
            }
            lastLvlId = Game.instance.currentLevelNumber;
        }

        void ToggleCheckpointTriggers(bool state)
        {
            foreach (var cp in Game.FindObjectsOfType<Checkpoint>())
            {
                cp.enabled = state;
            }
        }

        Human GetLocalHuman()
        {
            foreach (Human h in Human.all)
            {
                if (h.player.isLocalPlayer) { return h; }
            }
            return null;
        }

        public HashSet<string> CommandNames => new HashSet<string>() { "cp-set", "cp-reset", "cp-toggle" };

        public void OnCommandTyped(string cmd, string[] args)
        {
            switch (cmd)
            {
                case "cp-toggle":
                    if (!Multiplayer.NetGame.isServer)
                    {
                        loader.LogLine("[cp-toggle] Not the game host");
                        return;
                    }
                    if (args.Length == 0)
                    {
                        cpTriggers = !cpTriggers;
                    }
                    else if (!bool.TryParse(args[0].ToLower(), out cpTriggers))
                    {
                        loader.LogLine("[cp-toggle] Invalid boolean");
                        return;
                    }
                    ToggleCheckpointTriggers(cpTriggers);
                    loader.Log(string.Format("[cp-toggle] {0} checkpoint triggers", cpTriggers ? "Enabled" : "Disabled"));

                    break;

                case "cp-reset":
                    Human h = GetLocalHuman();
                    if (h == null)
                    {
                        loader.LogLine("[cp-reset] Could not get Human object");
                        return;
                    }
                    if (!Multiplayer.NetGame.isServer)
                    {
                        loader.LogLine("[cp-reset] Not the game host");
                        return;
                    }
                    var spndata = h.GetComponent<SpawnpointData>();
                    if (spndata != null)
                    {
                        spndata.enabled = false;
                    }
                    loader.LogLine("[cp-reset] Disabled player-specific checkpoints");

                    break;

                case "cp-set":
                    h = GetLocalHuman();
                    if (h == null)
                    {
                        loader.LogLine("[cp-set] Could not get Human object");
                        return;
                    }
                    List<Human> toSet = new List<Human>() { h };
                    if (args.Length > 0 && args[0] == "all")
                    {
                        if (!Multiplayer.NetGame.isServer)
                        {
                            loader.LogLine("[cp-set] Not the game host");
                            return;
                        }
                        loader.LogLine("[cp-set] Setting spawn for all players");
                        toSet = Human.all;
                    }

                    foreach (var hu in toSet)
                    {
                        spndata = hu.GetComponent<SpawnpointData>();
                        if (spndata == null)
                        {
                            hu.gameObject.AddComponent<SpawnpointData>();
                            spndata = hu.GetComponent<SpawnpointData>();
                        }
                        spndata.enabled = true;
                        spndata.spawnpoint = h.transform.position + Vector3.up;
                    }

                    loader.LogLine("[cp-set] Set spawnpoint to " + (h.transform.position + Vector3.up).ToString());

                    break;
            }
        }

        public string ProvideHelp(string cmd)
        {
            switch (cmd)
            {
                case "cp-set":
                    return "Usage: cp-set (all)\nSets the checkpoint of the current (or all players) to the given position.";
                case "cp-reset":
                    return "Usage: cp-reset\nDisables player-specific checkpoints for all players.";
                case "cp-toggle":
                    return "Usage: cp-toggle (true|false)\nToggles auto checkpoint trigger boxes";
            }

            return "";
        }
    }
}
