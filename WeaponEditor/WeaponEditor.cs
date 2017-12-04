using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using ModLoader;

namespace WeaponEditor
{
    [ModEntryPoint]
    public class WeaponEditor : MonoBehaviour, ICommandHandler
    {
        public HashSet<string> CommandNames => new HashSet<string>()
        {
            "weaponedit"
        };

        int selectedWeap = 0;
        int selectedProp = -1;
        string editingProp;

        bool showEditor = false;
        bool handleClicked;
        Rect windowRect = new Rect(Screen.width * 0.75f - 5, 5, Screen.width * 0.25f, Screen.height * 0.7f);
        Rect origWindowRect;
        Rect windowHandle;
        Vector3 clickedPos;
        Vector2 scrollPos1 = new Vector2(0, 0);
        Vector2 scrollPos2 = new Vector2(0, 0);

        float minWindowWidth = Screen.width * 0.25f,
              minWindowHeight = Screen.height * 0.4f,
              maxWindowWidth = Screen.width * 0.5f,
              maxWindowHeight = Screen.height * 0.95f;

        ModLoader.ModLoader loader;
        GameManager manager;
        MultiplayerManager networkManager;

        Weapons weapons;
        string[] weaponNames;

        Dictionary<int, Dictionary<string, object>> originalProps;

        void Start()
        {
            loader = gameObject.GetComponent<ModLoader.ModLoader>();
            manager = gameObject.GetComponent<GameManager>();
            networkManager = FindObjectOfType<MultiplayerManager>();

            weaponNames = Enum.GetNames(typeof(WeaponId));
            originalProps = new Dictionary<int, Dictionary<string, object>>();

            loader.RunCommand("bind weaponedit:open E+LeftControl weaponedit");
            loader.LogLine("[WeaponEditor] Weapon editor will load upon joining a game.");
        }

