using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Taiyaki
{
    [RequireComponent(typeof(ObjectModeDual))]
    public class PlayerRootController : MonoBehaviour, IKillable
    {
        [Header("無敵演出")]
        [SerializeField] private Color _invincibleColor = Color.yellow; // ★ 無敵中の色
        [Header("死亡演出")]
        [SerializeField] private GameObject deathEffect;

        private ObjectModeDual dualMode;
        private GameObject object3D;
        private GameObject object2D;

        // 1. 無敵状態を管理するフラグ
        private bool _isInvincible = false;
        // 2. 無敵タイマーをキャンセルするためのトークン
        private CancellationTokenSource _invincibilityCts;

        private List<Material> _allMaterials = new List<Material>();
        private List<Color> _originalColors = new List<Color>();

        private void Awake()
        {
            dualMode = GetComponent<ObjectModeDual>();
            object3D = dualMode.Object3D;
            object2D = dualMode.Object2D;
            _invincibilityCts = new CancellationTokenSource();

            Renderer[] allRenderers = gameObject.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in allRenderers)
            {
                // .material を呼ぶと、そのRenderer専用のマテリアルインスタンスが作られる
                Material mat = renderer.material;
                if (mat != null)
                {
                    _allMaterials.Add(mat);
                    _originalColors.Add(mat.color); // 元の色を保存
                }
            }
        }
        private void OnEnable()
        {
            EventBus.OnModeChanged += CustomProjection;
            EventBus.OnPlayerDied += PlayDeathDirection;
            EventBus.OnDragonDied += OnKillDragon;
            EventBus.OnModeChanged += ActivateInvincibility;
        }
        private void OnDisable()
        {
            EventBus.OnModeChanged -= CustomProjection;
            EventBus.OnPlayerDied -= PlayDeathDirection;
            EventBus.OnDragonDied -= OnKillDragon;
            EventBus.OnModeChanged -= ActivateInvincibility;
            // オブジェクト破棄時にタイマーを確実に停止
            _invincibilityCts?.Cancel();
            _invincibilityCts?.Dispose();
        }

        public void OnKill()
        {
            if (_isInvincible)
            {
                return;
            }
            EventBus.PublishPlayerDied();
        }
        /// <summary>
        /// モード切替時に呼び出され、1秒間の無敵タイマーを開始する
        /// </summary>
        private void ActivateInvincibility(bool is2DMode)
        {
            // 既に実行中の古いタイマーがあれば、まずキャンセルする
            _invincibilityCts?.Cancel();
            _invincibilityCts?.Dispose();
            _invincibilityCts = new CancellationTokenSource();

            // 新しいタイマー処理を「実行しっぱなし」にする (.Forget())
            InvincibilityTimer(_invincibilityCts.Token).Forget();
        }

        /// <summary>
        /// 1秒間の無敵処理（非同期）
        /// </summary>
        private async UniTask InvincibilityTimer(CancellationToken token)
        {
            try
            {
                // 1. 無敵状態にし、色を「黄色」に変える
                _isInvincible = true;
                SetInvincibleVisuals(true);

                // 2. 実時間で1秒間待機
                await UniTask.Delay(TimeSpan.FromSeconds(1.0f), ignoreTimeScale: true, cancellationToken: token);

                // 3. 1秒経過後、無敵状態を解除し、色を「元の色」に戻す
                _isInvincible = false;
                SetInvincibleVisuals(false);
            }
            catch (OperationCanceledException)
            {
                // (キャンセルされた場合、色は元に戻す)
                _isInvincible = false;
                SetInvincibleVisuals(false);
            }
        }
        private void CustomProjection(bool is2DMode)
        {
            if (is2DMode)
            {
                Vector3 pos = object3D.transform.position;
                object2D.transform.position = new Vector3(pos.x, 0, pos.z);
            }
            else
            {
                Vector3 pos = object2D.transform.position;
                object3D.transform.position = new Vector3(pos.x, 0, pos.z);
            }
        }

        private void PlayDeathDirection()
        {
            //エフェクト出す位置を決めるため、現在のモードのPlayerを取る
            GameObject currentPlayerObj;
            if (GameModeManager.Instance.Is2DMode)
            { currentPlayerObj = object2D; }
            else
            { currentPlayerObj = object3D; }

            Renderer[] allRenderers = gameObject.GetComponentsInChildren<Renderer>(true);
            Collider[] allCollider = gameObject.GetComponentsInChildren<Collider>(true);

            foreach (Renderer renderer in allRenderers)
            {
                renderer.enabled = false;
            }
            foreach (Collider collider in allCollider)
            {
                collider.enabled = false;
            }
            SetInvincibleVisuals(false);

            Instantiate(deathEffect, currentPlayerObj.transform.position, Quaternion.identity);
        }
        private void OnKillDragon()
        {
            _invincibilityCts?.Cancel();

            _isInvincible = true;
        }

        // --- ▼ ここから追加 ▼ ---

        /// <summary>
        /// 2D/3D両方のマテリアルの色を一括で変更する
        /// </summary>
        private void SetInvincibleVisuals(bool isActive)
        {
            for (int i = 0; i < _allMaterials.Count; i++)
            {
                if (_allMaterials[i] == null) continue;

                if (isActive)
                {
                    // 無敵色（黄色）に設定
                    _allMaterials[i].color = _invincibleColor;
                }
                else
                {
                    // 保存しておいた元の色に戻す
                    _allMaterials[i].color = _originalColors[i];
                }
            }
        }
    }
}