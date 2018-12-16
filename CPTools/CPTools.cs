using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModLoader;
using UnityEngine;
using HumanAPI;

namespace CPTools
{
    [ModEntryPoint]
    public class CPTools : MonoBehaviour, ICommandHandler
    {
        ModLoader.ModLoader loader;

        bool checkpointsOn = true;

        int lvlLast = 0;
        Vector3 prvCheckpt;

        float timeAutosave = 5;
        float lastOnGround;
        bool onGround;

        public void Start()
        {
            loader = gameObject.GetComponent<ModLoader.ModLoader>();
        }


        public void Update()
        {
            if (lvlLast != Game.instance.currentLevelNumber)
            {
                checkpointsOn = true;
            }
            lvlLast = Game.instance.currentLevelNumber;
        }

        public HashSet<string> CommandNames => new HashSet<string>() { "cp-toggle", "cp-set" };

        public void OnCommandTyped(string cmd, string[] args)
        {
            switch (cmd)
            {
                case "cp-toggle":
                    Game.instance.currentCheckpointNumber = 0;
                    Game.instance.currentSolvedCheckpoints.Clear();

                    checkpointsOn = !checkpointsOn;
                    loader.LogLine("Checkpoints turned " + (checkpointsOn ? "On" : "Off"));
                    if (checkpointsOn)
                    {
                        Game.currentLevel.checkpoints[0].position = prvCheckpt;
                    }

                    Checkpoint[] cps = Game.FindObjectsOfType<Checkpoint>();
                    foreach (Checkpoint cp in cps)
                    {
                        cp.enabled = checkpointsOn;
                    }
                    break;

                case "cp-set":
                    Game.instance.currentCheckpointNumber = 0;
                    Game.instance.currentSolvedCheckpoints.Clear();

                    break;
            }
        }

        public string ProvideHelp(string cmd)
        {
            return "";
        }
    }
}
