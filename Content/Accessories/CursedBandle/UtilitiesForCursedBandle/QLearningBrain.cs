using System;
using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Accessories
{
	/// <summary>
	/// Q-Learning: настоящий ИИ с обучением с подкреплением.
	/// Q(s,a) = Q(s,a) + α * (reward + γ * max(Q(s',a')) - Q(s,a))
	/// </summary>
	public class QLearningBrain
	{
		// === Действия ИИ ===
		public enum AIAction
		{
			MoveLeft,
			MoveRight,
			Jump,
			JumpLeft,
			JumpRight,
			Attack,
			AttackApproach,
			Flee,
			Heal,
			Idle,
			DodgeJump,
			BuildBelow,
			COUNT // всегда последний — количество действий
		}

		// === Дискретизация состояния ===
		public struct StateKey : IEquatable<StateKey>
		{
			public int hpLevel;          // 0=критично, 1=низко, 2=среднее, 3=высоко, 4=полно
			public int enemyDistance;     // 0=нет, 1=вплотную, 2=близко, 3=средне, 4=далеко
			public int enemyDirection;   // 0=нет, 1=слева, 2=справа, 3=сверху, 4=снизу
			public int isBlocked;        // 0=нет, 1=стена
			public int isOnGround;       // 0=в воздухе, 1=на земле
			public int enemyHpLevel;     // 0=нет, 1=низко, 2=среднее, 3=высоко
			public int hasRangedWeapon;  // 0=нет, 1=да
			public int enemyCount;       // 0=нет, 1=один, 2=несколько

			public override int GetHashCode()
			{
				unchecked
				{
					int hash = 17;
					hash = hash * 31 + hpLevel;
					hash = hash * 31 + enemyDistance;
					hash = hash * 31 + enemyDirection;
					hash = hash * 31 + isBlocked;
					hash = hash * 31 + isOnGround;
					hash = hash * 31 + enemyHpLevel;
					hash = hash * 31 + hasRangedWeapon;
					hash = hash * 31 + enemyCount;
					return hash;
				}
			}

			public bool Equals(StateKey other)
			{
				return hpLevel == other.hpLevel
					&& enemyDistance == other.enemyDistance
					&& enemyDirection == other.enemyDirection
					&& isBlocked == other.isBlocked
					&& isOnGround == other.isOnGround
					&& enemyHpLevel == other.enemyHpLevel
					&& hasRangedWeapon == other.hasRangedWeapon
					&& enemyCount == other.enemyCount;
			}

			public override bool Equals(object obj) => obj is StateKey k && Equals(k);
		}

		// === Гиперпараметры ===
		private float learningRate = 0.15f;   // α — скорость обучения
		private float discountFactor = 0.95f; // γ — важность будущих наград
		private float epsilon = 0.2f;         // ε — вероятность случайного действия (исследование)
		private float epsilonDecay = 0.9999f; // Уменьшение ε со временем (меньше случайностей)
		private float minEpsilon = 0.05f;     // Минимум ε

		// === Q-таблица ===
		private readonly Dictionary<(StateKey, AIAction), float> qTable = new Dictionary<(StateKey, AIAction), float>();
		private readonly Random rng = new Random();

		// === Отслеживание прогресса ===
		public int totalKills;
		public int totalDeaths;
		public int totalDamageDealt;
		public int totalDamageTaken;
		public int episodeCount;

		private StateKey previousState;
		private AIAction previousAction;
		private bool hasPreviousState;

		// === Награды ===
		private const float RewardKill = 100f;
		private const float RewardDamageDealt = 5f;
		private const float RewardDodge = 15f;
		private const float RewardHeal = 10f;
		private const float PenaltyDamageTaken = -20f;
		private const float PenaltyDeath = -200f;
		private const float PenaltyStuck = -5f;
		private const float RewardExplore = 1f;
		private const float PenaltyIdle = -1f;

		/// <summary>
		/// Наблюдение: преобразует состояние мира в дискретный StateKey.
		/// </summary>
		public StateKey Observe(Player player, NPC nearestEnemy, int enemyCountNearby)
		{
			float hpRatio = (float)player.statLife / player.statLifeMax2;

			var s = new StateKey();

			// HP уровень
			if (hpRatio > 0.9f) s.hpLevel = 4;
			else if (hpRatio > 0.6f) s.hpLevel = 3;
			else if (hpRatio > 0.35f) s.hpLevel = 2;
			else if (hpRatio > 0.15f) s.hpLevel = 1;
			else s.hpLevel = 0;

			// Враг
			if (nearestEnemy != null && nearestEnemy.active)
			{
				float dist = Microsoft.Xna.Framework.Vector2.Distance(player.Center, nearestEnemy.Center);
				float dx = nearestEnemy.Center.X - player.Center.X;
				float dy = nearestEnemy.Center.Y - player.Center.Y;

				if (dist < 80f) s.enemyDistance = 1;
				else if (dist < 250f) s.enemyDistance = 2;
				else if (dist < 500f) s.enemyDistance = 3;
				else s.enemyDistance = 4;

				if (Math.Abs(dy) > Math.Abs(dx))
					s.enemyDirection = dy < 0 ? 3 : 4; // сверху / снизу
				else
					s.enemyDirection = dx < 0 ? 1 : 2; // слева / справа

				float enemyHpRatio = (float)nearestEnemy.life / nearestEnemy.lifeMax;
				if (enemyHpRatio > 0.6f) s.enemyHpLevel = 3;
				else if (enemyHpRatio > 0.3f) s.enemyHpLevel = 2;
				else s.enemyHpLevel = 1;
			}
			else
			{
				s.enemyDistance = 0;
				s.enemyDirection = 0;
				s.enemyHpLevel = 0;
			}

			s.isBlocked = (Math.Abs(player.velocity.X) < 0.5f && player.velocity.Y == 0f) ? 1 : 0;
			s.isOnGround = player.velocity.Y == 0f ? 1 : 0;

			// Проверяем наличие рэнж-оружия в хотбаре
			s.hasRangedWeapon = 0;
			for (int i = 0; i < 10; i++)
			{
				if (player.inventory[i].active && player.inventory[i].DamageType == Terraria.ModLoader.DamageClass.Ranged)
				{
					s.hasRangedWeapon = 1;
					break;
				}
			}

			s.enemyCount = enemyCountNearby <= 0 ? 0 : (enemyCountNearby == 1 ? 1 : 2);

			return s;
		}

		/// <summary>
		/// Выбор действия: ε-greedy стратегия.
		/// </summary>
		public AIAction ChooseAction(StateKey state)
		{
			// Исследование: случайное действие
			if (rng.NextDouble() < epsilon)
				return (AIAction)rng.Next((int)AIAction.COUNT);

			// Эксплуатация: лучшее известное действие
			return GetBestAction(state);
		}

		/// <summary>
		/// Обучение: обновляет Q-значение на основе полученной награды.
		/// </summary>
		public void Learn(StateKey newState, float reward)
		{
			if (!hasPreviousState)
				return;

			float oldQ = GetQ(previousState, previousAction);
			float maxFutureQ = GetMaxQ(newState);

			// Формула Q-Learning:
			// Q(s,a) = Q(s,a) + α * (reward + γ * max(Q(s',a')) - Q(s,a))
			float newQ = oldQ + learningRate * (reward + discountFactor * maxFutureQ - oldQ);
			SetQ(previousState, previousAction, newQ);

			// Уменьшаем исследование со временем
			epsilon = Math.Max(minEpsilon, epsilon * epsilonDecay);
		}

		/// <summary>
		/// Запоминает текущее состояние и действие для следующего шага обучения.
		/// </summary>
		public void Remember(StateKey state, AIAction action)
		{
			previousState = state;
			previousAction = action;
			hasPreviousState = true;
		}

		/// <summary>
		/// Штраф за смерть — сильный отрицательный сигнал.
		/// </summary>
		public void OnDeath()
		{
			if (hasPreviousState)
			{
				float oldQ = GetQ(previousState, previousAction);
				float newQ = oldQ + learningRate * (PenaltyDeath - oldQ);
				SetQ(previousState, previousAction, newQ);
			}

			totalDeaths++;
			episodeCount++;
			hasPreviousState = false;
		}

		/// <summary>
		/// Рассчитывает награду за текущий тик.
		/// </summary>
		public float CalculateReward(Player player, NPC enemy,
			int prevHp, int prevEnemyHp, bool enemyKilled, bool wasHit, bool isStuck)
		{
			float reward = 0f;

			if (enemyKilled)
			{
				reward += RewardKill;
				totalKills++;
			}

			if (enemy != null && prevEnemyHp > 0 && enemy.life < prevEnemyHp)
			{
				int dmgDealt = prevEnemyHp - enemy.life;
				reward += RewardDamageDealt * (dmgDealt / 10f);
				totalDamageDealt += dmgDealt;
			}

			if (wasHit)
			{
				int dmgTaken = prevHp - player.statLife;
				reward += PenaltyDamageTaken * (dmgTaken / 20f);
				totalDamageTaken += dmgTaken;
			}
			else if (enemy != null)
			{
				// Не получили урон при наличии врага рядом — уклонение
				float dist = Microsoft.Xna.Framework.Vector2.Distance(player.Center, enemy.Center);
				if (dist < 200f)
					reward += RewardDodge * 0.1f;
			}

			if (isStuck)
				reward += PenaltyStuck;

			if (player.velocity.X != 0)
				reward += RewardExplore * 0.1f;

			return reward;
		}

		// === Сохранение / загрузка Q-таблицы ===

		public void Save(string modPath)
		{
			string path = Path.Combine(modPath, "cursed_bandle_brain.dat");
			try
			{
				using (var writer = new BinaryWriter(File.Open(path, FileMode.Create)))
				{
					writer.Write(epsilon);
					writer.Write(totalKills);
					writer.Write(totalDeaths);
					writer.Write(totalDamageDealt);
					writer.Write(totalDamageTaken);
					writer.Write(episodeCount);
					writer.Write(qTable.Count);

					foreach (var kvp in qTable)
					{
						var (state, action) = kvp.Key;
						writer.Write(state.hpLevel);
						writer.Write(state.enemyDistance);
						writer.Write(state.enemyDirection);
						writer.Write(state.isBlocked);
						writer.Write(state.isOnGround);
						writer.Write(state.enemyHpLevel);
						writer.Write(state.hasRangedWeapon);
						writer.Write(state.enemyCount);
						writer.Write((int)action);
						writer.Write(kvp.Value);
					}
				}
			}
			catch { }
		}

		public void Load(string modPath)
		{
			string path = Path.Combine(modPath, "cursed_bandle_brain.dat");
			if (!File.Exists(path)) return;

			try
			{
				using (var reader = new BinaryReader(File.Open(path, FileMode.Open)))
				{
					epsilon = reader.ReadSingle();
					totalKills = reader.ReadInt32();
					totalDeaths = reader.ReadInt32();
					totalDamageDealt = reader.ReadInt32();
					totalDamageTaken = reader.ReadInt32();
					episodeCount = reader.ReadInt32();
					int count = reader.ReadInt32();

					qTable.Clear();
					for (int i = 0; i < count; i++)
					{
						var state = new StateKey
						{
							hpLevel = reader.ReadInt32(),
							enemyDistance = reader.ReadInt32(),
							enemyDirection = reader.ReadInt32(),
							isBlocked = reader.ReadInt32(),
							isOnGround = reader.ReadInt32(),
							enemyHpLevel = reader.ReadInt32(),
							hasRangedWeapon = reader.ReadInt32(),
							enemyCount = reader.ReadInt32()
						};
						var action = (AIAction)reader.ReadInt32();
						float value = reader.ReadSingle();
						qTable[(state, action)] = value;
					}
				}
			}
			catch { }
		}

		// === Вспомогательные методы Q-таблицы ===

		private float GetQ(StateKey state, AIAction action)
		{
			return qTable.TryGetValue((state, action), out float val) ? val : 0f;
		}

		private void SetQ(StateKey state, AIAction action, float value)
		{
			qTable[(state, action)] = value;
		}

		private float GetMaxQ(StateKey state)
		{
			float max = float.MinValue;
			for (int a = 0; a < (int)AIAction.COUNT; a++)
			{
				float q = GetQ(state, (AIAction)a);
				if (q > max) max = q;
			}
			return max == float.MinValue ? 0f : max;
		}

		private AIAction GetBestAction(StateKey state)
		{
			AIAction best = AIAction.Idle;
			float bestQ = float.MinValue;

			for (int a = 0; a < (int)AIAction.COUNT; a++)
			{
				float q = GetQ(state, (AIAction)a);
				if (q > bestQ)
				{
					bestQ = q;
					best = (AIAction)a;
				}
			}

			return best;
		}

		public void Reset()
		{
			hasPreviousState = false;
		}

		/// <summary>Текущий уровень исследования (для отладки).</summary>
		public float Epsilon => epsilon;

		/// <summary>Размер Q-таблицы (сколько комбинаций изучено).</summary>
		public int KnowledgeSize => qTable.Count;
	}
}