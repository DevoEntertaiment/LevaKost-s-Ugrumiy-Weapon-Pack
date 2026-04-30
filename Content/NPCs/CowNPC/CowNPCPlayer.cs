using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.NPCs
{
	/// <summary>
	/// Per-player милк-кулдаун для <see cref="CowNPC"/>. Жил раньше как поле
	/// в ModNPC, но в MP это ломалось: NPC.AI/PostAI крутится только на сервере,
	/// а OnChatButtonClicked срабатывает на клиенте, открывшем диалог, — серверный
	/// таймер никто не выставлял, клиентский никто не декрементил.
	/// </summary>
	public class CowNPCPlayer : ModPlayer
	{
		public int MilkCooldown;

		public override void PostUpdate()
		{
			if (MilkCooldown > 0)
				MilkCooldown--;
		}
	}
}
