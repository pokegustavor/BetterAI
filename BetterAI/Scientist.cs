using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
namespace BetterAI
{
    public class Scientist
    {
        [HarmonyPatch(typeof(PLBot), "TickOptimizeStationAction")]
        public class ScienceScreen 
        {
            public static void Postfix(PLBot __instance)
            {
                if (__instance.PlayerOwner == null || __instance.PlayerOwner.StartingShip == null) return;
                Vector3 down = new Vector3(0, 1, 0);
                PLUIScreen sciencescreen = null;
                List<PLPawnItem_ResearchMaterial> myResearch = new List<PLPawnItem_ResearchMaterial>();
                List<int> IDS = new List<int>();
                foreach (PLMissionObjective objective in PLMissionObjective.AllMissionObjectives)
                {
                    if ((objective as PLMissionObjective_PickupItem) != null && (objective as PLMissionObjective_PickupItem).ItemTypeToPickup == EPawnItemType.E_RESEARCH_MAT)
                    {
                        IDS.Add((objective as PLMissionObjective_PickupItem).SubItemType);
                    }
                }
                foreach (PLPawnItem item in __instance.PlayerOwner.MyInventory.AllItems)
                {
                    if (item as PLPawnItem_ResearchMaterial != null && !IDS.Contains((item as PLPawnItem_ResearchMaterial).SubType))
                    {
                        myResearch.Add(item as PLPawnItem_ResearchMaterial);
                    }
                }
                GameObject atomizer = __instance.PlayerOwner.StartingShip.ResearchLockerCollider.gameObject;
                if (myResearch.Count > 0 && atomizer != null && __instance.PlayerOwner.MyCurrentTLI == __instance.PlayerOwner.StartingShip.MyTLI && __instance.PlayerOwner.TeamID == 0)
                {
                    if ((atomizer.transform.position - __instance.PlayerOwner.GetPawn().transform.position).sqrMagnitude > 16)
                    {
                        __instance.AI_TargetPos = atomizer.transform.position + atomizer.transform.forward - down;
                        __instance.AI_TargetPos_Raw = __instance.AI_TargetPos;
                        __instance.EnablePathing = true;
                    }
                    else
                    {
                        foreach (PLPawnItem_ResearchMaterial research in myResearch)
                        {
                            __instance.PlayerOwner.MyInventory.photonView.RPC("ServerItemSwap", PhotonTargets.All, new object[]
                            {
                            PLServer.Instance.ResearchLockerInventory.InventoryID,
                            research.NetID
                            });
                        }
                        __instance.PlayerOwner.StartingShip.photonView.RPC("ServerClickAtomize", PhotonTargets.All, new object[0]);
                    }
                }
                if (__instance.PlayerOwner.GetPlayerID() == __instance.PlayerOwner.StartingShip.SensorDishControllerPlayerID && __instance.PlayerOwner.TeamID == 0)
                {
                    PLShipInfo ship = __instance.PlayerOwner.StartingShip;
                    Vector3 normalized = (ship.TractorBeamRotation * Vector3.forward).normalized;
                    PLProbePickup pickup = null;
                    foreach (int ID in PLEncounterManager.Instance.GetCPEI().MyPersistantData.ProbePickupPersistantData.Keys)
                    {
                        if (PLGameStatic.Instance.GetProbePickupObjectAtID(ID).VisibleObject.GetActive() == false) continue;
                        if (pickup == null)
                        {
                            pickup = PLGameStatic.Instance.GetProbePickupObjectAtID(ID);
                        }
                        else if (Vector3.SqrMagnitude(PLGameStatic.Instance.GetProbePickupObjectAtID(ID).VisibleObject.transform.position - ship.GetSensorDishPosition()) < Vector3.SqrMagnitude(pickup.VisibleObject.transform.position - ship.GetSensorDishPosition()))
                        {
                            pickup = PLGameStatic.Instance.GetProbePickupObjectAtID(ID);
                        }
                    }
                    if (pickup != null && Vector3.Magnitude(pickup.VisibleObject.transform.position - ship.GetSensorDishPosition()) < 250 && !ship.InWarp)
                    {
                        ship.SendFireProbeMsg(__instance.PlayerOwner.GetPlayerID(), ship.GetSensorDishPosition(), PLGlobal.SafeLookRotation(((pickup.VisibleObject.transform.position - ship.GetSensorDishPosition()).normalized).normalized));
                    }
                    if (ship.SensorDishCurrentSecondaryTarget_Scrap != null)
                    {
                        ship.photonView.RPC("RequestScrapCollectFromSensorDish", PhotonTargets.MasterClient, new object[]
                        {
                            ship.SensorDishCurrentSecondaryTarget_Scrap.EncounterNetID
                        });
                    }
                }
                if (__instance.PlayerOwner.StartingShip == null || __instance.PlayerOwner.GetPawn() == null || __instance.PlayerOwner.GetClassID() != 2
                    || __instance.PlayerOwner.StartingShip.ShipTypeID == EShipType.E_INTREPID || __instance.PlayerOwner.StartingShip.ShipTypeID == EShipType.E_INTREPID_SC || __instance.PlayerOwner.StartingShip.ShipTypeID == EShipType.E_FLUFFY_TWO || __instance.PlayerOwner.MyCurrentTLI != __instance.PlayerOwner.StartingShip.MyTLI) return;
                foreach (PLUIScreen screen in __instance.PlayerOwner.StartingShip.MyScreenBase.AllScreens)
                {
                    if (((screen.name.ToLower().Contains("cloned") && __instance.PlayerOwner.StartingShip.ShipTypeID != EShipType.E_STARGAZER) || (__instance.PlayerOwner.StartingShip.ShipTypeID == EShipType.E_STARGAZER && !screen.name.ToLower().Contains("cloned"))) && (screen.name.ToLower().Contains("computer") || screen.name.ToLower().Contains("science") || (__instance.PlayerOwner.StartingShip.ShipTypeID == EShipType.E_DESTROYER && screen.name.ToLower().Contains("status 6 (6)"))))
                    {
                        sciencescreen = screen;
                        break;
                    }
                }
                GameObject spawn = __instance.PlayerOwner.StartingShip.Spawners[__instance.PlayerOwner.GetClassID()] as GameObject;
                if (sciencescreen != null && (__instance.AI_TargetPos == spawn.transform.position || __instance.AI_TargetPos == sciencescreen.transform.position + sciencescreen.transform.forward - down) && __instance.PlayerOwner.MyCurrentTLI != null && __instance.PlayerOwner.MyCurrentTLI == __instance.PlayerOwner.StartingShip.MyTLI)
                {
                    __instance.AI_TargetPos = sciencescreen.transform.position + sciencescreen.transform.forward - down;
                    __instance.AI_TargetPos_Raw = __instance.AI_TargetPos;
                    __instance.EnablePathing = true;
                }
            }
        }

