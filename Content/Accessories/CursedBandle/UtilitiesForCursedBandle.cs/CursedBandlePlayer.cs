using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameInput;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace LK_Ugrumiy_WP.Content.Accessories
{
	public class CursedBandlePlayer : ModPlayer
	{
		public bool isCursed;
		public int cursedItemType;

		/// <summary>Позиция цели ИИ — для подмены курсора мыши.</summary>
		public Vector2 aiTargetPosition;

		private readonly CursedBandleAI ai = new CursedBandleAI();

		/// <summary>
		/// Запоминаем, в каком слоте стоит аксессуар и сам предмет,
		/// чтобы принудительно возвращать при попытке снять.
		/// </summary>
		private int cursedSlotIndex = -1;
		private Item cursedItemBackup;

		public override void ResetEffects()
		{
			isCursed = false;
		}

		public override void SetControls()
		{
			if (!isCursed)
				return;

			// 1. Полностью обнуляем пользовательский ввод
			Player.controlLeft = false;
			Player.controlRight = false;
			Player.controlUp = false;
			Player.controlDown = false;
			Player.controlJump = false;
			Player.controlUseItem = false;
			Player.controlUseTile = false;
			Player.controlThrow = false;
			Player.controlSmart = false;
			Player.controlMount = false;
			Player.controlHook = false;
			Player.controlTorch = false;
			Player.controlQuickHeal = false;
			Player.controlQuickMana = false;
			Player.controlInv = false;

			// 2. Закрываем любые открытые интерфейсы
			if (Player.chest != -1)
				Player.chest = -1;
			Main.playerInventory = false;

			// 3. ИИ задаёт свои управляющие сигналы
			ai.Update(Player);

			// 4. Подменяем позицию мыши на позицию цели ИИ,
			//    чтобы оружие било в сторону врага, а не курсора
			if (aiTargetPosition != Vector2.Zero)
			{
				Vector2 screenPos = aiTargetPosition - Main.screenPosition;
				Main.mouseX = (int)screenPos.X;
				Main.mouseY = (int)screenPos.Y;
			}
		}

		/// <summary>
		/// PreUpdate: принудительно возвращаем аксессуар в слот,
		/// если игрок каким-то образом его вытащил.
		/// </summary>
		public override void PreUpdate()
		{
			if (cursedSlotIndex < 0 || cursedItemType == 0)
				return;

			// Проверяем — если слот вдруг пуст, а проклятие было активно
			if (Player.armor[cursedSlotIndex].type != cursedItemType && cursedItemBackup != null)
			{
				// Ищем аксессуар в инвентаре (игрок мог его туда перетащить)
				for (int i = 0; i < Player.inventory.Length; i++)
				{
					if (Player.inventory[i].type == cursedItemType)
					{
						Player.armor[cursedSlotIndex] = Player.inventory[i].Clone();
						Player.inventory[i].TurnToAir();
						return;
					}
				}

				// Ищем на курсоре мыши
				if (Main.mouseItem.type == cursedItemType)
				{
					Player.armor[cursedSlotIndex] = Main.mouseItem.Clone();
					Main.mouseItem.TurnToAir();
					return;
				}

				// Если вообще нигде нет — восстанавливаем из бэкапа
				Player.armor[cursedSlotIndex] = cursedItemBackup.Clone();
			}
		}

		/// <summary>
		/// PostUpdate: запоминаем слот и бэкап аксессуара каждый тик.
		/// </summary>
		public override void PostUpdate()
		{
			if (!isCursed)
			{
				cursedSlotIndex = -1;
				cursedItemBackup = null;
				return;
			}

			// Находим и запоминаем слот с Cursed Bandle
			for (int i = 3; i < 3 + Player.extraAccessorySlots + 7; i++)
			{
				if (Player.armor[i].type == cursedItemType)
				{
					cursedSlotIndex = i;
					cursedItemBackup = Player.armor[i].Clone();
					break;
				}
			}
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			if (cursedSlotIndex >= 0 && cursedSlotIndex < Player.armor.Length
				&& Player.armor[cursedSlotIndex].type == cursedItemType)
			{
				Player.QuickSpawnItem(Player.GetSource_Death(), Player.armor[cursedSlotIndex], Player.armor[cursedSlotIndex].stack);
				Player.armor[cursedSlotIndex].TurnToAir();
			}

			isCursed = false;
			cursedSlotIndex = -1;
			cursedItemBackup = null;
			ai.Reset();
		}
	}
}