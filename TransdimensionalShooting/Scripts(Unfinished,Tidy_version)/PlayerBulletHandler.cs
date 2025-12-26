using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Taiyaki
{
    /// <summary>
    /// 「プレイヤーの弾」専用の衝突・寿命ハンドラ
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerBulletHandler : MonoBehaviour
    {
        [SerializeField] private float lifetime = 10.0f;
        const int PLAYER_POWER = 1;

        private IDestructible myDestructible;
        private bool isDestroyed = false;

        private void Awake()
        {
            myDestructible = GetComponentInParent<IDestructible>();
        }

        private async void Start()
        {
            try
            {
                await UniTask.WaitForSeconds(lifetime);
                myDestructible.DestroySelf();
            }
            catch (OperationCanceledException)
            {
                // オブジェクトが破棄された場合、Delayがキャンセルされてここに来る。
                // これは正常な動作なので、エラーログを出す必要はない。
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (isDestroyed) return;

            // ★ この弾は「Enemy」や「Breakable」に興味がある
            IDamageable damageableTarget = collision.gameObject.GetComponentInParent<IDamageable>();
            if (damageableTarget != null)
            {
                damageableTarget.TakeDamage(PLAYER_POWER);
            }            

            // 衝突したので自分も破棄
            myDestructible.DestroySelf();
        }
    }
}