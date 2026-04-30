using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Mounts.OppressorMK2
{
    public class OppressorMK2System : ModSystem
    {
        public static ModKeybind BoostKeybind { get; private set; }

        public override void Load()
        {
            BoostKeybind = KeybindLoader.RegisterKeybind(Mod, "OppressorMK2Boost", Keys.E);
        }

        public override void Unload()
        {
            BoostKeybind = null;
        }
    }
}
