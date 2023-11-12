using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
namespace BetterAI
{
    public class Engineer
    {
        [HarmonyPatch(typeof(PLBot), "TickOptimizeStationAction")]
        public class EngineScreens 
        {
            public static Vector3[] scraps = new Vector3[5] { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
            public static void Postfix(PLBot __instance) 
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
                        if (cargoObjectDisplay2 != null && cargoObjectDisplay2.RootObj != null && cargoObjectDisplay2.DisplayedItem != null && !cargoObjectDisplay2.DisplayedItem.IsFlaggedForSelfDestruction() && cargoObjectDisplay2.DisplayedItem.ActualSlotType == ESlotType.E_COMP_SCRAP)
                        {
                            float num34 = Vector3.SqrMagnitude(cargoObjectDisplay2.RootObj.transform.position - __instance.PlayerOwner.GetPawn().transform.position);
                            bool alreadyScrapping = false;
                            for(int i = 0; i < 5; i++) 
                            {
                                if (i == __instance.PlayerOwner.GetClassID()) continue;
                                if (scraps[i] == cargoObjectDisplay2.RootObj.transform.position) 
                                {
                                    alreadyScrapping = true;
                                    break;
                                }
                            }
                            if (num34 < num32 && !alreadyScrapping)
                            {
                                cargo = cargoObjectDisplay2;
                                num32 = num34;
                                scraps[__instance.PlayerOwner.GetClassID()] = cargo.RootObj.transform.position;
                            }
                        }
                    }
                    if (cargo != null && cargo.DisplayedItem != null) 
                    {
                        if (Vector3.SqrMagnitude(cargo.RootObj.transform.position - __instance.PlayerOwner.GetPawn().transform.position) > 6) 
                        {
                            __instance.AI_TargetPos = cargo.RootObj.transform.position;
                            __instance.AI_TargetPos_Raw = __instance.AI_TargetPos;
                            __instance.EnablePathing = true;
                            return;
                        }
                        else
                        {
                            if (Random.value < 0.05f)
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
                    else if(scraps[__instance.PlayerOwner.GetClassID()].y != 0 && scraps[__instance.PlayerOwner.GetClassID()].z != 0 && scraps[__instance.PlayerOwner.GetClassID()].x != 0)
                    {
                        scraps[__instance.PlayerOwner.GetClassID()] = new Vector3(0,0,0);
                    }
                }
                if (__instance.PlayerOwner.StartingShip == null || __instance.PlayerOwner.GetPawn() == null || __instance.PlayerOwner.GetClassID() != 4 || __instance.PlayerOwner.MyCurrentTLI != __instance.PlayerOwner.StartingShip.MyTLI) return;
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
        [HarmonyPatch(typeof(PLBot), "Update")]
        class ReactorSafety 
        {
            static bool shouldChange = false;
            static void Postfix(PLBot __instance) 
            {
                if (__instance.PlayerOwner.StartingShip == null || __instance.PlayerOwner.GetPawn() == null || __instance.PlayerOwner.GetClassID() != 4 || Time.time - __instance.PlayerOwner.StartingShip.LastReactorCoolingToggleTime < 3f || __instance.PlayerOwner.StartingShip.ShipTypeID == EShipType.E_ABYSS_PLAYERSHIP) return;
                Vector3 down = new Vector3(0, 1, 0);
                PLReactorSafetyPanel myPanel = null;
                if (__instance.PlayerOwner.StartingShip.InteriorDynamic != null)
                {
                    myPanel = __instance.PlayerOwner.StartingShip.InteriorDynamic.GetComponentInChildren<PLReactorSafetyPanel>();
                }
                if (myPanel == null && __instance.PlayerOwner.StartingShip.InteriorStatic != null)
                {
                    myPanel = __instance.PlayerOwner.StartingShip.InteriorStatic.GetComponentInChildren<PLReactorSafetyPanel>();
                }
                if (myPanel == null) return;
                if (!__instance.PlayerOwner.StartingShip.ReactorCoolingEnabled && (__instance.AI_TargetTLI != __instance.PlayerOwner.StartingShip.MyTLI || __instance.PlayerOwner.StartingShip.ReactorCoolantLevelPercent <= 0 || __instance.PlayerOwner.MyCurrentTLI != __instance.PlayerOwner.StartingShip.MyTLI)) 
                {
                    shouldChange = true;
                }
                else if(__instance.PlayerOwner.StartingShip.ReactorCoolingEnabled && __instance.PlayerOwner.StartingShip.ReactorCoolantLevelPercent > 0 && __instance.PlayerOwner.MyCurrentTLI == __instance.PlayerOwner.StartingShip.MyTLI && __instance.AI_TargetTLI == __instance.PlayerOwner.StartingShip.MyTLI) 
                {
                    shouldChange = true;
                }
                if ((myPanel.transform.position - __instance.PlayerOwner.GetPawn().transform.position).sqrMagnitude > 16 && shouldChange && __instance.PlayerOwner.MyCurrentTLI == __instance.PlayerOwner.StartingShip.MyTLI)
                {
                    __instance.AI_TargetPos = myPanel.transform.position + myPanel.transform.forward - down;
                    __instance.AI_TargetPos_Raw = __instance.AI_TargetPos;
                    __instance.AI_TargetTLI = __instance.PlayerOwner.StartingShip.MyTLI;
                    __instance.EnablePathing = true;
                }
                else if(shouldChange && __instance.PlayerOwner.MyCurrentTLI == __instance.PlayerOwner.StartingShip.MyTLI && (myPanel.transform.position - __instance.PlayerOwner.GetPawn().transform.position).sqrMagnitude < 16)
                {
                    __instance.PlayerOwner.StartingShip.ReactorCoolingEnabled = !__instance.PlayerOwner.StartingShip.ReactorCoolingEnabled;
                    __instance.PlayerOwner.StartingShip.LastReactorCoolingToggleTime = Time.time;
                    shouldChange = false;
                }
            }
        }
        [HarmonyPatch(typeof(PLEngineerReactorScreen), "OptimizeForBot_OnLeave")]
        class DisablePowerReductionOnLeave 
        {
            static bool Prefix(PLEngineerReactorScreen __instance) 
            {
                if (__instance.MyScreenHubBase.OptionalShipInfo.Reactor_OCActive)
                {
                    __instance.MyScreenHubBase.OptionalShipInfo.photonView.RPC("SetReactorOCStatus", PhotonTargets.All, new object[] { !__instance.MyScreenHubBase.OptionalShipInfo.Reactor_OCActive });
                }
                return false;
            }
        }
    }
}
