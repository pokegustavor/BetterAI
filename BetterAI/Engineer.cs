using HarmonyLib;
using UnityEngine;

namespace BetterAI
{
    internal class Engineer
    {
        [HarmonyPatch(typeof(PLServer), "ServerAddCrewBotPlayer")]
        class SetSpawner 
        {
            static void Postfix() 
            {
                PLShipInfo ship = PLEncounterManager.Instance.PlayerShip;
                if(ship != null) 
                {
                    GameObject spawner = ship.Spawners[4] as GameObject;
                    PLUIScreen enginescreen = null;
                    foreach(PLUIScreen screen in ship.MyScreenBase.AllScreens) 
                    {
                        if (screen.name.ToLower().Contains("cloned") && (screen.name.ToLower().Contains("reactor") || screen.name.ToLower().Contains("engineering"))) 
                        {
                            enginescreen = screen;
                            break;
                        }
                    }
                    if(enginescreen != null && spawner != null) 
                    {
                        Vector3 down = new Vector3(0, 1, 0);
                        spawner.transform.position = enginescreen.transform.position + enginescreen.transform.forward - down;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(PLBot), "TickOptimizeStationAction")]
        class WarpScreen 
        {
            static void Postfix(PLBot __instance) 
            {
                if(__instance.PlayerOwner.ActiveSubPriority != null) 
                {
                    EAIPriorityListDisplayed typeData = (EAIPriorityListDisplayed)__instance.PlayerOwner.ActiveSubPriority.TypeData;
                    if (typeData == EAIPriorityListDisplayed.ENG_JUMP_SHIP || typeData == EAIPriorityListDisplayed.ENG_CHARGE_WARP_DRIVE) 
                    {
                        PLUIScreen warpscreen = null;
                        foreach (PLUIScreen screen in __instance.PlayerOwner.StartingShip.MyScreenBase.AllScreens)
                        {
                            if (screen.name.ToLower().Contains("cloned") && (screen.name.ToLower().Contains("warp") || screen.name.ToLower().Contains("jump")))
                            {
                                warpscreen = screen;
                                break;
                            }
                        }
                        if (warpscreen != null)
                        {
                            Vector3 down = new Vector3(0, 1, 0);
                            __instance.AI_TargetPos = warpscreen.transform.position + warpscreen.transform.forward - down;
                            __instance.AI_TargetPos_Raw = __instance.AI_TargetPos;
                            __instance.EnablePathing = true;
                        }
                    }
                }
            }
        }
    }
}
