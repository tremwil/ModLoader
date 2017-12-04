using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CodeSnippet
{
    public class Snippet : MonoBehaviour
    {
        public void SomeCode()
        {
            gameObject.AddComponent(typeof(ModLoader.ModLoader));
        }
    }
}
