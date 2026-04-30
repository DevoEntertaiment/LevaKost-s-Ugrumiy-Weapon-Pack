using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Items.Consumables
{
	public class FatPlayerScale : ModPlayer
	{
		internal float scaleX = 1f;
		internal float scaleY = 1f;



		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			var fp = drawInfo.drawPlayer.GetModPlayer<FatPlayer>();
			if (fp.FatLevel <= 5f) return;

			// Не расширяем модельку, когда игрок на маунте — иначе растягивается спрайт маунта
			// if (drawInfo.drawPlayer.mount.Active) return; // Убрано, чтобы игрок оставался толстым на маунте

			float ratio = fp.FatLevel / FatPlayer.MaxFat;

			float sx = 1f + ratio * 0.35f;
			float sy = 1f + ratio * 0.1f;

			drawInfo.Position.X -= (sx - 1f) * drawInfo.drawPlayer.width * 0.5f;

			drawInfo.drawPlayer.GetModPlayer<FatPlayerScale>().scaleX = sx;
			drawInfo.drawPlayer.GetModPlayer<FatPlayerScale>().scaleY = sy;
		}
	}

	public class FatDrawLayer : PlayerDrawLayer
	{
		public override Position GetDefaultPosition()
		{
			// AfterParent(HeldItem) — один из последних слоёв отрисовки
			return new AfterParent(PlayerDrawLayers.HeldItem);
		}

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
		{
			// Скрываем fat-эффекты на маунте, чтобы не масштабировать спрайт маунта
			// if (drawInfo.drawPlayer.mount.Active) return false; // Убрано, чтобы эффекты рисовались на маунте
			return drawInfo.drawPlayer.GetModPlayer<FatPlayer>().FatLevel > 5f;
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			var fp = drawInfo.drawPlayer.GetModPlayer<FatPlayer>();
			var fatScale = drawInfo.drawPlayer.GetModPlayer<FatPlayerScale>();

			if (fp.FatLevel <= 5f) return;

			float sX = fatScale.scaleX;
			float sY = fatScale.scaleY;

			Vector2 feetCenter = drawInfo.Position - Main.screenPosition
				+ new Vector2(drawInfo.drawPlayer.width / 2f, drawInfo.drawPlayer.height);

			bool onMount = drawInfo.drawPlayer.mount.Active;
			ModMount modMount = onMount ? MountLoader.GetMount(drawInfo.drawPlayer.mount.Type) : null;

			for (int i = 0; i < drawInfo.DrawDataCache.Count; i++)
			{
				DrawData data = drawInfo.DrawDataCache[i];

				if (onMount)
				{
					bool isMountTexture = false;
					if (modMount != null)
					{
						if (data.texture == modMount.MountData.backTexture?.Value ||
							data.texture == modMount.MountData.backTextureExtra?.Value ||
							data.texture == modMount.MountData.backTextureGlow?.Value ||
							data.texture == modMount.MountData.frontTexture?.Value ||
							data.texture == modMount.MountData.frontTextureExtra?.Value ||
							data.texture == modMount.MountData.frontTextureGlow?.Value)
						{
							isMountTexture = true;
						}
					}
					else
					{
						if (data.texture != null && data.texture.Name != null && data.texture.Name.StartsWith("Images/Mount_"))
						{
							isMountTexture = true;
						}
					}

					if (data.texture != null && data.texture.Name != null && data.texture.Name.Contains("OppressorMK2"))
					{
						isMountTexture = true;
					}
					
					Texture2D boostTex = null;
					try {
						boostTex = ModContent.Request<Texture2D>("LK_Ugrumiy_WP/Content/Mounts/OppressorMK2/OppressorMK2Boost_Back").Value;
					} catch {}

					if (data.texture != null && boostTex != null && data.texture == boostTex)
					{
						isMountTexture = true;
					}

					if (isMountTexture) continue;
				}

				Vector2 offset = data.position - feetCenter;
				offset.X *= sX;
				offset.Y *= sY;
				data.position = feetCenter + offset;

				if (data.scale.X != 0f && data.scale.Y != 0f)
				{
					data.scale = new Vector2(data.scale.X * sX, data.scale.Y * sY);
				}

				drawInfo.DrawDataCache[i] = data;
			}

			if (fp.FatLevel > 40f)
			{
				float ratio = fp.FatLevel / FatPlayer.MaxFat;

				// Центр живота — чуть ниже середины тела
				Vector2 bellyCenter = feetCenter - new Vector2(0f, drawInfo.drawPlayer.height * 0.42f * sY);

				// Полуоси эллипса
				float halfW = (5f + ratio * 10f) * sX;
				float halfH = (3f + ratio * 7f) * sY;
				int strips = Math.Max(4, (int)(halfH * 2)); // Количество горизонтальных полосок

				Color baseColor = Color.Lerp(
					new Color(255, 220, 190),
					new Color(240, 180, 140),
					ratio
				);
				float baseAlpha = 0.12f + ratio * 0.18f;

				// Рисуем эллипс горизонтальными полосками
				for (int row = 0; row < strips; row++)
				{
					// t от -1 до +1 (верх к низу)
					float t = (row / (float)(strips - 1)) * 2f - 1f;

					// Ширина полоски по формуле эллипса: w = halfW * sqrt(1 - t^2)
					float stripHalfW = halfW * (float)Math.Sqrt(1f - t * t);
					if (stripHalfW < 0.5f) continue;

					float yPos = bellyCenter.Y + t * halfH;
					float stripH = Math.Max(1f, (halfH * 2f) / strips + 0.5f);

					// Градиент: верхняя часть светлее (блик), нижняя темнее (тень)
					float shade = 1f - t * 0.15f; // Верх чуть ярче
					// Боковое затемнение ближе к краям
					float edgeDarken = 0.85f + 0.15f * (1f - Math.Abs(t));

					Color stripColor = new Color(
						(int)(baseColor.R * shade * edgeDarken),
						(int)(baseColor.G * shade * edgeDarken),
						(int)(baseColor.B * shade * edgeDarken)
					) * baseAlpha;

					Rectangle stripRect = new Rectangle(
						(int)(bellyCenter.X - stripHalfW),
						(int)yPos,
						(int)(stripHalfW * 2f),
						(int)stripH
					);

					drawInfo.DrawDataCache.Add(new DrawData(
						TextureAssets.MagicPixel.Value,
						stripRect,
						new Rectangle(0, 0, 1, 1),
						stripColor,
						0f,
						Vector2.Zero,
						SpriteEffects.None,
						0
					));
				}

				// Блик по центру живота (маленький светлый овал)
				if (ratio > 0.4f)
				{
					float highlightW = halfW * 0.4f;
					float highlightH = halfH * 0.3f;
					Color highlightColor = Color.White * (0.04f + ratio * 0.06f);
					
					Rectangle highlightRect = new Rectangle(
						(int)(bellyCenter.X - highlightW / 2f - 1f),
						(int)(bellyCenter.Y - highlightH - halfH * 0.1f),
						(int)highlightW,
						(int)highlightH
					);
					
					drawInfo.DrawDataCache.Add(new DrawData(
						TextureAssets.MagicPixel.Value,
						highlightRect,
						new Rectangle(0, 0, 1, 1),
						highlightColor,
						0f,
						Vector2.Zero,
						SpriteEffects.None,
						0
					));
				}
			}
		}
	}

	public class FatEffects : ModPlayer
	{
		private bool wasOnGround;

		public override void PostUpdate()
		{
			var fp = Player.GetModPlayer<FatPlayer>();
			if (fp.FatLevel < 40f)
			{
				wasOnGround = Player.velocity.Y == 0f;
				return;
			}

			float ratio = fp.FatLevel / FatPlayer.MaxFat;

			bool onGround = Player.velocity.Y == 0f;
			if (onGround && !wasOnGround && fp.FatLevel >= 60f)
			{
				int dustCount = 3 + (int)(ratio * 10);
				for (int i = 0; i < dustCount; i++)
				{
					Vector2 dustPos = Player.Bottom + new Vector2(Main.rand.Next(-12, 12), -2);
					Dust.NewDust(dustPos, 4, 4,
						Terraria.ID.DustID.Smoke,
						Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 2f),
						150, default, 0.6f + ratio * 0.4f);
				}
			}
			wasOnGround = onGround;

			if (Math.Abs(Player.velocity.X) > 3f && onGround && Main.GameUpdateCount % 8 == 0 && fp.FatLevel >= 50f)
			{
				Dust.NewDust(Player.Bottom - new Vector2(4, 0), 8, 4,
					Terraria.ID.DustID.Smoke, 0f, -0.5f, 100, default, 0.5f);
			}
		}
	}
}