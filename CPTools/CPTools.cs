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
        bool checkpointsAuto = false;

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
                checkpointsOn = false;
                checkpointsAuto = false;
            }
            lvlLast = Game.instance.currentLevelNumber;

            if (checkpointsAuto)
            {
                if (Human.instance == null) { return; }

                if (Human.instance.onGround && !onGround)
                {
                    onGround = true;
                    lastOnGround = Time.time;
                }
                onGround = Human.instance.onGround;

                if (onGround && Time.time - lastOnGround > timeAutosave)
                {
                    Game.currentLevel.checkpoints[0].position = Human.instance.transform.position + 50 * Vector3.up;
                    lastOnGround = Time.time;
                }
            }
        }

        public HashSet<string> CommandNames => new HashSet<string>() { "cp-toggle", "cp-auto" };

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
                        checkpointsAuto = false;
                        Game.currentLevel.checkpoints[0].position = prvCheckpt;
                    }

                    Checkpoint[] cps = Game.FindObjectsOfType<Checkpoint>();
                    foreach (Checkpoint cp in cps)
                    {
                        cp.enabled = checkpointsOn;
                    }
                    break;

                case "cp-auto":
                    Game.instance.currentCheckpointNumber = 0;
                    Game.instance.currentSolvedCheckpoints.Clear();

                    checkpointsAuto = !checkpointsAuto;
                    if (checkpointsAuto)
                    {
                        checkpointsOn = false;
                        prvCheckpt = Game.currentLevel.checkpoints[0].position;
                    }
                    else
                    {
                        checkpointsOn = true;
                        Game.currentLevel.checkpoints[0].position = prvCheckpt;
                    }

                    cps = Game.FindObjectsOfType<Checkpoint>();
                    foreach (Checkpoint cp in cps)
                    {
                        cp.enabled = !checkpointsAuto;
                    }
                    break;
            }
        }

        public string ProvideHelp(string cmd)
        {
            return "";
        }
    }
}
