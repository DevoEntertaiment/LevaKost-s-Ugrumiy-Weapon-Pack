using Terraria;
using Terraria.ModLoader;
using LK_Ugrumiy_WP.Content.Mounts;

namespace LK_Ugrumiy_WP.Content.Buffs
{
    public class OppressorMK2Buff : ModBuff
    {
        // Временно используем текстуру маунта как иконку баффа, так как файл OppressorMK2Buff.png отсутствует
        public override string Texture => "LK_Ugrumiy_WP/Content/Mounts/OppressorMK2";
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.mount.SetMount(ModContent.MountType<OppressorMK2>(), player);
            player.buffTime[buffIndex] = 10;
        }
    }
}
