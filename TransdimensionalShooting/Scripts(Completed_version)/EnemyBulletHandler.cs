using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Taiyaki
{
    /// <summary>
    /// 「敵の弾」専用の衝突・寿命ハンドラ
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyBulletHandler : MonoBehaviour
    {
        [SerializeField] private float lifetime = 10.0f;

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
                await UniTask.Delay(TimeSpan.FromSeconds(lifetime), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());
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

            // ★ この弾は「Player」レイヤーにしか興味がない
            // (LayerCollisionMatrixでPlayer以外とは衝突しない設定にするのがベスト)

            IKillable killableTarget = collision.gameObject.GetComponentInParent<IKillable>();
            if (killableTarget != null)
            {
                killableTarget.OnKill();
            }

            // 衝突したので自分も破棄
            myDestructible.DestroySelf();
        }
    }
}