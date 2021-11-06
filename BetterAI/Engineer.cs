using HarmonyLib;
using UnityEngine;

namespace BetterAI
{
    internal class Engineer
    {
        [HarmonyPatch(typeof(PLBot), "TickOptimizeStationAction")]
        class EngineScreens 
        {
            static void Postfix(PLBot __instance) 
            {
                Vector3 down = new Vector3(0, 1, 0);
                PLUIScreen enginescreen = null;
                if (__instance.PlayerOwner.StartingShip == null || __instance.PlayerOwner.GetPawn() == null || __instance.PlayerOwner.GetClassID() != 4) return;
                foreach (PLUIScreen screen in __instance.PlayerOwner.StartingShip.MyScreenBase.AllScreens)
                {
                    if (screen.name.ToLower().Contains("cloned") && (screen.name.ToLower().Contains("reactor") || screen.name.ToLower().Contains("engineering")))
                    {
                        enginescreen = screen;
                        break;
                    }
                }
                if (enginescreen != null)
                {
                    __instance.AI_TargetPos = enginescreen.transform.position + enginescreen.transform.forward - down;
                    __instance.AI_TargetPos_Raw = __instance.AI_TargetPos;
                    __instance.EnablePathing = true;
                }
                if (__instance.PlayerOwner.ActiveSubPriority != null) 
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