        [HarmonyPatch(typeof(PLBot), "TickGoCloseToCaptainAction")]
        class PickItems 
        {
            static void Postfix(PLBot __instance) 
            {
                float maxDistance = 13 * 13;
                if (__instance.PlayerOwner.GetPawn() == null || __instance.PlayerOwner.MyInventory == null) return;
                PLPawnItem_Scanner scanner = null;
                using (List<PLPawnItem>.Enumerator enumerator = __instance.PlayerOwner.MyInventory.AllItems.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        PLPawnItem plpawnItem = enumerator.Current;
                        if (plpawnItem != null && plpawnItem.EquipID != -1 && plpawnItem.PawnItemType == EPawnItemType.E_SCANNER)
                        {
                            scanner = (plpawnItem as PLPawnItem_Scanner);
                            break;
                        }
                    }
                }
                if (scanner == null) return;
                if (__instance.PlayerOwner.Talents[34] > 0)
                {
                    List<PLPickupObject> allitems = new List<PLPickupObject>();
                    foreach (PLPickupObject inObj in PLGameStatic.Instance.m_AllPickupObjects)
                    {
                        if (inObj != null && inObj.gameObject.activeInHierarchy && !inObj.PickedUp && inObj.PickupID != -1 && __instance.PlayerOwner.GetPawn() != null && __instance.PlayerOwner.GetPawn().MyCurrentTLI != null && __instance.PlayerOwner.GetPawn().MyCurrentTLI.SubHubID == 9001 && __instance.PlayerOwner.GetPawn().GetPlayer() != null && __instance.PlayerOwner.GetPawn().GetPlayer().CurrentInterior == inObj.MyInterior && inObj.ItemType != EPawnItemType.E_RESEARCH_MAT && (inObj.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude < maxDistance)
                        {
                            allitems.Add(inObj);
                        }
                    }
                    PLPickupObject nearest = null;
                    foreach (PLPickupObject obj in allitems)
                    {
                        if (nearest == null)
                        {
                            nearest = obj;
                        }
                        else if ((obj.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude < (nearest.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude)
                        {
                            nearest = obj;
                        }
                    }
                    if (nearest != null)
                    {
                        if ((nearest.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude > 16)
                        {
                            __instance.AI_TargetPos = nearest.gameObject.transform.position;
                            __instance.AI_TargetPos_Raw = __instance.AI_TargetPos;
                            __instance.EnablePathing = true;
                        }
                        else
                        {
                            __instance.PlayerOwner.photonView.RPC("AttemptToPickupObjectAtID", PhotonTargets.MasterClient, new object[]
                            {
                            nearest.PickupID
                            });
                            __instance.PlayerOwner.GetPawn().photonView.RPC("Anim_Pickup", PhotonTargets.Others, new object[0]);
                            PLMusic.PostEvent("play_sx_player_item_pickup", __instance.PlayerOwner.GetPawn().gameObject);
                        }
                    }
                    List<PLPickupComponent> allComp = new List<PLPickupComponent>();
                    foreach (PLPickupComponent inComp in PLGameStatic.Instance.m_AllPickupComponents)
                    {
                        if (inComp != null && inComp.gameObject.activeInHierarchy && !inComp.PickedUp && inComp.GetInternalComp() != null && inComp.PickupID != -1 && __instance.PlayerOwner.GetPawn() != null && __instance.PlayerOwner.GetPawn().MyCurrentTLI.SubHubID == 9001 && __instance.PlayerOwner.GetPawn().GetPlayer() != null && __instance.PlayerOwner.CurrentInterior == inComp.MyInterior && (inComp.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude < maxDistance)
                        {
                            allComp.Add(inComp);
                        }
                    }
                    List<PLPickupRandomComponent> allrandComp = new List<PLPickupRandomComponent>();
                    foreach (PLPickupRandomComponent inComp in PLGameStatic.Instance.m_AllPickupRandomComponents)
                    {
                        if (inComp != null && inComp.gameObject.activeInHierarchy && !inComp.PickedUp && inComp.GetInternalComp() != null && inComp.PickupID != -1 && __instance.PlayerOwner.GetPawn() != null && __instance.PlayerOwner.GetPawn().MyCurrentTLI.SubHubID == 9001 && __instance.PlayerOwner.GetPawn().GetPlayer() != null && __instance.PlayerOwner.GetPawn().GetPlayer().CurrentInterior == inComp.MyInterior && (inComp.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude < maxDistance) 
                        {
                            allrandComp.Add(inComp);
                        }
                    }
                    PLPickupComponent nearestComp = null;
                    foreach(PLPickupComponent comp in allComp) 
                    {
                        if (nearestComp == null)
                        {
                            nearestComp = comp;
                        }
                        else if ((comp.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude < (nearestComp.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude)
                        {
                            nearestComp = comp;
                        }
                    }
                    if (nearestComp != null)
                    {
                        if ((nearestComp.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude > 16)
                        {
                            __instance.AI_TargetPos = nearestComp.gameObject.transform.position;
                            __instance.AI_TargetPos_Raw = __instance.AI_TargetPos;
                            __instance.EnablePathing = true;
                        }
                        else
                        {
                            __instance.PlayerOwner.photonView.RPC("AttemptToPickupComponentAtID", PhotonTargets.MasterClient, new object[]
                            {
                            nearestComp.PickupID
                            });
                            __instance.PlayerOwner.GetPawn().photonView.RPC("Anim_Pickup", PhotonTargets.Others, new object[0]);
                            PLMusic.PostEvent("play_sx_player_item_pickup_large", __instance.PlayerOwner.GetPawn().gameObject);
                        }
                    }
                    PLPickupRandomComponent nearestRandComp = null;
                    foreach (PLPickupRandomComponent comp in allrandComp)
                    {
                        if (nearestRandComp == null)
                        {
                            nearestRandComp = comp;
                        }
                        else if ((comp.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude < (nearestRandComp.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude)
                        {
                            nearestRandComp = comp;
                        }
                    }
                    if (nearestRandComp != null)
                    {
                        if ((nearestRandComp.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude > 16)
                        {
                            __instance.AI_TargetPos = nearestRandComp.gameObject.transform.position;
                            __instance.AI_TargetPos_Raw = __instance.AI_TargetPos;
                            __instance.EnablePathing = true;
                        }
                        else
                        {
                            __instance.PlayerOwner.photonView.RPC("AttemptToPickupRandomComponentAtID", PhotonTargets.MasterClient, new object[]
                            {
                            nearestRandComp.PickupID
                            });
                            __instance.PlayerOwner.GetPawn().photonView.RPC("Anim_Pickup", PhotonTargets.Others, new object[0]);
                            PLMusic.PostEvent("play_sx_player_item_pickup_large", __instance.PlayerOwner.GetPawn().gameObject);
                        }
                    }
                }
                if (__instance.PlayerOwner.Talents[33] > 0)
                {
                    List<PLPickupObject> allresearch = new List<PLPickupObject>();
                    foreach (PLPickupObject inObj in PLGameStatic.Instance.m_AllPickupObjects)
                    {
                        if (inObj != null && inObj.gameObject.activeInHierarchy && !inObj.PickedUp && inObj.PickupID != -1 && __instance.PlayerOwner.GetPawn() != null && __instance.PlayerOwner.GetPawn().MyCurrentTLI != null && __instance.PlayerOwner.GetPawn().MyCurrentTLI.SubHubID == 9001 && __instance.PlayerOwner.GetPawn().GetPlayer() != null && __instance.PlayerOwner.GetPawn().GetPlayer().CurrentInterior == inObj.MyInterior && inObj.ItemType == EPawnItemType.E_RESEARCH_MAT && (inObj.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude < maxDistance)
                        {
                            allresearch.Add(inObj);
                        }
                    }
                    PLPickupObject nearest = null;
                    foreach (PLPickupObject obj in allresearch)
                    {
                        if (nearest == null)
                        {
                            nearest = obj;
                        }
                        else if ((obj.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude < (nearest.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude)
                        {
                            nearest = obj;
                        }
                    }
                    if (nearest != null)
                    {
                        if ((nearest.gameObject.transform.position - __instance.PlayerOwner.GetPawn().gameObject.transform.position).sqrMagnitude > 16)
                        {
                            __instance.AI_TargetPos = nearest.gameObject.transform.position;
                            __instance.AI_TargetPos_Raw = __instance.AI_TargetPos;
                            __instance.EnablePathing = true;
                        }
                        else
                        {
                            __instance.PlayerOwner.photonView.RPC("AttemptToPickupObjectAtID", PhotonTargets.MasterClient, new object[]
                            {
                            nearest.PickupID
                            });
                            __instance.PlayerOwner.GetPawn().photonView.RPC("Anim_Pickup", PhotonTargets.Others, new object[0]);
                            PLMusic.PostEvent("play_sx_player_item_pickup", __instance.PlayerOwner.GetPawn().gameObject);
                        }
                    }
                }
            }
        }
    }
}
