using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LK_Ugrumiy_WP.Content.Mounts;

namespace LK_Ugrumiy_WP.Content.Items
{
    public class OppressorMK2Item : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item79; // Звук маунта
            Item.noMelee = true;
            Item.mountType = ModContent.MountType<OppressorMK2>();
        }

        public override void AddRecipes()
        {
            // Простой рецепт для теста, потом можно поменять
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
