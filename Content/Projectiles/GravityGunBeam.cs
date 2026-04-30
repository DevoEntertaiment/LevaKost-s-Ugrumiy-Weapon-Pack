using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Projectiles
{
    public class GravityGunBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None; // Невидимый снаряд

        // Индексы захваченных объектов (-1 если ничего не захвачено)
        private int GrabbedItemIndex = -1;
        private int GrabbedNPCIndex = -1;

        // Для определения одиночного нажатия правой кнопки
        private bool wasRightClicking = false;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2; // Поддерживается живым пока игрок зажимает кнопку
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Если игрок мертв, отпустил ЛКМ или убрал пушку - убиваем снаряд
            if (player.dead || !player.active || !player.channel || player.HeldItem.type != ModContent.ItemType<Items.Weapons.GravityGun>())
            {
                ReleaseEntity();
                Projectile.Kill();
                return;
            }

            // Продлеваем жизнь снаряду
            Projectile.timeLeft = 2;
            Projectile.Center = player.MountedCenter;

            // Захват/бросок и прицеливание управляются мышью локального игрока,
            // поэтому исполняем эту логику только на машине-владельце снаряда.
            // На остальных клиентах снаряд просто следует за игроком и рендерится.
            if (Projectile.owner != Main.myPlayer)
                return;

            // Прицеливание: поворачиваем персонажа и руку за курсором
            Vector2 targetPos = Main.MouseWorld;
            Vector2 dirToMouse = (targetPos - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            player.ChangeDir(Math.Sign(dirToMouse.X));
            player.itemRotation = (float)Math.Atan2(dirToMouse.Y * player.direction, dirToMouse.X * player.direction);
            player.itemTime = 2;
            player.itemAnimation = 2;

            // Ловим ПКМ (Правая кнопка мыши) для броска
            bool isRightClicking = Main.mouseRight;
            bool justRightClicked = isRightClicking && !wasRightClicking;
            wasRightClicking = isRightClicking;

            // Фаза 1: Если ничего не держим, пытаемся захватить
            if (GrabbedItemIndex == -1 && GrabbedNPCIndex == -1)
            {
                TryGrabEntity(targetPos);
            }

            // Фаза 2: Если что-то держим, обновляем позицию и ждем бросок
            if (GrabbedItemIndex != -1 || GrabbedNPCIndex != -1)
            {
                UpdateGrabbedEntity(targetPos, justRightClicked);
            }
        }

        private void TryGrabEntity(Vector2 targetPos)
        {
            float grabRadius = 150f; // Увеличил радиус захвата до 150

            // Визуальный индикатор поиска (чтобы было видно, что снаряд жив)
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(targetPos + Main.rand.NextVector2Circular(grabRadius, grabRadius), DustID.Flare, Vector2.Zero, 100, Color.Orange, 0.8f);
                d.noGravity = true;
            }

            float closestDist = grabRadius;
            int bestItem = -1;
            int bestNPC = -1;

            // 1. Ищем ближайший выброшенный предмет
            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (item.active && item.stack > 0)
                {
                    float dist = Vector2.Distance(targetPos, item.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        bestItem = i;
                        bestNPC = -1; // Сбрасываем NPC, так как предмет оказался ближе
                    }
                }
            }

            // 2. Ищем ближайшего моба
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                // Хватаем любых живых NPC (теперь включая боссов!), кроме манекенов
                if (npc.active && npc.type != NPCID.TargetDummy) 
                {
                    float dist = Vector2.Distance(targetPos, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        bestNPC = i;
                        bestItem = -1; // Сбрасываем предмет, так как NPC оказался ближе
                    }
                }
            }

            if (bestItem != -1)
            {
                GrabbedItemIndex = bestItem;
                return;
            }
            if (bestNPC != -1)
            {
                GrabbedNPCIndex = bestNPC;
                return;
            }

            // 3. Если нет предметов и мобов, пробуем поднять любой блок или объект (земля, камень, мебель)
            int tileX = (int)(targetPos.X / 16f);
            int tileY = (int)(targetPos.Y / 16f);
            if (WorldGen.InWorld(tileX, tileY))
            {
                Tile tile = Main.tile[tileX, tileY];
                // Проверяем только наличие тайла (убрали проверку на мебель)
                if (tile.HasTile)
                {
                    // Пытаемся сломать блок или мебель. 
                    WorldGen.KillTile(tileX, tileY, fail: false, effectOnly: false, noItem: false);
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, tileX, tileY);
                    }
                    // Если блок сломался, из него выпадет предмет. На следующем кадре мы его захватим.
                }
            }
        }

        private void UpdateGrabbedEntity(Vector2 targetPos, bool justRightClicked)
        {
            Player player = Main.player[Projectile.owner];

            // Ограничение максимальной дистанции захвата
            float maxGrabDistance = 700f;
            Vector2 dirToTarget = targetPos - player.MountedCenter;
            if (dirToTarget.Length() > maxGrabDistance)
            {
                targetPos = player.MountedCenter + dirToTarget.SafeNormalize(Vector2.Zero) * maxGrabDistance;
            }

            // Вектор броска (от игрока в сторону курсора)
            Vector2 throwVelocity = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX) * 25f;

            // ==== ЛОГИКА ДЛЯ ПРЕДМЕТОВ ====
            if (GrabbedItemIndex != -1)
            {
                Item item = Main.item[GrabbedItemIndex];
                if (!item.active)
                {
                    ReleaseEntity();
                    return;
                }

                if (justRightClicked)
                {
                    // Бросаем предмет
                    item.velocity = throwVelocity;
                    item.timeSinceItemSpawned = 0; // Обновляем таймер деспавна
                    ReleaseEntity();
                    Projectile.Kill();
                    return;
                }

                // Тянем предмет к курсору (пружинная физика)
                Vector2 toTarget = targetPos - item.Center;
                item.velocity = toTarget * 0.2f; 
                
                // Запрещаем предмету проваливаться сквозь блоки и падать вниз из-за гравитации
                // item.position -= new Vector2(0, 0.1f);
                
                // Запрещаем игроку подбирать предмет в инвентарь, пока мы его держим
                item.noGrabDelay = 10;

                DrawBeam(player.MountedCenter, item.Center);
            }

            // ==== ЛОГИКА ДЛЯ NPC ====
            if (GrabbedNPCIndex != -1)
            {
                NPC npc = Main.npc[GrabbedNPCIndex];
                if (!npc.active)
                {
                    ReleaseEntity();
                    return;
                }

                if (justRightClicked)
                {
                    // Бросаем моба
                    npc.velocity = throwVelocity;

                    // Устанавливаем флаг броска для нанесения урона при столкновении
                    if (npc.TryGetGlobalNPC(out NPCs.GravityGunGlobalNPC gNpc))
                    {
                        gNpc.thrownByGravityGun = true;
                        gNpc.throwTimer = 180; // 3 секунды на то, чтобы врезаться
                        gNpc.lastVelocityLength = throwVelocity.Length();
                    }

                    ReleaseEntity();
                    Projectile.Kill();
                    return;
                }

                // Тянем моба к курсору
                Vector2 toTarget = targetPos - npc.Center;
                npc.velocity = toTarget * 0.2f;

                DrawBeam(player.MountedCenter, npc.Center);
            }
        }

        private void ReleaseEntity()
        {
            GrabbedItemIndex = -1;
            GrabbedNPCIndex = -1;
        }

        private void DrawBeam(Vector2 startPos, Vector2 endPos)
        {
            // Отрисовка луча с помощью частиц (пыли)
            int dustCount = (int)(Vector2.Distance(startPos, endPos) / 10f);
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 pos = Vector2.Lerp(startPos, endPos, i / (float)dustCount);
                if (Main.rand.NextBool(3)) // Создаем частицы не каждый кадр, чтобы не перегружать
                {
                    Dust d = Dust.NewDustPerfect(pos, DustID.Flare, Vector2.Zero, 100, Color.Orange, 1.2f);
                    d.noGravity = true;
                    d.noLight = false;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];

            // Рисуем спрайт самой пушки в руке игрока
            Texture2D texture = TextureAssets.Item[ModContent.ItemType<Items.Weapons.GravityGun>()].Value;

            // Центр игрока (рука)
            Vector2 drawPos = player.MountedCenter - Main.screenPosition;

            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

            // Целимся туда, куда смотрит рука игрока (см. AI: itemRotation выставляется по targetPos)
            Vector2 dirToMouse = new Vector2(
                (float)Math.Cos(player.itemRotation) * player.direction,
                (float)Math.Sin(player.itemRotation) * player.direction);
            if (dirToMouse.LengthSquared() < 0.0001f)
                dirToMouse = Vector2.UnitX * player.direction;
            float rotation = dirToMouse.ToRotation();

            // Спрайт нарисован «горизонтально» (барабан вправо), поэтому при взгляде
            // влево отзеркаливаем по вертикали, чтобы рукоять оставалась внизу.
            SpriteEffects effects = player.direction == 1
                ? SpriteEffects.None
                : SpriteEffects.FlipVertically;

            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, rotation, origin, player.GetAdjustedItemScale(player.HeldItem), effects, 0);

            return false; // Мы не рисуем сам снаряд, только частицы в DrawBeam
        }
    }
}
