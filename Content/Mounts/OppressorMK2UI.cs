using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace LK_Ugrumiy_WP.Content.Mounts
{
    public class OppressorMK2UI : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // Ищем слой ресурсных полосок (хп, мана), чтобы нарисовать нашу шкалу рядом
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBarIndex != -1)
            {
                layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                    "LK_Ugrumiy_WP: Oppressor Dash Cooldown",
                    delegate
                    {
                        DrawDashCooldown(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.Game));
            }

            int cursorIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Cursor"));
            if (cursorIndex != -1)
            {
                layers.Insert(cursorIndex, new LegacyGameInterfaceLayer(
                    "LK_Ugrumiy_WP: Oppressor Dash Screen Effect",
                    delegate
                    {
                        DrawDashScreenEffect(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }

        private void DrawDashCooldown(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            
            // Проверяем, что игрок жив, активен и сидит на Апрессоре
            if (!player.active || player.dead || player.mount.Type != ModContent.MountType<OppressorMK2>())
                return;

            OppressorMK2Player modPlayer = player.GetModPlayer<OppressorMK2Player>();

            // Вычисляем прогресс перезарядки (customDashDelay уменьшается с 300 до 0)
            float progress = 1f - (modPlayer.customDashDelay / 300f);
            progress = MathHelper.Clamp(progress, 0f, 1f);

            int barWidth = 40;
            int barHeight = 6;
            
            // Позиция под игроком в мире (смещаем чуть ниже ног)
            Vector2 worldPos = player.Bottom + new Vector2(0, 15);
            
            // Переводим мировые координаты в координаты экрана
            Vector2 screenPos = worldPos - Main.screenPosition;

            int x = (int)screenPos.X - barWidth / 2;
            int y = (int)screenPos.Y;

            // Цвета
            Color borderColor = Color.Black;
            Color emptyColor = new Color(139, 69, 0); // Темно-оранжевый
            Color filledColor = new Color(255, 140, 0); // Ярко-оранжевый

            Texture2D pixel = TextureAssets.MagicPixel.Value;

            // 1. Черная обводка (рисуем прямоугольник чуть большего размера)
            spriteBatch.Draw(pixel, new Rectangle(x - 2, y - 2, barWidth + 4, barHeight + 4), borderColor);

            // 2. Темно-оранжевый фон (пустая шкала)
            spriteBatch.Draw(pixel, new Rectangle(x, y, barWidth, barHeight), emptyColor);

            // 3. Ярко-оранжевая заполненная часть
            int filledWidth = (int)(barWidth * progress);
            if (filledWidth > 0)
            {
                spriteBatch.Draw(pixel, new Rectangle(x, y, filledWidth, barHeight), filledColor);
            }
        }

        private void DrawDashScreenEffect(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            
            if (!player.active || player.dead || player.mount.Type != ModContent.MountType<OppressorMK2>())
                return;

            OppressorMK2Player modPlayer = player.GetModPlayer<OppressorMK2Player>();

            if (modPlayer.customDashTimer > 0)
            {
                // Прогресс от 1 до 0 (20 тиков = начальное значение таймера)
                float progress = modPlayer.customDashTimer / 30f; 
                
                // Эффект мерцания на основе времени
                float flicker = (float)System.Math.Abs(System.Math.Sin(Main.GlobalTimeWrappedHourly * 30f));
                
                // Цвет переливается между желтым и оранжевым
                Color color = Color.Lerp(Color.Yellow, new Color(255, 100, 0), flicker);
                
                // Прозрачность затухает по мере окончания таймера (значительно уменьшено)
                float alpha = progress * 0.12f; 
                
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                
                // Легкий общий фон экрана
                Rectangle screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                spriteBatch.Draw(pixel, screenRect, color * alpha);
                
                // Более яркие края (эффект виньетки скорости) - размер и яркость уменьшены
                int borderSize = (int)(50 * progress);
                float borderAlpha = alpha * 1.5f;
                
                // Верхняя и нижняя границы
                spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, borderSize), color * borderAlpha);
                spriteBatch.Draw(pixel, new Rectangle(0, Main.screenHeight - borderSize, Main.screenWidth, borderSize), color * borderAlpha);
                
                // Левая и правая границы
                spriteBatch.Draw(pixel, new Rectangle(0, 0, borderSize, Main.screenHeight), color * borderAlpha);
                spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - borderSize, 0, borderSize, Main.screenHeight), color * borderAlpha);
            }
        }
    }
}
