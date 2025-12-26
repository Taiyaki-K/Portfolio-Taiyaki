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
        [SerializeField] private GameObject doragonDeathEffect;
        private bool playerDead = false;

        private int currentHealth;

        private IDestructible myDestructible;

        private void Awake()
        {
            myDestructible = GetComponentInParent<IDestructible>();
            maxHealth = GameManager.Instance.dragonHp;
        }
        private void Start()
        {
            currentHealth = maxHealth;
            EventBus.PublishDragonHpInitialized(maxHealth);
        }

        private void OnEnable()
        {
            EventBus.OnPlayerDied += PlayerSEISI;
        }
        private void OnDisable()
        {
            EventBus.OnPlayerDied -= PlayerSEISI;
        }

        // ★ IDamageable の実装
        public void TakeDamage(int amount)
        {
            if (playerDead)
                return;
            currentHealth -= amount;
            EventBus.PublishTakeDragonDamage(currentHealth);
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        private void PlayerSEISI()
        {
            playerDead = true;
        }

        private void Die()
        {
            EventBus.PublishDragonDied();
            Instantiate(doragonDeathEffect, new Vector3(0, 0, 47), Quaternion.identity);
            // (例) 死亡エフェクトを再生
            // ...

            // 死亡イベントを発行（スコア計算などに使える）
            // EventBus.PublishEnemyDied(this); 

            // 自分自身を破棄する
            myDestructible.DestroySelf();
        }
    }
}