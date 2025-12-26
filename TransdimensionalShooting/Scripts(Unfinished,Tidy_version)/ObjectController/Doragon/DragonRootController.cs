using UnityEngine;

namespace Taiyaki
{
    public enum DragonState
    {
        Idle,
        Move,
        HomingAttack,
        Cooldown,
        Dead
    }

    public class DragonRootController : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 300;
        private int currentHealth;

        private IDestructible myDestructible;

        private void Awake()
        {
            myDestructible = GetComponentInParent<IDestructible>();
        }
        private void Start()
        {
            currentHealth = maxHealth;
            EventBus.PublishDragonHpInitialized(maxHealth);
        }

        // ★ IDamageable の実装
        public void TakeDamage(int amount)
        {
            currentHealth -= amount;
            EventBus.PublishTakeDragonDamage(currentHealth);
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            // (例) 死亡エフェクトを再生
            // ...

            // 死亡イベントを発行（スコア計算などに使える）
            // EventBus.PublishEnemyDied(this); 

            // 自分自身を破棄する
            myDestructible.DestroySelf();
        }
    }
}