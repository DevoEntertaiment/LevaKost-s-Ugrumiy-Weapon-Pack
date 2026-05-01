using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LK_Ugrumiy_WP.Content.Items.Consumables
{
	public class FatPlayer : ModPlayer
	{
		public float FatLevel;
		public const float MaxFat = 100f;

		public int FatStage => FatLevel switch
		{
			< 10f => 0,
			< 30f => 1,
			< 55f => 2,
			< 80f => 3,
			_ => 4
		};

		private const float DeathFatLoss = 25f;
		private float burnAccumulator;

		public void AddFat(float amount)
		{
			FatLevel = Math.Min(MaxFat, FatLevel + amount);
			burnAccumulator = 0f;
		}

		public void RemoveFat(float amount)
		{
			FatLevel = Math.Max(0f, FatLevel - amount);
		}

		public void ResetFat()
		{
			FatLevel = 0f;
			burnAccumulator = 0f;
		}

		public override void ResetEffects()
		{
			ApplyFatEffects();
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			if (FatLevel > 0)
			{
				FatLevel = Math.Max(0f, FatLevel - DeathFatLoss);
				// Kill() runs on every client for every player's death — without the
				// myPlayer guard the local screen would show the fat-loss message for
				// every remote player who died.
				if (Player.whoAmI == Main.myPlayer)
				{
					string msg = Language.GetTextValue("Mods.LK_Ugrumiy_WP.Misc.FatLostOnDeath", (int)DeathFatLoss);
					Main.NewText(msg, 200, 200, 100);
				}
			}
		}

		private float previousVelocityY;
		private bool isFalling;
		private int customFallStartY; // Наша собственная точка начала падения (в тайлах)

		public override void PreUpdate()
		{
			previousVelocityY = Player.velocity.Y;
		}

		public override void PostUpdate()
		{
			// Отслеживаем начало падения вручную (Terraria сбрасывает fallStart когда noFallDmg = true)
			if (FatStage == 4)
			{
				if (Player.velocity.Y > 0f && !isFalling)
				{
					// Игрок начал падать — запоминаем начальную точку
					isFalling = true;
					customFallStartY = (int)(Player.position.Y / 16f);
				}
				else if (Player.velocity.Y == 0f && isFalling)
				{
					// Игрок приземлился
					int currentTileY = (int)(Player.position.Y / 16f);
					int fallDist = currentTileY - customFallStartY;
					isFalling = false;

					if (fallDist >= 34)
					{
						BreakBlocksUnderneath();
					}
				}
				else if (Player.velocity.Y <= 0f)
				{
					isFalling = false;
				}
			}

			if (FatLevel <= 0) return;

			float burnRate = 0f;

			// Базовое сжигание: всегда тикает (очень медленно)
			burnRate += 0.001f; // ~0.06/сек, ~3.6/мин

			// На маунте жир не сжигается активно — игрок не бегает сам
			if (!Player.mount.Active)
			{
				// Бег: быстро сжигает
				if (Math.Abs(Player.velocity.X) > 2f && Player.velocity.Y == 0f)
				{
					burnRate += 0.015f; // ~0.48/сек, ~28.8/мин
				}

				// Спринт (быстрый бег)
				if (Math.Abs(Player.velocity.X) > 5f)
				{
					burnRate += 0.012f; // ещё быстрее
				}

				// Прыжки
				if (Player.jump > 0 || Player.velocity.Y < -2f)
				{
					burnRate += 0.005f;
				}

				// Бой (если игрок атакует)
				if (Player.itemAnimation > 0)
				{
					burnRate += 0.004f;
				}

				// Плавание
				if (Player.wet && !Player.lavaWet)
				{
					burnRate += 0.01f;
				}
			}

			// Накапливаем и применяем
			burnAccumulator += burnRate;

			if (burnAccumulator >= 1f)
			{
				float toLose = (float)Math.Floor(burnAccumulator);
				FatLevel = Math.Max(0f, FatLevel - toLose);
				burnAccumulator -= toLose;

				// PostUpdate runs for every player on every client; without the myPlayer
				// guard the local chat would print burn-progress messages from every
				// remote player who is also losing fat.
				if (Player.whoAmI == Main.myPlayer
					&& FatLevel > 0
					&& (int)(FatLevel + toLose) / 10 != (int)FatLevel / 10)
				{
					string msg = Language.GetTextValue("Mods.LK_Ugrumiy_WP.Misc.BurningFat", (int)FatLevel, (int)MaxFat);
					Main.NewText(msg, 150, 255, 150);
				}
			}
		}

		private void BreakBlocksUnderneath()
		{
			// PostUpdate runs for every player on every client. Without the myPlayer
			// guard each remote client would also rip the tiles locally, desyncing
			// the world. Only the owning client kills the tiles and broadcasts a
			// TileManipulation packet so the server / other clients update.
			if (Player.whoAmI != Main.myPlayer)
				return;

			int playerTileX = (int)(Player.Center.X / 16f);
			int playerTileY = (int)((Player.position.Y + Player.height + 2f) / 16f); // Плитка прямо под ногами
			int depth = Main.rand.Next(2, 4); // 2 или 3 блока в глубину
			bool playedSound = false;

			for (int x = playerTileX - 1; x <= playerTileX + 1; x++)
			{
				for (int y = playerTileY; y < playerTileY + depth; y++)
				{
					if (!WorldGen.InWorld(x, y))
						continue;

					Tile tile = Main.tile[x, y];
					// Проверяем, что есть блок и он твердый
					if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType])
					{
						// Уничтожаем блок
						WorldGen.KillTile(x, y, fail: false, effectOnly: false, noItem: false);
						if (!tile.HasTile)
						{
							playedSound = true;
							if (Main.netMode == NetmodeID.MultiplayerClient)
							{
								NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, x, y);
							}
						}
					}
				}
			}

			if (playedSound)
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Player.Center);
				for (int i = 0; i < 20; i++)
				{
					int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
					Main.dust[d].velocity *= 2f;
				}
			}
		}

		private void ApplyFatEffects()
		{
			if (FatLevel <= 0) return;

			float ratio = FatLevel / MaxFat;

			Player.moveSpeed *= Math.Max(0.2f, 1f - (ratio * 0.4f));
			Player.jumpSpeedBoost -= ratio * 2f;

			if (FatLevel > 50f)
				Player.GetAttackSpeed(DamageClass.Generic) *= Math.Max(0.5f, 1f - ratio * 0.15f);

			Player.statLifeMax2 += (int)(ratio * 60f);
			Player.statDefense += (int)(ratio * 10f);
			Player.noKnockback = FatLevel >= 80f;

			if (FatLevel >= 90f)
				Player.lifeRegen = Math.Max(0, Player.lifeRegen - 4);

			if (FatStage == 4)
			{
				Player.noFallDmg = true; // Отключаем урон от падения
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag["fatLevel"] = FatLevel;
			tag["burnAccumulator"] = burnAccumulator;
		}

		public override void LoadData(TagCompound tag)
		{
			FatLevel = tag.GetFloat("fatLevel");
			burnAccumulator = tag.GetFloat("burnAccumulator");
		}
	}
}