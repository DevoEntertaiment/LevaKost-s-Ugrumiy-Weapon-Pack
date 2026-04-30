using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Items.Consumables
{
    /// <summary>
    /// Колокольчик для призыва коровы. Имеет кулдаун, чтобы корову нельзя было заспамить.
    /// </summary>
    public class CowBell : ModItem
    {
        // 30 seconds cooldown between cow summons.
        private const int CooldownTicks = 60 * 30;

        public override string Texture => "Terraria/Images/Item_" + ItemID.Bell;

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item35;
            Item.consumable = false;
            Item.maxStack = 1;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.HasBuff(ModContent.BuffType<Buffs.CowBellCooldown>()))
            {
                if (player.whoAmI == Main.myPlayer)
                {
                    Main.NewText(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Misc.CowBellCooldownMsg"), 255, 200, 100);
                }
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            // NPC spawning must happen on the server (or in singleplayer). On a
            // MultiplayerClient, NPC.NewNPC creates a local-only NPC that never
            // gets synced. ExampleMod (Content/NPCs/ExampleTravelingMerchant.cs)
            // and Calamity follow the same `Main.netMode != MultiplayerClient`
            // pattern.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int npcType = ModContent.NPCType<NPCs.CowNPC>();

                int npcIndex = NPC.NewNPC(
                    player.GetSource_ItemUse(Item),
                    (int)player.Center.X + 100,
                    (int)player.Center.Y,
                    npcType
                );

                if (Main.netMode == NetmodeID.Server && npcIndex < Main.maxNPCs)
                {
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcIndex);
                }
            }

            if (player.whoAmI == Main.myPlayer)
            {
                Main.NewText(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Misc.CowBellSpawn"), 100, 255, 100);
            }

            // Apply cooldown so the cow can't be re-summoned immediately.
            player.AddBuff(ModContent.BuffType<Buffs.CowBellCooldown>(), CooldownTicks);
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Hay, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
