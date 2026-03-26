using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace LK_Ugrumiy_WP.Content.Accessories
{
	public class CursedBandleAI
	{
		private enum AIState
		{
			Idle,
			Patrol,
			Combat,
			Flee,
			Build
		}

		private AIState state = AIState.Patrol;
		private int stateTimer;
		private int directionX = 1;
		private int thinkTimer;
		private int jumpCooldown;
		private int attackCooldown;
		private int buildCooldown;
		private readonly Random rng = new Random();

		private const float FleeHealthThreshold = 0.3f;
		private const float DetectRange = 600f;
		private const float FleeRange = 200f;
		private const int MinPatrolTicks = 60;
		private const int MaxPatrolTicks = 300;
		private const int MinIdleTicks = 30;
		private const int MaxIdleTicks = 120;

		public void Reset()
		{
			state = AIState.Patrol;
			stateTimer = 0;
			thinkTimer = 0;
		}

		public void Update(Player player)
		{
			var modPlayer = player.GetModPlayer<CursedBandlePlayer>();

			if (jumpCooldown > 0) jumpCooldown--;
			if (attackCooldown > 0) attackCooldown--;
			if (buildCooldown > 0) buildCooldown--;
			if (thinkTimer > 0) thinkTimer--;

			NPC nearestEnemy = FindNearestEnemy(player);

			// Сбрасываем цель мыши — если нет врага, целимся перед собой
			if (nearestEnemy != null)
				modPlayer.aiTargetPosition = nearestEnemy.Center;
			else
				modPlayer.aiTargetPosition = player.Center + new Vector2(player.direction * 200f, 0f);

			DecideState(player, nearestEnemy);

			switch (state)
			{
				case AIState.Idle:
					DoIdle(player);
					break;
				case AIState.Patrol:
					DoPatrol(player);
					break;
				case AIState.Combat:
					DoCombat(player, nearestEnemy);
					break;
				case AIState.Flee:
					DoFlee(player, nearestEnemy);
					break;
				case AIState.Build:
					DoBuild(player);
					break;
			}

			if (rng.Next(300) == 0 && state != AIState.Combat && state != AIState.Flee)
			{
				thinkTimer = rng.Next(MinIdleTicks, MaxIdleTicks);
				state = AIState.Idle;
			}
		}

		private void DecideState(Player player, NPC nearestEnemy)
		{
			float hpRatio = (float)player.statLife / player.statLifeMax2;

			if (nearestEnemy != null)
			{
				float dist = Vector2.Distance(player.Center, nearestEnemy.Center);

				if (hpRatio < FleeHealthThreshold && dist < FleeRange)
				{
					state = AIState.Flee;
					return;
				}

				if (dist < DetectRange)
				{
					state = AIState.Combat;
					return;
				}
			}

			if (stateTimer <= 0 && state != AIState.Idle)
			{
				if (rng.Next(10) < 2)
				{
					state = AIState.Build;
					stateTimer = rng.Next(60, 180);
				}
				else
				{
					state = AIState.Patrol;
					stateTimer = rng.Next(MinPatrolTicks, MaxPatrolTicks);
					if (rng.Next(2) == 0) directionX *= -1;
				}
			}

			stateTimer--;
		}

		private void DoIdle(Player player)
		{
			if (thinkTimer <= 0)
			{
				state = AIState.Patrol;
				stateTimer = rng.Next(MinPatrolTicks, MaxPatrolTicks);
			}
		}

		private void DoPatrol(Player player)
		{
			if (directionX > 0)
				player.controlRight = true;
			else
				player.controlLeft = true;

			if (IsBlockedHorizontally(player) || rng.Next(80) == 0)
				TryJump(player);

			if (rng.Next(200) == 0)
				directionX *= -1;
		}

		private void DoCombat(Player player, NPC target)
		{
			if (target == null || !target.active)
			{
				state = AIState.Patrol;
				return;
			}

			float dx = target.Center.X - player.Center.X;
			float dy = target.Center.Y - player.Center.Y;

			if (dx > 40f)
				player.controlRight = true;
			else if (dx < -40f)
				player.controlLeft = true;

			if (dy < -60f || IsBlockedHorizontally(player))
				TryJump(player);

			// Поворачиваем персонажа к врагу
			player.ChangeDir(dx > 0 ? 1 : -1);

			if (attackCooldown <= 0)
			{
				SelectBestWeapon(player);
				player.controlUseItem = true;
				attackCooldown = 10 + rng.Next(15);
			}
		}

		private void DoFlee(Player player, NPC threat)
		{
			if (threat == null || !threat.active)
			{
				state = AIState.Patrol;
				return;
			}

			float dx = threat.Center.X - player.Center.X;

			if (dx > 0)
				player.controlLeft = true;
			else
				player.controlRight = true;

			TryJump(player);
			player.controlQuickHeal = true;

			float dist = Vector2.Distance(player.Center, threat.Center);
			if (dist > DetectRange)
				state = AIState.Patrol;
		}

		private void DoBuild(Player player)
		{
			if (buildCooldown > 0) return;

			for (int i = 0; i < 50; i++)
			{
				Item item = player.inventory[i];
				if (item.active && item.createTile >= TileID.Dirt && item.stack > 0)
				{
					player.selectedItem = i;
					break;
				}
			}

			player.controlUseItem = true;
			player.controlDown = true;
			buildCooldown = 30 + rng.Next(60);

			if (stateTimer <= 0)
				state = AIState.Patrol;
		}

		private void SelectBestWeapon(Player player)
		{
			int bestSlot = 0;
			int bestDamage = 0;

			for (int i = 0; i < 10; i++)
			{
				Item item = player.inventory[i];
				if (item.active && item.damage > bestDamage && !item.accessory && item.createTile < 0)
				{
					bestDamage = item.damage;
					bestSlot = i;
				}
			}

			player.selectedItem = bestSlot;
		}

		private NPC FindNearestEnemy(Player player)
		{
			NPC closest = null;
			float closestDist = float.MaxValue;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (!npc.active || npc.friendly || npc.townNPC || npc.life <= 0)
					continue;

				float dist = Vector2.Distance(player.Center, npc.Center);
				if (dist < closestDist)
				{
					closestDist = dist;
					closest = npc;
				}
			}

			return closestDist < DetectRange * 1.5f ? closest : null;
		}

		private bool IsBlockedHorizontally(Player player)
		{
			return Math.Abs(player.velocity.X) < 0.5f && player.velocity.Y == 0f;
		}

		private void TryJump(Player player)
		{
			if (jumpCooldown <= 0)
			{
				player.controlJump = true;
				jumpCooldown = 15 + rng.Next(20);
			}
		}
	}
}