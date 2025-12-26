using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Taiyaki
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Image dragonHpBar;
        [SerializeField] private GameObject cursor;
        [SerializeField] private GameObject gameOverScreen;

        private int dragonMaxHp;
        private void OnEnable()
        {
            EventBus.OnModeChanged += EnableCursor;
            EventBus.OnPlayerDied += HandlePlayerDeath;
            EventBus.OnDragonHpInitialized += InitializeDragonHp;
            EventBus.OnTakeDragonDamage += UpdateDragonHp;
        }
        private void OnDisable()
        {
            EventBus.OnModeChanged -= EnableCursor;
            EventBus.OnPlayerDied -= HandlePlayerDeath;
            EventBus.OnDragonHpInitialized -= InitializeDragonHp;
            EventBus.OnTakeDragonDamage -= UpdateDragonHp;
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
            }
            catch (System.Exception e)
            {
                Debug.LogError("ゲームオーバー画面の表示に失敗: " + e.Message);
            }
        }
    }
}