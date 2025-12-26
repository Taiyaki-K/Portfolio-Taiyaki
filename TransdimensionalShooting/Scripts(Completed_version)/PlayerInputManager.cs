using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Taiyaki
{
    public class PlayerInputManager : MonoBehaviour
    {
        private TaiyakiActions inputActions;

        [Header("クールダウン設定")]
        [SerializeField] private float changeModeCooldown = 5.0f; // モード切替のクールダウン（秒）

        private bool _isChangeModeCooldown = false; // クールダウン中かどうかのフラグ
        private CancellationTokenSource _cts;     // 非同期処理をキャンセルするため

        private void Awake()
        {
            inputActions = new TaiyakiActions();
            _cts = new CancellationTokenSource();
        }

        private void OnEnable()
        {
            inputActions.Player.Enable();

            inputActions.Player.Shoot.performed += OnShoot;
            inputActions.Player.ChangeMode.performed += OnChangeMode;
            EventBus.OnPlayerDied += StopInput;
            EventBus.OnDragonDied += StopInput;
        }

        private void OnDisable()
        {
            inputActions.Player.Shoot.performed -= OnShoot;
            inputActions.Player.ChangeMode.performed -= OnChangeMode;
            EventBus.OnPlayerDied -= StopInput;
            EventBus.OnDragonDied -= StopInput;

            inputActions.Player.Disable();

            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void Update()
        {
            Vector2 currentMove = inputActions.Player.Move.ReadValue<Vector2>();
            EventBus.PublishMove(currentMove);
        }

        // --- ▼ OnShoot (射撃) のロジック ▼ ---
        private void OnShoot(InputAction.CallbackContext context)
        {
            // ★ クールダウンロジックはここには不要

            // ★ PublishShoot (射撃イベント) を呼ぶ
            var pointer = Pointer.current; // 現在のマウス/タッチ
            if (pointer != null)
            {
                Vector2 mousePos = pointer.position.ReadValue(); // スクリーン座標
                EventBus.PublishShoot(mousePos);
            }
            else
            {
                EventBus.PublishShoot(Vector2.zero);
                Debug.LogWarning("現在のマウス/タッチを取得できませんでした");
            }
        }

        // --- ▼ OnChangeMode (モード変更) のロジック ▼ ---
        private void OnChangeMode(InputAction.CallbackContext context)
        {
            // 1. クールダウン中(true)であれば、何もせずに処理を終了
            //    (★ このロジックを OnShoot から移動)
            if (_isChangeModeCooldown)
            {
                return;
            }
            EventBus.PublishModeCooldownStarted(changeModeCooldown);
            // 2. クールダウン中でなければ、イベントを発行
            EventBus.PublishSpacePressed();

            // 3. クールダウン処理を「実行しっぱなし」(.Forget())で開始
            StartChangeModeCooldown().Forget();
        }

        /// <summary>
        /// モード切替のクールダウン（5秒）を開始する
        /// </summary>
        private async UniTask StartChangeModeCooldown()
        {
            // 1. クールダウンフラグを立てる
            _isChangeModeCooldown = true;

            try
            {
                // 2. 5秒間、実時間で待機する (Time.timeScale を無視)
                await UniTask.Delay(TimeSpan.FromSeconds(changeModeCooldown), ignoreTimeScale: true, cancellationToken: _cts.Token);
            }
            catch (OperationCanceledException)
            {
                // (5秒経つ前にオブジェクトが破棄された場合。正常な動作)
            }

            // 3. 5秒経過したら、フラグを戻す (再度 Space を押せるようにする)
            _isChangeModeCooldown = false;
        }
        /// <sVsummary>
        /// PlayerDiedイベントを受け取った時に呼び出される
        /// </summary>
        private void StopInput()
        {
            // 1. "Player" アクションマップ全体を無効にする
            inputActions.Player.Disable();

            // 2. (念のため) クールダウンタイマーも止める
            _cts?.Cancel();
        }
    }
}