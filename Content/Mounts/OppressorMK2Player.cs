using Terraria;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Mounts
{
    public class OppressorMK2Player : ModPlayer
    {
        // Кастомный таймер для перезарядки рывка Апрессора
        public int customDashDelay = 0;

        public override void PreUpdateMovement()
        {
            // Уменьшаем таймер каждый кадр (60 раз в секунду)
            if (customDashDelay > 0)
            {
                customDashDelay--;
            }
        }
    }
}
