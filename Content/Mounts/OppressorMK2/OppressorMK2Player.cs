using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Mounts.OppressorMK2
{
    public class OppressorMK2Player : ModPlayer
    {
        // Кастомный таймер для перезарядки рывка Опрессора
        public int customDashDelay = 0;

        // Таймер активности самого рывка (для анимации и эффектов)
        public int customDashTimer = 0;

        public override void PreUpdateMovement()
        {
            // Уменьшаем таймеры каждый кадр (60 раз в секунду)
            if (customDashDelay > 0)
            {
                customDashDelay--;
            }
            if (customDashTimer > 0)
            {
                customDashTimer--;
            }

            // E-кнопка для буста (только на локальном игроке)
            if (Player.whoAmI != Main.myPlayer)
            {
                return;
            }

            if (!Player.mount.Active || Player.mount.Type != ModContent.MountType<OppressorMK2>())
            {
                return;
            }

            if (OppressorMK2System.BoostKeybind != null && OppressorMK2System.BoostKeybind.JustPressed)
            {
                if (customDashDelay == 0)
                {
                    TriggerBoost(Player.direction);
                }
                else
                {
                    SoundEngine.PlaySound(OppressorMK2Sounds.Cooldown, Player.Center);
                }
            }
        }

        // Триггер буста: вызывается из E-кнопки и из двойного тапа стрелок
        public void TriggerBoost(int dashDirection)
        {
            if (dashDirection == 0)
            {
                return;
            }

            Player.velocity.X = 45f * dashDirection;
            customDashDelay = 300; // Перезарядка (300 тиков = 5 секунд)
            customDashTimer = 30;  // Время действия рывка

            SoundEngine.PlaySound(OppressorMK2Sounds.Boost, Player.Center);

            // Визуальный эффект дыма/пыли
            for (int i = 0; i < 45; i++)
            {
                int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Smoke, 0f, 0f, 100, default(Color), 2.5f);
                Main.dust[d].velocity *= 4.5f;
                Main.dust[d].noGravity = true;
            }
        }
    }
}
