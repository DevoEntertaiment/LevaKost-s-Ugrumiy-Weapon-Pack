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
			byte playerId = reader.ReadByte();
			sbyte direction = reader.ReadSByte();

			// На сервере — пересылаем всем кроме отправителя.
			if (Main.netMode == NetmodeID.Server)
			{
				ModPacket relay = GetPacket();
				relay.Write((byte)MessageType.OppressorMK2BoostSync);
				relay.Write(playerId);
				relay.Write(direction);
				relay.Send(-1, whoAmI);
				return;
			}

			// На клиенте — применяем эффекты буста на удалённого игрока.
			if (playerId == Main.myPlayer)
			{
				return;
			}

			Player remote = Main.player[playerId];
			if (remote == null || !remote.active)
			{
				return;
			}

			OppressorMK2Player modPlayer = remote.GetModPlayer<OppressorMK2Player>();
			modPlayer.ApplyBoostEffectsRemote(direction);
		}
	}
}
