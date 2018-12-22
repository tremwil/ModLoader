using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModLoader;
using UnityEngine;

namespace CodeExec
{
    [ModEntryPoint]
    public class CodeExec : MonoBehaviour, ICommandHandler
    {


        public HashSet<string> CommandNames => throw new NotImplementedException();

        public void OnCommandTyped(string cmd, string[] args)
        {
            throw new NotImplementedException();
        }

        public string ProvideHelp(string cmd)
        {
            throw new NotImplementedException();
        }
    }
}
