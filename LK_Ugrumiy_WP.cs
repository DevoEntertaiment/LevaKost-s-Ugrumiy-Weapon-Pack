using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LK_Ugrumiy_WP.Content.Mounts.OppressorMK2;

namespace LK_Ugrumiy_WP
{
	public class LK_Ugrumiy_WP : Mod
	{
		public enum MessageType : byte
		{
			OppressorMK2BoostSync = 0,
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			MessageType type = (MessageType)reader.ReadByte();

			switch (type)
			{
				case MessageType.OppressorMK2BoostSync:
					HandleOppressorBoostSync(reader, whoAmI);
					break;
			}
		}

		private void HandleOppressorBoostSync(BinaryReader reader, int whoAmI)
		{
			// Wire-формат:
			//   client -> server: [direction(sbyte)]                    (playerId не доверяем)
			//   server -> client: [playerId(byte)][direction(sbyte)]    (playerId выставляет сервер)
			byte playerId;
			sbyte direction;

			if (Main.netMode == NetmodeID.Server)
			{
				// Сервер игнорирует playerId из тела пакета — пишет реального отправителя whoAmI.
				// Иначе клиент мог бы заспуфить чужой ID и заставить сервер бродкастить
				// эффекты буста (звук + 45 пылинок) у произвольного игрока.
				direction = reader.ReadSByte();
				playerId = (byte)whoAmI;
			}
			else
			{
				playerId = reader.ReadByte();
				direction = reader.ReadSByte();
			}

			// Валидация направления: только ±1, иначе TriggerBoost будет no-op,
			// а сервер всё равно успеет бродкастить пакет — это просто шум на проводе.
			if (direction != 1 && direction != -1)
			{
				return;
			}

			if (Main.netMode == NetmodeID.Server)
			{
				ModPacket relay = GetPacket();
				relay.Write((byte)MessageType.OppressorMK2BoostSync);
				relay.Write(playerId);
				relay.Write(direction);
				relay.Send(-1, whoAmI);
				return;
			}

			// На клиенте — применяем эффекты на удалённого игрока.
			if (playerId == Main.myPlayer)
			{
				return;
			}

			if (playerId >= Main.maxPlayers)
			{
				return;
			}

			Player remote = Main.player[playerId];
			if (remote == null || !remote.active)
			{
				return;
			}

			// Не верим даже легитимному пакету «вслепую»: применяем эффекты только если
			// remote-игрок реально на нашем маунте — иначе спуфнуть может сам сервер.
			if (!remote.mount.Active || remote.mount.Type != ModContent.MountType<OppressorMK2>())
			{
				return;
			}

			OppressorMK2Player modPlayer = remote.GetModPlayer<OppressorMK2Player>();
			modPlayer.ApplyBoostEffectsRemote(direction);
		}
	}
}
