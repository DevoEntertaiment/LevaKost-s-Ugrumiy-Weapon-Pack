using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Items.Weapons
{
    public class GravityGun : ModItem
    {
        public override string Texture => "LK_Ugrumiy_WP/Content/Items/Weapons/GravityGun/GravityGun";

        public override void SetDefaults()
        {
            Item.damage = 0;
            Item.DamageType = DamageClass.Magic;
            Item.width = 64;
            Item.height = 28;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true; // Скрываем стандартную отрисовку, нарисуем сами в снаряде
            Item.knockBack = 0;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item8; // Звук магического выстрела
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.GravityGunBeam>();
            Item.shootSpeed = 1f;
            Item.channel = true; // Важно для удержания луча
        }

        public override bool CanUseItem(Player player)
        {
            // Запрещаем использовать, если уже есть активный луч
            return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

    }
}
