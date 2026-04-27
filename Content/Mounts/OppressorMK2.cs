using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LK_Ugrumiy_WP.Content.Buffs;

namespace LK_Ugrumiy_WP.Content.Mounts
{
    public class OppressorMK2 : ModMount
    {
        public override void SetStaticDefaults()
        {
            MountData.buff = ModContent.BuffType<OppressorMK2Buff>();

            // Базовые параметры движения (высокая скорость)
            MountData.jumpHeight = 10;
            MountData.acceleration = 0.5f;
            MountData.jumpSpeed = 8f;
            MountData.blockExtraJumps = true;
            MountData.constantJump = true;
            MountData.heightBoost = 20;
            MountData.fallDamage = 0f;
            MountData.runSpeed = 18f;
            MountData.dashSpeed = 18f;
            
            // Бесконечный полет, как у НЛО или Рыброна
            MountData.flightTimeMax = int.MaxValue;
            MountData.fatigueMax = int.MaxValue;
            MountData.usesHover = true;

            // Настройки текстуры (простые значения для заглушки 1x1)
            MountData.totalFrames = 1;
            MountData.playerYOffsets = new int[] { 30 }; // Смещение игрока (уменьшай, чтобы поднять игрока выше)
            MountData.xOffset = 0;
            MountData.yOffset = 0;
            MountData.playerHeadOffset = 22;
            MountData.bodyFrame = 3;
            
            MountData.standingFrameCount = 1;
            MountData.standingFrameDelay = 12;
            MountData.standingFrameStart = 0;
            
            MountData.runningFrameCount = 1;
            MountData.runningFrameDelay = 12;
            MountData.runningFrameStart = 0;
            
            MountData.flyingFrameCount = 1;
            MountData.flyingFrameDelay = 12;
            MountData.flyingFrameStart = 0;
            
            MountData.inAirFrameCount = 1;
            MountData.inAirFrameDelay = 12;
            MountData.inAirFrameStart = 0;
            
            MountData.idleFrameCount = 1;
            MountData.idleFrameDelay = 12;
            MountData.idleFrameStart = 0;
            MountData.idleFrameLoop = true;

            MountData.swimFrameCount = 1;
            MountData.swimFrameDelay = 12;
            MountData.swimFrameStart = 0;

            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }

            if (!Main.dedServ)
            {
                int width = 32;
                int height = 32;
                if (MountData.backTexture != null) {
                    width = Math.Max(width, MountData.backTexture.Width());
                    height = Math.Max(height, MountData.backTexture.Height());
                }
                if (MountData.frontTexture != null) {
                    width = Math.Max(width, MountData.frontTexture.Width());
                    height = Math.Max(height, MountData.frontTexture.Height());
                }
                MountData.textureWidth = width;
                MountData.textureHeight = height;
            }
        }

        public override void UpdateEffects(Player player)
        {
            // Убираем урон от падения
            player.noFallDmg = true;

            OppressorMK2Player modPlayer = player.GetModPlayer<OppressorMK2Player>();

            // Механика рывка (boost)
            if (modPlayer.customDashDelay == 0)
            {
                int dashDirection = 0;

                // Проверяем двойное нажатие вправо
                if (player.controlRight && player.releaseRight && player.doubleTapCardinalTimer[2] < 15)
                {
                    dashDirection = 1;
                }
                // Проверяем двойное нажатие влево
                else if (player.controlLeft && player.releaseLeft && player.doubleTapCardinalTimer[3] < 15)
                {
                    dashDirection = -1;
                }

                if (dashDirection != 0)
                {
                    // Выполняем рывок
                    player.velocity.X = 30f * dashDirection; 
                    modPlayer.customDashDelay = 300; // Перезарядка (300 тиков = 5 секунд)
                    
                    // Звуковой эффект
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item82, player.Center); 
                    
                    // Визуальный эффект дыма/пыли
                    for (int i = 0; i < 30; i++)
                    {
                        int d = Dust.NewDust(player.position, player.width, player.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
                        Main.dust[d].velocity *= 3f;
                        Main.dust[d].noGravity = true;
                    }
                }
            }
        }
    }
}
