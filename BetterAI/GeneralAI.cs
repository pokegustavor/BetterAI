using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace BetterAI
{
    internal class GeneralAI
    {
        [HarmonyPatch(typeof(PLBot), "TickFindInvaderAction")]
        class Intruders
        {
            static void Postfix(PLBot __instance)
            {
                //This allows the sylvassi bot to use splitshots
                PLCombatTarget plcombatTarget = null;
                Transform nearestEnemyTargetTransform = __instance.GetNearestEnemyTargetTransform(ref plcombatTarget, 1f);
                if (nearestEnemyTargetTransform != null && plcombatTarget != null && __instance.PlayerOwner != null && __instance.PlayerOwner.GetPawn() != null && __instance.MyBotController != null && __instance.MyBotController.MyPawn != null && __instance.PlayerOwner.MyInventory != null && __instance.PlayerOwner.MyInventory.ActiveItem != null && __instance.PlayerOwner.RaceID == 1 && __instance.PlayerOwner.GetPawn().Cloaked)
                {
                    if ((__instance.PlayerOwner.MyInventory.ActiveItem is PLPawnItem_IceSpikes || __instance.PlayerOwner.MyInventory.ActiveItem is PLPawnItem_HandShotgun) && __instance.PlayerOwner.MyInventory.ActiveItem.AmmoCurrent > 0)
                    {
                        __instance.MyBotController.AI_ShouldUseActiveItem = true;
                    }
                }
            }
        }
    }
}
