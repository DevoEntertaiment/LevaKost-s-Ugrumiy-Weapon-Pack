using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace LK_Ugrumiy_WP.Content.Mounts.OppressorMK2
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
            // Звук посадки на маунт играет наш OppressorMK2.SetMount (EngineStart.wav).
            // UseSound = null, чтобы Player.QuickMount не накладывал поверх ваниловый Item79.
            Item.UseSound = null;
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
