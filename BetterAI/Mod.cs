using PulsarModLoader;
using static UnityEngine.GUILayout;
using PulsarModLoader.CustomGUI;
using PulsarModLoader.Patches;
[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo("Assembly-CSharp")]
namespace BetterAI
{
    public class Mod : PulsarMod
    {
        public override string Version => "1.7";

        public override string Author => "pokegustavo";

        public override string ShortDescription => "Makes the AI a little better";

        public override string Name => "Better AI";

        public override string HarmonyIdentifier()
        {
            return "Pokegustavo.BetterAI";
        }
    }

    class Settings : ModSettingsMenu
    {
        public static SaveValue<bool> Engineer_CoreSafety = new SaveValue<bool>("Engineer_CoreSafety", true);
        public static SaveValue<bool> General_MoveStation = new SaveValue<bool>("General_MoveStation", true);
        public static SaveValue<bool> General_ProcessScrap = new SaveValue<bool>("General_ProcessScrap", true);
        public static SaveValue<bool> Scientist_AtomizeResearch = new SaveValue<bool>("Scientist_AtomizeResearch", true);
        public static SaveValue<bool> General_CollectPlanetItems = new SaveValue<bool>("General_CollectPlanetItems", true);
        public override void Draw()
        {
            Engineer_CoreSafety.Value = Toggle(Engineer_CoreSafety, "EngiBot control Core Safety");
            General_MoveStation.Value = Toggle(General_MoveStation, "Bots work outside of bridge");
            General_ProcessScrap.Value = Toggle(General_ProcessScrap, "Bots process scrap");
            Scientist_AtomizeResearch.Value = Toggle(Scientist_AtomizeResearch, "SciBot atomize research items");
            General_CollectPlanetItems.Value = Toggle(General_CollectPlanetItems, "Bots collect items on planets");
        }

        public override string Name()
        {
            return "Better AI Settings";
        }
    }
}
