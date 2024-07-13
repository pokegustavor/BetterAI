using HarmonyLib;
using System.Collections.Generic;

namespace BetterAI
{
    /*
    internal class PriorityMenu
    {
        [HarmonyPatch(typeof(AIPriority), "GetTootipTextForPriority")]
        class Description
        {
            static void Postfix(int priData, ref string __result)
            {
                switch (priData)
                {
                    case 84:
                        __result = "Importance of Reactor Safety been active";
                        break;
                }
            }
        }
        [HarmonyPatch(typeof(PLTabMenu), "CreatePriorityDisplayIfNecessary")]
        class Name 
        {
            static void Prefix(AIPriority pri, ref List<PLTabMenu.PrioritiesDisplay> processedPDS, PLPlayer player, ref int __state) 
            {
                __state = -1;
                if(pri.TypeData > 83) 
                {
                    __state = pri.TypeData;
                    pri.TypeData = 0;
                }
            }
            static void Postfix(AIPriority pri, ref List<PLTabMenu.PrioritiesDisplay> processedPDS, PLPlayer player, ref int __state, PLTabMenu __instance) 
            {
                if(__state != -1) 
                {
                    pri.TypeData = __state;
                    string name = "";
                    switch (__state) 
                    {
                        case 84:
                            name = "Core Safety Toggle";
                            break;
                    }
                    __instance.allPDs[__instance.allPDs.Count - 1].Name.text = name;
                }
            }
        }
        [HarmonyPatch(typeof(PLGlobal), "SetupClassDefaultData")]
        static void Postfix(ref AIDataIndividual dataInv, int classID, bool enemyAI = false)
        {
            switch (classID) 
            {
                case 4:
                    AIPriority aipriority = new AIPriority(AIPriorityType.E_CLASS_MAIN, 84, 0);
                    dataInv.Priorities.Add(aipriority);
                    PulsarModLoader.Utilities.Messaging.Notification("Added!");
                    break;
            }
        }
    }
    */
}
