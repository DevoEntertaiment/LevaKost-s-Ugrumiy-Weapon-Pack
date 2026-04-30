using Terraria;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Mounts.OppressorMK2
{
    public class OppressorMK2Player : ModPlayer
    {
        // Кастомный таймер для перезарядки рывка Апрессора
        public int customDashDelay = 0;
        
        // Таймер активности самого рывка (для анимации и эффектов)
        public int customDashTimer = 0;

        public override void PreUpdateMovement()
        {
            // Уменьшаем таймер каждый кадр (60 раз в секунду)
            if (customDashDelay > 0)
            {
                customDashDelay--;
            }
            if (customDashTimer > 0)
            {
                customDashTimer--;
            }
        }
    }
}
