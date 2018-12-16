using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using UnityEngine;

namespace CPTools
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class StaticConstructorOnStartup : Attribute
    {
    }

    [StaticConstructorOnStartup]
    static class PatchManager
    {
        static PatchManager()
        {

        }
    }

    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch("Respawn", typeof(Human), typeof(Vector3))]
    class RespawnPatch
    {
        static void Postfix(Human human, Vector3 offset)
        {
            Debug.Log("Respawn called");

            var spndata = human.GetComponent<SpawnpointData>();
            if (spndata == null || !spndata.enabled) { return; }

            human.SpawnAt(spndata.spawnpoint + Vector3.up);
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
                if (spndata != null) { spndata.enabled = false; }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Human))]
    [HarmonyPatch("SpawnAt", typeof(Vector3))]
    class HumanSpawnPatch
    {
        static bool Prefix(Human __instance, Vector3 pos)
        {
            var spndata = __instance.GetComponent<SpawnpointData>();
            if (spndata == null || !spndata.enabled) { return true; }

            __instance.state = HumanState.Spawning;
            Vector3 a = __instance.KillHorizontalVelocity();
            Vector3 position = pos;

            __instance.SetPosition(position);
            if (a.magnitude < 5f)
            {
                __instance.AddRandomTorque(1f);
            }
            __instance.Reset();

            return false;
        }
    }
}
