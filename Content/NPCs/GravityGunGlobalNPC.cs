using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace LK_Ugrumiy_WP.Content.NPCs
{
    public class GravityGunGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool thrownByGravityGun = false;
        public float lastVelocityLength = 0f;
        public int throwTimer = 0;

        public override void AI(NPC npc)
        {
            if (thrownByGravityGun)
            {
                throwTimer--;
                
                if (throwTimer <= 0)
                {
                    thrownByGravityGun = false;
                }

                float currentSpeed = npc.velocity.Length();

                // Если скорость резко упала (удар о стену или пол), и предыдущая скорость была высокой
                if (lastVelocityLength > 10f && currentSpeed < lastVelocityLength * 0.5f)
                {
                    // Вычисляем урон от удара (зависит от скорости до столкновения)
                    // Чем выше скорость, тем больше урон. При скорости 25f урон будет около 250-500 в зависимости от множителя.
                    int damage = (int)(lastVelocityLength * lastVelocityLength * 0.8f);

                    // Если это босс, можно сделать урон чуть меньше или оставить таким же
                    if (npc.boss)
                    {
                        damage = (int)(damage * 1.5f); // Боссам больнее, они тяжелые!
                    }

                    // Наносим урон
                    NPC.HitInfo hit = new NPC.HitInfo
                    {
                        Damage = damage,
                        Knockback = 0f,
                        HitDirection = 0
                    };
                    npc.StrikeNPC(hit);

                    // Визуальные и звуковые эффекты удара
                    SoundEngine.PlaySound(SoundID.Item14, npc.Center); // Звук сильного удара/взрыва
                    
                    for (int i = 0; i < 15; i++)
                    {
                        Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Smoke, 0, 0, 100, default, 1.5f);
                        d.velocity *= 2f;
                    }

                    // Сбрасываем статус броска, чтобы не получать урон несколько раз подряд
                    thrownByGravityGun = false;
                }

                lastVelocityLength = npc.velocity.Length();
            }
        }
    }
}
