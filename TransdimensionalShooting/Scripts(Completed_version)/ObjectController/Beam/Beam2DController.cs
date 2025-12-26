using KanKikuchi.AudioManager;
using UnityEngine;

namespace Taiyaki
{
    public class Beam2DController : MonoBehaviour
    {
        [SerializeField]
        private GameObject hitEffectPrefab;
        [SerializeField] float speed = 50f;

        // ★ 1. オブジェクトが生成された「実時間」を記録する変数
        private float _creationTime;

        private void Awake()
        {
            // ★ 2. 生成された瞬間の実時間（timeScaleを無視）を記録
            _creationTime = Time.unscaledTime;
        }

        private void Update()
        {
            transform.position += transform.up * speed * Time.deltaTime;
        }

        /// <summary>
        /// このGameObjectが破棄される瞬間に呼び出されます
        /// </summary>
        private void OnDestroy()
        {
            // (安全装置 1: アプリが再生中でなければ何もしない)
            if (!Application.isPlaying)
            {
                return;
            }

            // (安全装置 2: シーンがアンロード中なら何もしない)
            if (!gameObject.scene.isLoaded)
            {
                return;
            }

            // (安全装置 3: プレハブが未設定なら何もしない)
            if (hitEffectPrefab == null)
            {
                return;
            }

            // (安全装置 4: GameModeManager が先に破棄されていたら何もしない)
            if (GameModeManager.Instance == null)
            {
                return;
            }

            // --- ▼ ここからが修正ロジック ▼ ---

            // 3. このオブジェクトが生存していた「実時間」を計算
            float lifeDuration = Time.unscaledTime - _creationTime;

            // 4. 生存時間が 2.0秒以内 かつ 2Dモードであるか
            if (lifeDuration <= 2.0f && GameModeManager.Instance.Is2DMode)
            {
                // 5. エフェクトを生成し、SEを再生
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                SEManager.Instance.Play(SEPath.BIT_HIT, 0.3f);
            }

            // --- ▲ 修正ここまで ▲ ---
        }
    }
}