        void Update()
        {
            if (weapons == null)
            {
                if (GetCurrentPlayer() != null)
                {
                    weapons = (Weapons)typeof(Fighting).GetField("weapons", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(GetCurrentPlayer().GetComponent<Fighting>());

                    if (weapons != null)
                    {
                        CopyOriginalWeaponData();
                        loader.LogLine($"[WeaponEditor] Weapons are loaded!");
                    }
                }
            }
        }

        void CopyOriginalWeaponData()
        {
            if (weapons != null)
            {
                foreach (object enumId in Enum.GetValues(typeof(WeaponId)))
                {
                    int id = (int)enumId;
                    originalProps[id] = new Dictionary<string, object>();
                    Weapon weapon = weapons.transform.GetChild(id - 1).GetComponent<Weapon>();

                    foreach (FieldInfo field in typeof(Weapon).GetFields(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (IsNumericType(field.FieldType) || Type.GetTypeCode(field.FieldType) == TypeCode.Boolean)
                        {
                            originalProps[id][field.Name] = field.GetValue(weapon);
                        }
                    }
                }
            }
        }

        void ResetWeapon(int id)
        {
            Weapon weapon = weapons.transform.GetChild(id - 1).GetComponent<Weapon>();

            foreach (FieldInfo field in typeof(Weapon).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (originalProps[id].ContainsKey(field.Name))
                {
                    field.SetValue(weapon, originalProps[id][field.Name]);
                }
            }
        }

        Controller[] ActiveControllers()
        {
            return networkManager.PlayerControllers.Where(x => x != null).ToArray();
        }

        Controller GetCurrentPlayer()
        {
            foreach (Controller player in ActiveControllers())
            {
                if (player.HasControl)
                {
                    return player;
                }
            }

            return null;
        }

        void OnGUI()
        {
            if (showEditor)
            {
                var mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;
                windowHandle = new Rect(windowRect.x + windowRect.width - 25, windowRect.y + windowRect.height - 25, 25, 25);

                if (Input.GetMouseButtonDown(0) && windowHandle.Contains(mousePos)) {
                    handleClicked = true;
                    clickedPos = mousePos;
                    origWindowRect = windowRect;
                }

                if (handleClicked)
                {
                    if (Input.GetMouseButton(0))
                    {
                        windowRect.width = Mathf.Clamp(origWindowRect.width + (mousePos.x - clickedPos.x), minWindowWidth, maxWindowWidth);
                        windowRect.height = Mathf.Clamp(origWindowRect.height + (mousePos.y - clickedPos.y), minWindowHeight, maxWindowHeight);
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        handleClicked = false;
                    }
                }

                GUI.color = Color.red;
                windowRect = GUI.Window(1, windowRect, (int id) =>
                {
                    GUI.color = Color.red;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Weapon to edit");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("RESET ALL"))
                    {
                        foreach (object oId in Enum.GetValues(typeof(WeaponId)))
                        {
                            ResetWeapon((int)oId);
                        }
                    }
                    GUILayout.EndHorizontal();

                    scrollPos1 = GUILayout.BeginScrollView(scrollPos1, GUILayout.Height(150));
                    selectedWeap = GUILayout.SelectionGrid(selectedWeap, weaponNames, 1);
                    GUILayout.EndScrollView();

                    int weapId = (int)Enum.Parse(typeof(WeaponId), weaponNames[selectedWeap]);

                    GUILayout.Space(10);
                    if (GUILayout.Button("Spawn this weapon!"))
                    {
                        GetCurrentPlayer().GetComponent<NetworkPlayer>().ThrowWeapon(false, (byte)weapId, 
                            GetCurrentPlayer().transform.GetComponentInChildren<Torso>().transform.position, Vector3.up, Vector3.zero);
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Properties:");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("RESET"))
                    {
                        ResetWeapon(weapId);
                    }
                    GUILayout.EndHorizontal();

                    if (weapons != null)
                    {
                        scrollPos2 = GUILayout.BeginScrollView(scrollPos2);
                        Weapon weapon = weapons.transform.GetChild(weapId - 1).GetComponent<Weapon>();

                        FieldInfo[] fields = typeof(Weapon).GetFields(BindingFlags.Public | BindingFlags.Instance);

                        for (int i = 0; i < fields.Length; i++)
                        {
                            FieldInfo field = fields[i];
                            if (!IsNumericType(field.FieldType) && Type.GetTypeCode(field.FieldType) != TypeCode.Boolean) { continue; }

                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"{field.Name} : {field.FieldType.Name}");
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("RESET"))
                            {
                                if (originalProps[weapId].ContainsKey(field.Name))
                                {
                                    field.SetValue(weapon, originalProps[weapId][field.Name]);
                                }
                            }
                            GUILayout.EndHorizontal();

                            if (Type.GetTypeCode(field.FieldType) == TypeCode.Boolean)
                            {
                                if (GUILayout.Button($"{field.GetValue(weapon)}"))
                                {
                                    field.SetValue(weapon, !(bool)field.GetValue(weapon));
                                }
                                continue;
                            }

                            if (selectedProp != i)
                            {
                                GUI.SetNextControlName($"{i}");
                                GUILayout.TextField($"{field.GetValue(weapon)}");

                                if (GUI.GetNameOfFocusedControl() == $"{i}")
                                {
                                    selectedProp = i;
                                    editingProp = field.GetValue(weapon).ToString();
                                }
                            }
                            else if (GUI.GetNameOfFocusedControl() == $"{selectedProp}")
                            {
                                GUI.color = (field.GetValue(weapon).ToString() == editingProp) ? Color.red : Color.white;

                                if (Event.current.type == EventType.KeyDown)
                                {
                                    char c = Event.current.character;
                                    if (!char.IsDigit(c) && c != '.' && c != '-' && c != 0)
                                    {
                                        Event.current.Use();
                                    }
                                    if (Event.current.keyCode == KeyCode.Return)
                                    {
                                        object val = ParseNumber(field.FieldType, editingProp);
                                        if (val != null) { field.SetValue(weapon, val); }
                                    }
                                }

                                GUI.SetNextControlName($"{i}");
                                editingProp = GUILayout.TextField(editingProp);

                                GUI.color = Color.red;
                            }
                            else
                            {
                                selectedProp = -1;

                                GUI.SetNextControlName($"{i}");
                                GUILayout.TextField($"{field.GetValue(weapon)}");
                            }
                        }

                        GUILayout.EndScrollView();
                    }
                    else
                    {
                        GUILayout.Label("Cannot load weapon data. Please host or join a game.");
                    }
                    if (!handleClicked) { GUI.DragWindow(); }
                }, "Weapon Editor V1.0");
            }
        }

        object ParseNumber(Type t, string value)
        {
            bool success;

            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                    success = byte.TryParse(value, out byte val0);
                    if (success) return val0;
                    break;
                case TypeCode.SByte:
                    success = sbyte.TryParse(value, out sbyte val1);
                    if (success) return val1;
                    break;
                case TypeCode.UInt16:
                    success = ushort.TryParse(value, out ushort val2);
                    if (success) return val2;
                    break;
                case TypeCode.UInt32:
                    success = uint.TryParse(value, out uint val3);
                    if (success) return val3;
                    break;
                case TypeCode.UInt64:
                    success = ulong.TryParse(value, out ulong val4);
                    if (success) return val4;
                    break;
                case TypeCode.Int16:
                    success = short.TryParse(value, out short val5);
                    if (success) return val5;
                    break;
                case TypeCode.Int32:
                    success = int.TryParse(value, out int val6);
                    if (success) return val6;
                    break;
                case TypeCode.Int64:
                    success = byte.TryParse(value, out byte val7);
                    if (success) return val7;
                    break;
                case TypeCode.Decimal:
                    success = decimal.TryParse(value, out decimal val8);
                    if (success) return val8;
                    break;
                case TypeCode.Double:
                    success = double.TryParse(value, out double val9);
                    if (success) return val9;
                    break;
                case TypeCode.Single:
                    success = float.TryParse(value, out float val10);
                    if (success) return val10;
                    break;
                default:
                    return null;
            }

            return null;
        }

        bool IsNumericType(Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public void OnCommandTyped(string cmd, string[] args)
        {
            switch (cmd)
            {
                case "weaponedit":
                    showEditor = !showEditor;
                    loader.LogLine("Turned weapon editor " + (showEditor ? "ON" : "OFF"));
                    break;
            }
        }

        public string ProvideHelp(string cmd)
        {
            switch (cmd)
            {
                case "weaponedit":
                    return "Usage: weaponedit\n Shows or masks the weapon editor.";
                default:
                    return "";
            }
        }
    }
}
