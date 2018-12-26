using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModLoader
{
    public static class ModuleAutoInit
    {
        public static bool HasRan { get; private set; } = false;
        public static GameObject GameObj { get; private set; }

        public static void InitModLoader()
        {
            if (!HasRan)
            {
                GameObj = new GameObject("ModLoader");
                UnityEngine.Object.DontDestroyOnLoad(GameObj);
                GameObj.AddComponent<ModLoader>();
            }
            HasRan = true;
        }
    }
}
