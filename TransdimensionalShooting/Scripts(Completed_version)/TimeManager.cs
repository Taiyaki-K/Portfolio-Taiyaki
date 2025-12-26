using Cysharp.Threading.Tasks;
using UnityEngine;
using KanKikuchi.AudioManager;

namespace Taiyaki
{
    public class TimeManager : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.OnPlayerDied += HandleDeath;
            EventBus.OnDragonDied += HandleDeath;
        }
        private void OnDisable()
        {
            EventBus.OnPlayerDied -= HandleDeath;
            EventBus.OnDragonDied -= HandleDeath;
        }
        private void HandleDeath()
        {
            //「_ =」を付けて、Taskを実行しっぱなしにする
            // これで HandlePlayerDeath はすぐに終了できる
            _ = DeathEffectAsync();
        }
        private async UniTask DeathEffectAsync()
        {
            StartSlowMotion(0.2f);
            await UniTask.WaitForSeconds(3f, ignoreTimeScale: true);
            StopSlowMotion();
        }
        private void StartSlowMotion(float slowFactor)
        {
            // slowFactor に 0.5 を渡すと、50%の速度になる
            Time.timeScale = slowFactor;
            //Fixedの間隔も同時に調整
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        private void StopSlowMotion()
        {
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = 0.02f;
        }
    }
}