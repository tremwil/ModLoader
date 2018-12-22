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
    public class CPToolsMod : MonoBehaviour, ICommandHandler
    {
        ModLoader.ModLoader loader;

        public static CPToolsMod Instance { get; private set; }

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
            if (GetLocalHuman() != null && GetLocalHuman().player.host.isLocal)
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

        public HashSet<string> CommandNames => new HashSet<string>() { "cp-state", "cp-set", "cp-toggle", "cp-reset" };

        public void OnCommandTyped(string cmd, string[] args)
        {
            Human hl = GetLocalHuman();
            if (hl == null)
            {
                loader.LogLine("[cp] Could not get Human object");
                return;
            }
            else if (!hl.player.host.isLocal)
            {
                loader.LogLine("[cp] Not the server host, may not run cp commands");
                return;
            }

            switch (cmd)
            {
                case "cp-toggle":
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

                case "cp-state":
                    if (args.Length == 0 || !bool.TryParse(args[0], out bool state))
                    {
                        loader.LogLine("[cp-set] Invalid boolean input");
                        return;
                    }
                    Human[] toSet = SelectHumans((args.Length < 2) ? "all" : args[1]).ToArray();
                    if (toSet.Length == 0)
                    {
                        loader.LogLine("[cp-set] Cannot find player given");
                        return;
                    }
                    foreach (var h in toSet)
                    {
                        var spndata = h.GetComponent<SpawnpointData>();
                        if (spndata == null)
                        {
                            h.gameObject.AddComponent<SpawnpointData>();
                            spndata = h.GetComponent<SpawnpointData>();
                        }
                        spndata.enabled = state;
                    }

                    loader.LogLine(string.Format("[cp-reset] {0} custom checkpoints for target", state? "Enabled" : "Disabled"));
                    break;

                case "cp-set":
                    toSet = SelectHumans((args.Length == 0) ? "" : args[0]).ToArray();
                    if (toSet.Length == 0)
                    {
                        loader.LogLine("[cp-set] Cannot find player given");
                        return;
                    }

                    foreach (var h in toSet)
                    {
                        var spndata = h.GetComponent<SpawnpointData>();
                        if (spndata == null)
                        {
                            h.gameObject.AddComponent<SpawnpointData>();
                            spndata = h.GetComponent<SpawnpointData>();
                        }
                        spndata.customSpawn = true;
                        spndata.spawnpoint = hl.transform.position;
                    }

                    loader.LogLine("[cp-set] Set spawnpoint to " + hl.transform.position.ToString());
                    break;

                case "cp-reset":
                    toSet = SelectHumans((args.Length == 0) ? "" : args[0]).ToArray();
                    if (toSet.Length == 0)
                    {
                        loader.LogLine("[cp-reset] Cannot find player given");
                        return;
                    }

                    foreach (var h in toSet)
                    {
                        var spndata = h.GetComponent<SpawnpointData>();
                        if (spndata == null)
                        {
                            h.gameObject.AddComponent<SpawnpointData>();
                            spndata = h.GetComponent<SpawnpointData>();
                        }
                        spndata.customSpawn = false;
                    }

                    loader.LogLine("[cp-reset] Removed custom spawn ");
                    break;
            }
        }

        IEnumerable<Human> SelectHumans(string selector)
        {
            if (selector == "")
            {
                yield return GetLocalHuman();
            }
            else if (selector.ToLower() == "all")
            {
                foreach (Human h in Human.all) { yield return h; };
            }
            else
            {
                foreach (Human h in Human.all)
                {
                    if (h.player.name == selector) { yield return h; }
                }
            }
        }

        public string ProvideHelp(string cmd)
        {
            switch (cmd)
            {
                case "cp-set":
                    return "Usage: cp-set (<player>|all)\nSets the checkpoint of the current (or given or all players) to the given position.";
                case "cp-state":
                    return "Usage: cp-state [true|false] (player)\nDisables or enables custom checkpoints for some or all players.";
                case "cp-toggle":
                    return "Usage: cp-toggle (true|false)\nToggles auto checkpoint trigger boxes";
            }

            return "";
        }
    }
}
