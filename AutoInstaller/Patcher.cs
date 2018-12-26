using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;
using UnityEngine;

namespace AutoInstaller
{
    public static class Patcher
    {
        public static void InjectMLLoader(string asmPath, string outAsmPath)
        {
            AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(asmPath);
            asm.MainModule.ModuleReferences.Add(new ModuleReference("ModLoader.dll"));

            TypeDefinition moduleType = asm.MainModule.Types.First(x => x.Name == "<Module>");
            MethodDefinition[] toDel = moduleType.Methods.Where(x => x.Name == ".cctor").ToArray();
            foreach (var method in toDel) { moduleType.Methods.Remove(method); }
            if (toDel.Length > 0) { Console.WriteLine("[Patcher] Remove previous module constructor"); }

            TypeReference voidRef = asm.MainModule.ImportReference(typeof(void));
            MethodAttributes cctorAttrs = MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            MethodDefinition moduleCtor = new MethodDefinition(".cctor", cctorAttrs, voidRef);
            Console.WriteLine("[Patcher] Create static method [Assembly-CSharp]<Module>.cctor()");

            MethodReference initMethod = asm.MainModule.ImportReference(typeof(ModLoader.ModuleAutoInit).GetMethod("InitModLoader"));
            moduleCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, initMethod));
            moduleCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            Console.WriteLine("[IL] <Module>.cctor() -> call class [ModLoader]ModLoader.ModuleAutoInit::InitModLoader");
            Console.WriteLine("[IL] <Module>.cctor() -> ret");

            moduleType.Methods.Add(moduleCtor);

            asm.Write(outAsmPath);
            asm.Dispose();
        }
    }
}
