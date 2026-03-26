using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Items.Consumables
{
	/// <summary>
	/// Молоко, полученное от "коровы" (которая бык).
	/// Восстанавливает HP и даёт небольшой бафф.
	/// </summary>
	public class CowMilk : ModItem
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.MilkCarton;

		public override void SetDefaults()
		{
			Item.consumable = true;
			Item.maxStack = 99;
			Item.width = 20;
			Item.height = 26;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 10);
			Item.UseSound = SoundID.Item3; // звук питья
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.healLife = 50;
			Item.potion = false; // нет кулдауна зелий
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				// Бафф Well Fed на 3 минуты
				player.AddBuff(BuffID.WellFed, 10800);

				// Белые частицы при использовании
				for (int i = 0; i < 10; i++)
				{
					Dust.NewDust(
						player.position, player.width, player.height,
						DustID.TintableDust,
						0f, -1f,
						200,
						new Microsoft.Xna.Framework.Color(255, 255, 255),
						1f
					);
				}
			}

			return true;
		}
	}
}