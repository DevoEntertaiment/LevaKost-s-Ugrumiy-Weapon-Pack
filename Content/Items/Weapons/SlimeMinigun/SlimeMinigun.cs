using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace LK_Ugrumiy_WP.Content.Items.Weapons
{
	/// <summary>
	/// Слизневая мини-акула: автоматическое оружие с высокой скорострельностью.
	/// Стреляет каплями белой слизи, как Minishark — пулями.
	/// 33% шанс не потратить выстрел (аналог экономии патронов).
	/// </summary>
	public class SlimeMinigun : ModItem
	{
		public override string Texture => "LK_Ugrumiy_WP/Content/Items/Weapons/SlimeMinigun/SlimeMinigun";

		public override void SetDefaults()
		{
			// Тип — дальнобойное оружие
			Item.DamageType = DamageClass.Ranged;
			Item.damage = 12;
			Item.knockBack = 1f;
			Item.crit = 4;

			// Быстрая стрельба как у Minishark
			Item.shootSpeed = 14f;
			Item.useAnimation = 8;
			Item.useTime = 8;
			Item.useStyle = ItemUseStyleID.Shoot;

			// Снаряд — капля слизи
			Item.shoot = ModContent.ProjectileType<Projectiles.SlimeGlob>();
			Item.useAmmo = AmmoID.None;

			// Размеры и редкость
			Item.width = 50;
			Item.height = 22;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(gold: 10);

			// Звук выстрела — мокрый хлюпающий
			Item.UseSound = SoundID.Item17;

			// Автоматическая стрельба
			Item.autoReuse = true;
			Item.noMelee = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// Случайный разброс как у Minishark
			float spreadAngle = MathHelper.ToRadians(6f);
			float angle = velocity.ToRotation() + Main.rand.NextFloat(-spreadAngle, spreadAngle);
			float speed = velocity.Length() * Main.rand.NextFloat(0.95f, 1.05f);
			Vector2 bulletVelocity = angle.ToRotationVector2() * speed;

			Projectile.NewProjectile(
				source,
				position,
				bulletVelocity,
				type,
				damage,
				knockback,
				player.whoAmI
			);

			return false;
		}

		public override bool CanConsumeAmmo(Item ammo, Player player)
		{
			// 33% шанс не потратить «заряд» (аналог экономии патронов у Minishark)
			return Main.rand.NextFloat() > 0.33f;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-8f, -2f);
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			// Смещаем точку выстрела к дулу
			Vector2 muzzleOffset = Vector2.Normalize(velocity) * 25f;
			if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
			{
				position += muzzleOffset;
			}
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Minishark, 1)
				.AddIngredient(ItemID.Gel, 80)
				.AddIngredient(ItemID.SlimeBlock, 30)
				.AddTile(TileID.Anvils)
				.Register();

			// Альтернативный рецепт без Minishark
			CreateRecipe()
				.AddIngredient(ItemID.IllegalGunParts, 1)
				.AddIngredient(ItemID.Gel, 150)
				.AddIngredient(ItemID.SlimeCrown, 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}