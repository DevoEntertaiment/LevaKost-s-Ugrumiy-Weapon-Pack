using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;

namespace LK_Ugrumiy_WP.Content.Items.Accessories
{
    /// <summary>
    /// Шапка Джона - редкий аксессуар, выпадающий из NPC "Джон" в пещерах.
    /// Отсылка на Grizzled Jon из RDR2.
    /// </summary>
    public class JohnsHat : ModItem
    {
        // Placeholder sprite: заменить на свой спрайт в Content/Items/Accessories/JohnsHat.png
        public override string Texture => "LK_Ugrumiy_WP/Content/Items/Accessories/JohnsHat";

        public override LocalizedText Tooltip => Language.GetOrRegister(
            "Mods.LK_Ugrumiy_WP.Items.JohnsHat.Tooltip",
            () => "'A worn-out cap with a story...'\n" +
                  "+10% damage to human-type enemies\n" +
                  "+5% critical strike chance\n" +
                  "John will hunt you down if you wear this...");

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Урон по гуманоидам
            player.GetDamage(DamageClass.Generic) += 0.1f;
            // Критический шанс
            player.GetCritChance(DamageClass.Generic) += 5;

            // Помечаем игрока как "носящего шляпу Джона"
            player.GetModPlayer<JohnsHatPlayer>().wearingJohnsHat = true;
        }
    }

    public class JohnsHatPlayer : ModPlayer
    {
        public bool wearingJohnsHat;

        public override void ResetEffects()
        {
            wearingJohnsHat = false;

            // Обновляем глобальную систему
            var johnSystem = ModContent.GetInstance<JohnHatSystem>();
            johnSystem.playerWearingHat[Player.whoAmI] = wearingJohnsHat;
        }
    }
}
