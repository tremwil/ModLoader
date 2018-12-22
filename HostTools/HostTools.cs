using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModLoader;
using UnityEngine;
using System.Reflection;
using HumanAPI;

namespace HostTools
{
    [ModEntryPoint]
    public class HostToolsMod : MonoBehaviour, ICommandHandler
    {
        ModLoader.ModLoader loader;

        bool fly = false;
        float flySpeed = 10f;
        float flyMaxAcc = 15f;
        Vector3 flyVelTgt = Vector3.zero;

        public void Start()
        {
            loader = gameObject.GetComponent<ModLoader.ModLoader>();
        }

        public void FixedUpdate()
        {
            Human h = GetLocalHuman();
            if (fly && h != null)
            {
                flyVelTgt = (h.controls.walkSpeed > 0) ? h.controls.walkDirection : Vector3.zero;
                if (Input.GetKey(KeyCode.Space)) { flyVelTgt += Vector3.up; }
                if (Input.GetKey(KeyCode.LeftShift)) { flyVelTgt += Vector3.down; }
                flyVelTgt *= flySpeed;

                Vector3 flyVelCurr = h.ragdoll.partChest.rigidbody.velocity;
                Vector3 impulse = Vector3.MoveTowards(Vector3.zero, flyVelTgt - flyVelCurr, flyMaxAcc * Time.fixedDeltaTime);

                foreach (Rigidbody r in h.rigidbodies)
                {
                    r.SafeAddForce(impulse, ForceMode.VelocityChange);
                }

                HumanSegment[] hands = new HumanSegment[2] { h.ragdoll.partLeftHand, h.ragdoll.partRightHand };
                Rigidbody bodyPrv = null;

                foreach (HumanSegment hand in hands)
                {
                    if (hand.sensor.grab && hand.sensor.grabBody && !hand.sensor.grabBody.isKinematic && bodyPrv != hand.sensor.grabBody)
                    {
                        hand.sensor.grabBody.SafeAddForceAtPosition(impulse, hand.transform.position, ForceMode.VelocityChange);
                        bodyPrv = hand.sensor.grabBody;
                    }
                }
            }
        }

        Human GetLocalHuman()
        {
            foreach (Human h in Human.all)
            {
                if (h.player.isLocalPlayer) { return h; }
            }
            return null;
        }

        public HashSet<string> CommandNames => new HashSet<string>() { "ht-grav", "ht-fly", "ht-flyspd", "ht-flyacc" };

        public void OnCommandTyped(string cmd, string[] args)
        {
            Human hl = GetLocalHuman();
            if (hl == null)
            {
                loader.LogLine("[ht] Could not get local Human object");
                return;
            }
            else if (!hl.player.host.isLocal)
            {
                loader.LogLine("[ht] Not the server host, may not run ht commands");
                return;
            }

            switch (cmd)
            {
                case "ht-grav":
                    Vector3 tgtGrav = new Vector3(0, -9.8f, 0); 
                    if (args.Length == 1 && float.TryParse(args[0].ToLower(), out float yVal))
                    {
                        tgtGrav = new Vector3(0, yVal, 0);
                    }
                    else if (args.Length == 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (!float.TryParse(args[i].ToLower(), out float component))
                            {
                                loader.LogLine("[ht-grav] Invalid input");
                                return;
                            }
                            tgtGrav[i] = component;
                        }
                    }
                    else if (args.Length != 0)
                    {
                        loader.LogLine("[ht-grav] Invalid input");
                        return;
                    }
                    Physics.gravity = tgtGrav;
                    loader.LogLine(string.Format("[ht-grav] Set gravity to ({0}, {1}, {2})", tgtGrav.x, tgtGrav.y, tgtGrav.z));
                    break;

                case "ht-fly":
                    if (args.Length == 0)
                    {
                        fly = !fly;
                    }
                    else if (!bool.TryParse(args[0].ToLower(), out fly))
                    {
                        loader.LogLine("[ht-fly] Invalid boolean");
                        return;
                    }
                    foreach (Rigidbody r in hl.rigidbodies)
                    {
                        r.useGravity = !fly;
                    }
                    loader.LogLine("[ht-fly] Set fly mode to " + fly.ToString());
                    break;

                case "ht-flyspd":
                    if (args.Length > 0)
                    {
                        if (!float.TryParse(args[0].ToLower(), out flySpeed))
                        {
                            loader.LogLine("[ht-flyspd] Invalid float");
                            return;
                        }
                    }
                    else
                    {
                        flySpeed = 10f;
                    }
                    loader.LogLine("[ht-flyspd] Set fly speed to " + flySpeed.ToString());
                    break;

                case "ht-flyacc":
                    if (args.Length > 0)
                    {
                        if (!float.TryParse(args[0].ToLower(), out flyMaxAcc))
                        {
                            loader.LogLine("[ht-flyacc] Invalid float");
                            return;
                        }
                    }
                    else
                    {
                        flyMaxAcc = 15f;
                    }
                    loader.LogLine("[ht-flyacc] Set fly acc. to " + flyMaxAcc.ToString());
                    break;
            }
        }

        public string ProvideHelp(string cmd)
        {
            return "";
        }
    }
}
