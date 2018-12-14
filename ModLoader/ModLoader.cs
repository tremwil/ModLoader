using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ModLoader
{
    [AttributeUsage(AttributeTargets.All)]
    public class ModEntryPointAttribute : Attribute
    {
        public ModEntryPointAttribute()
        {

        }
    }

    public interface ICommandHandler
    {
        void OnCommandTyped(string cmd, string[] args);

        string ProvideHelp(string cmd);

        HashSet<string> CommandNames { get; }
    }

    class KeyBinding
    {
        public string key;
        public string[] modifiers;

        public string command;
    }

    public sealed class ModLoader : MonoBehaviour, ICommandHandler
    {
        private Dictionary<string, Assembly> modDlls;
        private Dictionary<string, ICommandHandler> handlers;
        private static Dictionary<string, KeyBinding> keybinds;

        private bool showConsole = true;
        private bool focusNextGUI = false;
        private string logText = "";
        private string typedCmd = "";

        private const bool DEBUG = false;

        private Vector2 scrollPos = new Vector2(0, 0);
        private Rect windowRect = new Rect(5, 5, Screen.width * 0.5f, Screen.height * 0.4f);

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public HashSet<string> CommandNames => new HashSet<string>()
        {
            "help", "listmods", "clear", "bind", "unbind"
        };

        void Awake()
        {
            LogLine("SFTG Mod Loader V1.0\n");

            Application.logMessageReceived += Application_logMessageReceived;

            modDlls = new Dictionary<string, Assembly>();
            keybinds = keybinds ?? new Dictionary<string, KeyBinding>();
            handlers = new Dictionary<string, ICommandHandler> { ["ModLoader"] = this };
            HashSet<string> uniqueCmds = new HashSet<string>(CommandNames);

            Log("Loading mods... ");
            DirectoryInfo[] dirs = Directory.GetParent(AssemblyDirectory).GetDirectories().Where(x => x.Name.ToUpper() == "MODS").ToArray();

            if (dirs.Length == 1)
            {
                FileInfo[] modfiles = dirs[0].GetFiles("*.dll");
                LogLine($"found {modfiles.Length} mods in {dirs[0].FullName}");

                foreach (FileInfo dll in modfiles)
                {
                    string name = Path.GetFileNameWithoutExtension(dll.Name), entryPtName = "";
                    modDlls[name] = Assembly.LoadFrom(dll.FullName);
                    Log($"\nLoading mod {name}... ");

                    foreach (Type t in modDlls[name].GetTypes())
                    {
                        object[] attrs = t.GetCustomAttributes(typeof(ModEntryPointAttribute), false);
                        if (t.IsSubclassOf(typeof(MonoBehaviour)) && attrs.Length != 0)
                        {
                            if (entryPtName != "")
                            {
                                LogLine($"[Warning] There is more than one ModEntryPoint in this assembly.\nOnly '{entryPtName}' was loaded.");
                                break;
                            }

                            gameObject.AddComponent(t);
                            Log("success\nChecking for command support... ");

                            entryPtName = t.Name;

                            if (gameObject.GetComponent(t) is ICommandHandler handler)
                            {
                                handlers[name] = handler;
                                LogLine("affirmative");

                                int prvSz = uniqueCmds.Count;
                                uniqueCmds.UnionWith(handler.CommandNames);
                                if (prvSz + handler.CommandNames.Count != uniqueCmds.Count)
                                {
                                    LogLine($"[Warning] Some commands were already defined. The old definition will prevail.");
                                }
                            }
                            else
                            {
                                LogLine("negative");
                            }
                        }
                    }
                    if (entryPtName == "")
                    {
                        LogLine("could not find ModEntryPoint");
                    }
                }
            }
            else
            {
                LogLine("no 'Mods' folder found");
            }

            LogLine("\nType 'help' for help on commands.\n");
        }

        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (!DEBUG) { return; }

            if (type == LogType.Error || type == LogType.Exception)
            {
                LogLine(condition + ": " + stackTrace);
            }
        }

        bool checkBindStr(string btn, Func<KeyCode, bool> keyboard, Func<int, bool> mouse)
        {
            if (int.TryParse(btn, out int mbtn))
            {
                return mouse(mbtn);
            }
            else if (Enum.IsDefined(typeof(KeyCode), btn))
            {
                return keyboard((KeyCode)Enum.Parse(typeof(KeyCode), btn));
            }
            return false;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Semicolon))
            {
                showConsole = !showConsole;
                focusNextGUI = showConsole;
            }

            foreach (KeyBinding keybind in keybinds.Values)
            {
                if (!checkBindStr(keybind.key, Input.GetKeyDown, Input.GetMouseButtonDown)) { continue; }

                bool passed = true;
                foreach (string modifier in keybind.modifiers)
                {
                    if (!checkBindStr(modifier, Input.GetKey, Input.GetMouseButton))
                    {
                        passed = false;
                        break;
                    }
                }
                if (passed) { RunCommand(keybind.command); }
            }
        }

        public void Log(string text)
        {
            logText += text;
            scrollPos = new Vector2(0, float.PositiveInfinity);
        }

        public void LogLine(string text)
        {
            logText += text + "\n";
            scrollPos = new Vector2(0, float.PositiveInfinity);
        }

        void OnGUI()
        {
            if (showConsole)
            {
                GUI.color = Color.red;

                windowRect = GUI.Window(0, windowRect, (int id) =>
                {
                    GUI.color = Color.red;

                    scrollPos = GUILayout.BeginScrollView(scrollPos);
                    GUILayout.Label(logText);
                    GUILayout.EndScrollView();

                    if (Event.current.type == EventType.KeyDown)
                    {
                        if (Event.current.keyCode == KeyCode.Return)
                        {
                            LogLine($">>> {typedCmd}");
                            RunCommand(typedCmd);
                            typedCmd = "";
                            Event.current.Use();
                        }
                        if (Event.current.keyCode == KeyCode.Semicolon)
                        {
                            showConsole = false;
                            typedCmd = "";
                            Event.current.Use();
                        }
                    }

                    GUI.SetNextControlName("cmdinput");
                    typedCmd = GUILayout.TextField(typedCmd);

                    if (focusNextGUI)
                    {
                        GUI.FocusControl("cmdinput");
                        focusNextGUI = false;
                    }

                    GUI.DragWindow();
                }, "SFTG Mod Loader V1.0");
            }
        }

        public void RunCommand(string cmd)
        {
            string[] splitted = cmd.Trim(' ').Split(' ');
            string cmdStr = splitted[0];

            bool found = false;

            foreach (ICommandHandler handler in handlers.Values)
            {
                if (handler.CommandNames.Contains(cmdStr))
                {
                    handler.OnCommandTyped(cmdStr, splitted.Skip(1).ToArray());
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                LogLine("[Error] Unknown command");
            }
        }

        public void OnCommandTyped(string cmd, string[] args)
        {
            switch (cmd)
            {
                case "help":
                    if (args.Length == 0)
                    {
                        foreach (string modName in handlers.Keys)
                        {
                            LogLine($"{modName}:");
                            foreach (string c in handlers[modName].CommandNames)
                            {
                                LogLine($"    {c}");
                            }
                        }
                    }
                    else
                    {
                        foreach (string modName in handlers.Keys)
                        {
                            string help = handlers[modName].ProvideHelp(args[0]);
                            if (help != "")
                            {
                                LogLine(help);
                                return;
                            }
                        }
                        LogLine($"Command {args[0]} not found.");
                    }
                    break;
                case "listmods":
                    LogLine($"{modDlls.Count} mods active:");
                    foreach (string modName in modDlls.Keys)
                    {
                        LogLine($"    {modName}");
                    }
                    break;
                case "clear":
                    logText = "";
                    break;
                case "bind":
                    if (args.Length == 0)
                    {
                        LogLine($"There are {keybinds.Count} current bindings:");
                        foreach (string kbname in keybinds.Keys)
                        {
                            KeyBinding keybind = keybinds[kbname];
                            string chr = (keybind.modifiers.Length == 0) ? "" : "+";
                            LogLine($"{kbname} ({keybind.key}{chr + string.Join("+", keybind.modifiers)}) -> {keybind.command}");
                        }
                        break;
                    }
                    else if (args.Length < 3)
                    {
                        LogLine("[bind] Invalid arguments");
                        break;
                    }

                    string name = args[0];
                    string[] keymod = args[1].Split('+');

                    KeyBinding binding = new KeyBinding();
                    binding.key = keymod[0];
                    binding.modifiers = keymod.Skip(1).ToArray();
                    binding.command = string.Join(" ", args.Skip(2).ToArray());
                    keybinds[name] = binding;
                    LogLine($"[bind] Successfully created binding {name}");
                    break;
                case "unbind":
                    if (args.Length < 1)
                    {
                        LogLine("[unbind] Invalid arguments");
                        break;
                    }
                    keybinds.Remove(args[0]);
                    LogLine($"[umbind] Successfully removed binding {args[0]}");
                    break;
            }
        }

        public string ProvideHelp(string cmd)
        {
            switch (cmd)
            {
                case "help":
                    return "Usage: help [command]\nReturns an help message for a specific command or lists them all.";
                case "listmods":
                    return "Usage: listmods\nLists all active mods.";
                case "clear":
                    return "Usage: clear\nClears the console completely.";
                case "bind":
                    return "Usage: bind [name] [key+modifier1+modifier2] [command]\nBind a key or a mouse button to a command.";
                case "unbind":
                    return "Usage: unbind [name]\nUnbind a previous key binding.";
                default:
                    return "";
            }
        }
    }
}
