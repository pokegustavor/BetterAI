using HarmonyLib;

namespace BetterAI
{
    
    internal class Weapons
    {
        [HarmonyPatch(typeof(PLBot), "TickManTurretAction")]
        class PerformOtherActions 
        {
            static void Postfix(PLBot __instance)
            {
                if(__instance.PlayerOwner.GetClassID() == 3) 
                {
                    Scientist.ScienceScreen.Postfix(__instance);
                    Engineer.EngineScreens.Postfix(__instance);
                }
            }
        }
    }
}
