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
    }
}
