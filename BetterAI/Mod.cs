using PulsarModLoader;

namespace BetterAI
{
    public class Mod : PulsarMod
    {
        public override string Version => "1.0";

        public override string Author => "pokegustavo";

        public override string ShortDescription => "Makes the AI a little better";

        public override string Name => "Better AI";

        public override string HarmonyIdentifier()
        {
            return "Pokegustavo.ExoticComponents";
        }
    }
}
