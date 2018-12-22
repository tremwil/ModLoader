using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using UnityEngine;
using HumanAPI;

namespace CPTools
{
    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch("Respawn", typeof(Human), typeof(Vector3))]
    class RespawnPatch
    {
        static void Postfix(Human human, Vector3 offset)
        {
            var spndata = human.GetComponent<SpawnpointData>();
            if (spndata == null || !spndata.enabled) { return; }

            human.SpawnAt(spndata.spawnpoint);
        }
    }

    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch("RestartLevel", typeof(bool))]
    class RestartLvlPatch
    {
        static bool Prefix(bool reset)
        {
            foreach (Human h in Human.all)
            {
                var spndata = h.GetComponent<SpawnpointData>();
                if (spndata != null)
                {
                    spndata.spawnpoint = Game.currentLevel.checkpoints[0].position;
                    spndata.cpNumber = 0;
                }
            }

            return true;
        }
    }

    //[HarmonyPatch(typeof(Human))]
    //[HarmonyPatch("SpawnAt", typeof(Vector3))]
    //class HumanSpawnPatch
    //{
    //    static bool Prefix(Human __instance, Vector3 pos)
    //    {
    //        var spndata = __instance.GetComponent<SpawnpointData>();
    //        if (spndata == null || !spndata.enabled) { return true; }

    //        __instance.state = HumanState.Spawning;
    //        Vector3 a = __instance.KillHorizontalVelocity();
    //        Vector3 position = pos;

    //        __instance.SetPosition(position);
    //        if (a.magnitude < 5f)
    //        {
    //            __instance.AddRandomTorque(1f);
    //        }
    //        __instance.Reset();

    //        return false;
    //    }
    //}

    [HarmonyPatch(typeof(Checkpoint))]
    [HarmonyPatch("OnTriggerEnter", typeof(Collider))]
    class CheckpointTriggerPatch
    {
        static void Postfix(Checkpoint __instance, Collider other)
        {
            if (!__instance.enabled)
            {
                return;
            }
            if (other.tag != "Player")
            {
                return;
            }
            Human h = other.GetComponent<Human>();
            SpawnpointData spndata = h.GetComponent<SpawnpointData>();
            if (spndata != null && spndata.enabled && spndata.cpNumber < __instance.number && !spndata.customSpawn)
            {
                spndata.spawnpoint = Game.currentLevel.checkpoints[__instance.number].position;
                spndata.cpNumber = __instance.number;
            }
        }
    }
}
