﻿using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using UnityEngine;

namespace BetterAI
{
    internal class GeneralAI
    {
        /*
         * Bug Fixed
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
        */
    }
    [HarmonyPatch(typeof(PLLiarsDiceGame), "AI_TakeTurns")]
    class LiarsDiceAI
    {
        static bool Prefix(PLLiarsDiceGame __instance, ref ObscuredInt ___LastNewPlayerTimeMs, ref float ___LastAIUpdateTime, ref ObscuredBool ___CallBluffOverTime_InProgress, ref ObscuredBool ___Game_AllowTurnsToAdvance, ref ObscuredInt ___CurrentTurn_PlayerID, ref int ___PrevTurn_PlayerID, ref float ___GameIsActive_Time, ref List<ObscuredInt> ___CurrentlyPlaying_PlayerIDs, ref ObscuredByte ___CurrentTurn_LastDieFace, ref ObscuredByte ___CurrentTurn_LastDieCount)
        {
            if (PhotonNetwork.isMasterClient && !__instance.IsCurrentlyChallenging() && !__instance.IsCurrentlyRolling() && PLServer.Instance.GetEstimatedServerMs() - ___LastNewPlayerTimeMs > 3000 && Time.time - ___LastAIUpdateTime > 1f && !___CallBluffOverTime_InProgress && ___Game_AllowTurnsToAdvance)
            {
                ___LastAIUpdateTime = Time.time;
                PLPlayer CurrentPlayer = PLServer.Instance.GetPlayerFromPlayerID(___CurrentTurn_PlayerID);
                if (CurrentPlayer != null && (CurrentPlayer.IsBot || (___GameIsActive_Time > 20f && PLServer.Instance.GetEstimatedServerMs() - ___LastNewPlayerTimeMs > 60000)))
                {
                    Dictionary<int, int> MyDices = new Dictionary<int, int>();
                    foreach (Byte value in CurrentPlayer.LocalGame_MyDice) //Gets my Current Hand
                    {
                        int num = (int)value;
                        if (!MyDices.ContainsKey(num))
                        {
                            MyDices.Add(num, 1);
                        }
                        else
                        {
                            MyDices[num]++;
                        }
                    }
                    int Players = 0;
                    foreach (int playerID in ___CurrentlyPlaying_PlayerIDs)
                    {
                        if (PLServer.Instance.GetPlayerFromPlayerID(playerID).LocalGame_MyDice.Count > 0) //Should help count only players currently playing, not the ones waiting for next turn
                        {
                            Players++;
                        }
                    }
                    Byte CurrentFace = ___CurrentTurn_LastDieFace;
                    Byte CurrentBid = ___CurrentTurn_LastDieCount;
                    int BetFace = 0;
                    int BetFace2 = 0;
                    int BetValue = 1;
                    int BetValue2 = 1;
                    double ChanceOfTruth = 0;
                    if (CurrentPlayer.LiarsDice_DieCountOfFace(CurrentFace) >= CurrentBid || CurrentBid == 0) //If I have more dices of that type than the bid I know its true, also if there is no bid
                    {
                        ChanceOfTruth = 100;
                    }
                    else
                    {
                        for (int i = CurrentBid; i <= Players * 5; i++) //This calculates the chance of bid being true
                        {
                            ChanceOfTruth += (Factorial(Players * 5) / (Factorial(i) * Factorial(Players * 5 - i))) * Math.Pow(1f / 6f, i) * Math.Pow(5f / 6f, Players * 5 - i);
                        }
                        ChanceOfTruth *= 100; //Transforms the decimal 0,22 in to 22% for example
                    }
                    foreach (Byte Face in MyDices.Keys) //Gets the highest value Dice in My Hand
                    {
                        if (MyDices.GetValueSafe(Face) > BetValue)
                        {
                            BetFace2 = BetFace;
                            BetValue2 = BetValue;
                            BetFace = Face;
                        }
                        else if (MyDices.GetValueSafe(Face) > BetValue2)//Gets sencond highest value Dice in My Hand 
                        {
                            BetFace2 = Face;
                            BetValue2 = MyDices.GetValueSafe(Face);
                        }
                    }
                    if (UnityEngine.Random.Range(0, 2) == 1) //Should Help AI get not as predicable (because it would always bet dice with biggest number, so now it has a chance of playing the second biggest dice) 
                    {
                        BetFace = BetFace2;
                    }
                    BetValue = (int)UnityEngine.Random.Range(CurrentBid + 1, CurrentBid + (float)Math.Ceiling(Players * 5 * 0.1)); //Gets a random number for the next challenge value between 1 and 10% of the dices
                    if (UnityEngine.Random.Range(0f, 100f) > ChanceOfTruth + 3 && ChanceOfTruth != 100) //Challanges if my random number is bigger than the chance of failure (Plus a little ballance to encorage a little rasing)
                    {
                        __instance.CallBluff();
                    }
                    else
                    {
                        __instance.Raise((Byte)BetFace, (Byte)BetValue);
                    }
                }
            }
            return false;
        }
        public static double Factorial(double num)
        {
            if (num == 0)
            {
                return 1;
            }
            else if (num < 0)
            {
                throw new Exception("Can't Factorial negative numbers!");
            }
            for (int i = (int)num - 1; i > 0; i--)
            {
                num *= i;
            }
            return num;
        }
    }
}

