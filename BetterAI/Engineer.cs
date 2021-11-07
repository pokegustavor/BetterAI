using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
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
                if((__instance.PlayerOwner.Talents[54] > 0 || __instance.PlayerOwner.Talents[55] > 0) && __instance.PlayerOwner.ScrapProcessingAttemptsLeft > 0 && __instance.PlayerOwner.StartingShip != null && __instance.PlayerOwner.StartingShip.HostileShips.Count == 0 && __instance.PlayerOwner.StartingShip.CoreInstability <= 0.001 && __instance.PlayerOwner.GetPawn() != null) 
                {
                    CargoObjectDisplay cargo = null;
                    List<PLShipComponent> componentsOfType = __instance.PlayerOwner.StartingShip.MyStats.GetComponentsOfType(ESlotType.E_COMP_CARGO, true);
                    componentsOfType.AddRange(__instance.PlayerOwner.StartingShip.MyStats.GetComponentsOfType(ESlotType.E_COMP_HIDDENCARGO, true));
                    float num32 = 5000f;
                    for (int num33 = 0; num33 < componentsOfType.Count; num33++)
                    {
                        PLShipComponent plshipComponent = componentsOfType[num33];
                        CargoObjectDisplay cargoObjectDisplay2;
                        if (plshipComponent.VisualSlotType == ESlotType.E_COMP_HIDDENCARGO)
                        {
                            cargoObjectDisplay2 = __instance.PlayerOwner.StartingShip.GetHiddenCODDisplayingCargoAtID(plshipComponent, plshipComponent.SortID);
                        }
                        else
                        {
                            cargoObjectDisplay2 = __instance.PlayerOwner.StartingShip.GetCODDisplayingCargoAtID(plshipComponent, plshipComponent.SortID);
                        }
                        if (cargoObjectDisplay2 != null && cargoObjectDisplay2.RootObj != null && cargoObjectDisplay2.DisplayedItem != null && !cargoObjectDisplay2.DisplayedItem.IsFlaggedForSelfDestruction())
                        {
                            float num34 = Vector3.SqrMagnitude(cargoObjectDisplay2.RootObj.transform.position - __instance.PlayerOwner.GetPawn().transform.position);
                            if (num34 < num32)
                            {
                                cargo = cargoObjectDisplay2;
                                num32 = num34;
                            }
                        }
                    }
                    if (cargo != null && cargo.DisplayedItem != null && cargo.DisplayedItem.ActualSlotType == ESlotType.E_COMP_SCRAP) 
                    {
                        if (Vector3.SqrMagnitude(cargo.RootObj.transform.position - __instance.PlayerOwner.GetPawn().transform.position) > 4) 
                        {
                            __instance.AI_TargetPos = cargo.RootObj.transform.position;
                            __instance.AI_TargetPos_Raw = __instance.AI_TargetPos;
                            __instance.EnablePathing = true;
                            return;
                        }
                        else
                        {
                            if (Random.Range(0,9) == 0)
                            {
                                __instance.PlayerOwner.ScrapProcessingAttemptsLeft--;
                            }
                            else 
                            {
                                __instance.PlayerOwner.photonView.RPC("AttemptToProcessScrapCargo", PhotonTargets.MasterClient, new object[]
                                {
                                __instance.PlayerOwner.StartingShip.ShipID,
                                cargo.DisplayedItem.NetID
                                });
                            }
                            return;
                        }
                    }
                }
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
