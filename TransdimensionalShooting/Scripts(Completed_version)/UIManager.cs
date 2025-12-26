using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Taiyaki
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Image dragonHpBar;
        [SerializeField] private GameObject cursor;
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private GameObject gameClearScreen;
        [Header("クールダウンUI")]
        [Tooltip("クールダウン表示の親オブジェクト (必要なら)")]
        [SerializeField] private GameObject cooldownUIGroup;

        [Tooltip("クールダウンをバーで表示するImage")]
        [SerializeField] private Image cooldownBarImage;

        [SerializeField] private TextMeshProUGUI nannido;


        private int dragonMaxHp;

        private CancellationTokenSource _cooldownCts; // ★ タイマーキャンセル用

        private void Awake()
        {
            // ★ CancellationTokenSource を初期化
            _cooldownCts = new CancellationTokenSource();

            if (cooldownUIGroup != null)
                cooldownUIGroup.SetActive(false);
            else if (cooldownBarImage != null)
                cooldownBarImage.gameObject.SetActive(false);

            nannido.text = GameManager.Instance.modeName;
        }
        private void OnEnable()
        {
            EventBus.OnModeChanged += EnableCursor;
            EventBus.OnPlayerDied += HandlePlayerDeath;
            EventBus.OnDragonDied += HandleDragonDeath;
            EventBus.OnDragonHpInitialized += InitializeDragonHp;
            EventBus.OnTakeDragonDamage += UpdateDragonHp;
            EventBus.OnModeCooldownStarted += StartCooldownVisuals;
        }
        private void OnDisable()
        {
            EventBus.OnModeChanged -= EnableCursor;
            EventBus.OnPlayerDied -= HandlePlayerDeath;
            EventBus.OnDragonDied -= HandleDragonDeath;
            EventBus.OnDragonHpInitialized -= InitializeDragonHp;
            EventBus.OnTakeDragonDamage -= UpdateDragonHp;
            EventBus.OnModeCooldownStarted -= StartCooldownVisuals;

            _cooldownCts?.Cancel();
            _cooldownCts?.Dispose();
        }
        private void InitializeDragonHp(int maxHp)
        {
            this.dragonMaxHp = maxHp;
            UpdateDragonHp(maxHp);
        }
        private void UpdateDragonHp(int dragonHp)
        {
            if (dragonMaxHp == 0) return;

            float fillPercentage = (float)dragonHp / dragonMaxHp;
            dragonHpBar.fillAmount = fillPercentage;
        }
        private void HandlePlayerDeath()
        {
            //「_ =」を付けて、Taskを実行しっぱなしにする
            // これで HandlePlayerDeath はすぐに終了できる
            _ = SetGameOverScreenAsync();
        }
        private void EnableCursor(bool is2DMode)
        {
            if(is2DMode)
            {
                cursor.SetActive(false);
            }
            else
            {
                cursor.SetActive(true);
            }
        }
        private async UniTask SetGameOverScreenAsync()
        {
            try
            {
                await UniTask.WaitForSeconds(3f, ignoreTimeScale: true);
                gameOverScreen.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                SEManager.Instance.Play(SEPath.LOSE);
            }
            catch (System.Exception e)
            {
                Debug.LogError("ゲームオーバー画面の表示に失敗: " + e.Message);
            }
        }
        private void HandleDragonDeath()
        {
            //「_ =」を付けて、Taskを実行しっぱなしにする
            // これで HandlePlayerDeath はすぐに終了できる
            _ = SetGameClearScreenAsync();
        }
        private async UniTask SetGameClearScreenAsync()
        {
            try
            {
                await UniTask.WaitForSeconds(3f, ignoreTimeScale: true);
                gameClearScreen.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                SEManager.Instance.Play(SEPath.WIN);
            }
            catch (System.Exception e)
            {
                Debug.LogError("ゲームオーバー画面の表示に失敗: " + e.Message);
            }
        }


        //ボタンから呼び出す関数
        public void Retry()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        public void BackToMenu()
        {
            SceneManager.LoadScene("Title");
        }

        /// <summary>
        /// EventBusから「クールダウン開始」通知を受け取る
        /// </summary>
        private void StartCooldownVisuals(float duration)
        {
            // 既に実行中のタイマーがあれば、まずキャンセルする
            _cooldownCts?.Cancel();
            _cooldownCts?.Dispose();
            _cooldownCts = new CancellationTokenSource();

            // 新しいタイマー処理を「実行しっぱなし」にする (.Forget())
            ShowCooldownTimer(duration, _cooldownCts.Token).Forget();
        }

        /// <summary>
        /// クールダウンタイマーのUI表示（非同期）
        /// </summary>
        private async UniTask ShowCooldownTimer(float duration, CancellationToken token)
        {
            if (cooldownBarImage == null) return; // バーが設定されてなければ何もしない

            // UIを表示（グループまたはバー自体）
            if (cooldownUIGroup != null)
                cooldownUIGroup.SetActive(true);
            else
                cooldownBarImage.gameObject.SetActive(true);

            float elapsedTime = 0f;

            try
            {
                // duration (5秒) が経過するまでループ
                while (elapsedTime < duration)
                {
                    // (UniTask.DeltaTime を使うと実時間（timeScale無視）で進む)
                    elapsedTime += Time.unscaledDeltaTime;

                    // --- ★ここが修正点 (テキスト -> Fill) ---

                    // 残り時間の割合を計算 (例: 4.9 / 5.0 = 0.98)
                    float fillPercentage = (duration - elapsedTime) / duration;

                    // fillAmount を 1.0 -> 0.0 に減らしていく
                    cooldownBarImage.fillAmount = Mathf.Clamp01(fillPercentage);
                    // --- ★修正ここまで ---

                    // 次のフレームまで待機
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                // ループが完了（クールダウン終了）
                cooldownBarImage.fillAmount = 0; // 確実に0にする
                if (cooldownUIGroup != null)
                    cooldownUIGroup.SetActive(false);
                else
                    cooldownBarImage.gameObject.SetActive(false);
            }
            catch (OperationCanceledException)
            {
                // (オブジェクト破棄などでタスクがキャンセルされた場合)
                if (cooldownUIGroup != null)
                    cooldownUIGroup.SetActive(false);
                else if (cooldownBarImage != null)
                    cooldownBarImage.gameObject.SetActive(false);
            }
        }
    }
}