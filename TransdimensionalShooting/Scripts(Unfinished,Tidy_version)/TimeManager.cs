using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Taiyaki
{
    public class TimeManager : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.OnPlayerDied += HandlePlayerDeath;
        }
        private void OnDisable()
        {
            EventBus.OnPlayerDied -= HandlePlayerDeath;
        }
        private void HandlePlayerDeath()
        {
            //「_ =」を付けて、Taskを実行しっぱなしにする
            // これで HandlePlayerDeath はすぐに終了できる
            _ = PlayDeathEffectAsync();
        }
        private async UniTask PlayDeathEffectAsync()
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