using Microsoft.Xna.Framework;
using ReLogic.Utilities;
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

        // SlotId-ы активных звуков — чтобы каждый кадр обновлять Position под Player.Center,
        // иначе звук остаётся "висеть" в точке спавна, и игрок улетает от него.
        // Паттерн взят из Calamity (WulfrumDiggingTurtleProjectile.cs:84+).
        private SlotId engineStartSoundSlot;
        private SlotId boostSoundSlot;
        private SlotId cooldownSoundSlot;

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

                    // Двойной тап синкается естественно через ModMount.UpdateEffects (control-флаги синхронны).
                    // E-кнопка локальная (ModKeybind) — нужен явный sync-пакет, иначе на других клиентах будет фантомный буст.
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        ModPacket packet = Mod.GetPacket();
                        packet.Write((byte)LK_Ugrumiy_WP.MessageType.OppressorMK2BoostSync);
                        packet.Write((sbyte)Player.direction);
                        packet.Send();
                    }
                }
                else
                {
                    PlayCooldownSound();
                }
            }
        }

        // Подтаскиваем активные звуки за игроком, чтобы они не висели в точке спавна.
        public override void PostUpdate()
        {
            UpdateSoundPosition(ref engineStartSoundSlot);
            UpdateSoundPosition(ref boostSoundSlot);
            UpdateSoundPosition(ref cooldownSoundSlot);
        }

        private void UpdateSoundPosition(ref SlotId slot)
        {
            if (SoundEngine.TryGetActiveSound(slot, out ActiveSound activeSound) && activeSound != null)
            {
                activeSound.Position = Player.Center;
            }
        }

        // Триггер буста на локальном игроке: вызывается из E-кнопки и из двойного тапа стрелок.
        // Меняет velocity и применяет общие эффекты (timer/sound/dust).
        // MP-sync для E-кнопки делается на месте вызова (PreUpdateMovement).
        public void TriggerBoost(int dashDirection)
        {
            if (dashDirection == 0)
            {
                return;
            }

            Player.velocity.X = 45f * dashDirection;
            ApplyBoostEffects(dashDirection);
        }

        // Применить эффекты буста на удалённого игрока (без изменения velocity — её пушит сам клиент игрока).
        public void ApplyBoostEffectsRemote(int dashDirection)
        {
            ApplyBoostEffects(dashDirection);
        }

        private void ApplyBoostEffects(int dashDirection)
        {
            customDashDelay = 300; // Перезарядка (300 тиков = 5 секунд)
            customDashTimer = 30;  // Время действия рывка

            PlayBoostSound();

            // Визуальный эффект дыма/пыли
            for (int i = 0; i < 45; i++)
            {
                int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Smoke, 0f, 0f, 100, default(Color), 2.5f);
                Main.dust[d].velocity *= 4.5f;
                Main.dust[d].noGravity = true;
            }
        }

        // Стартуем звук от позиции игрока и запоминаем SlotId, чтобы PostUpdate подтягивал
        // Position за игроком (3D-аудио иначе остаётся в точке спавна и затихает в одном ухе).

        public void PlayEngineStartSound()
        {
            engineStartSoundSlot = SoundEngine.PlaySound(OppressorMK2Sounds.EngineStart, Player.Center);
        }

        public void PlayBoostSound()
        {
            boostSoundSlot = SoundEngine.PlaySound(OppressorMK2Sounds.Boost, Player.Center);
        }

        public void PlayCooldownSound()
        {
            cooldownSoundSlot = SoundEngine.PlaySound(OppressorMK2Sounds.Cooldown, Player.Center);
        }
    }
}
