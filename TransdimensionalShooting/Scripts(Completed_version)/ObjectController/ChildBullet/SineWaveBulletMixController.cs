using UnityEngine;

namespace Taiyaki
{
    [RequireComponent(typeof(Rigidbody))]
    public class SineWaveBulletMixController : MonoBehaviour
    {
        [Header("基本移動")]
        [SerializeField]
        private float speed = 20f; // 前進する速度

        [Header("波状設定")]
        [Tooltip("波の揺れる速さ（周波数）")]
        [SerializeField]
        private float waveFrequency = 5f;

        [Tooltip("波の揺れる幅（振幅）")]
        [SerializeField]
        private float waveAmplitude = 1f;

        [Tooltip("2D/3D: 水平方向(左右)に揺れるか(falseにすると垂直(上下)に揺れる)")]
        [SerializeField]
        private bool wiggleHorizontal = true;

        [Header("傾き設定")]
        [Tooltip("横移動の速度に合わせて機体を傾ける量（角度）")]
        [SerializeField]
        private float tiltAmount = 10f;

        private Rigidbody rb;
        private float timeAlive = 0f; // 弾が発射されてからの時間
        private Quaternion baseRotation; // 生成時の向き

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;

            // Instantiate時に設定された基本の向きを保存
            baseRotation = transform.rotation;
        }
        private void FixedUpdate()
        {
            // 弾がアクティブになってからの時間を加算
            timeAlive += Time.fixedDeltaTime;

            // --- 1. 波状（サインカーブ）の「横速度」を計算 ---
            float waveVelocityMagnitude = Mathf.Cos(timeAlive * waveFrequency) * waveFrequency * waveAmplitude;

            // --- 2. 傾き（ローリング）の計算と適用 ---

            // 横速度(waveVelocityMagnitude)に応じて、ローカルZ軸の傾き角度を計算
            // (右に動く時(プラス)は、右に傾く(Zマイナス回転))
            float rollAngle = -waveVelocityMagnitude * tiltAmount;

            // スポナーが設定した「基本の向き」に「傾き」を追加
            Quaternion tiltRotation = Quaternion.Euler(0, 0, rollAngle);
            rb.MoveRotation(baseRotation * tiltRotation); // ★ 回転を物理的に適用

            // --- 3. 移動速度の計算と適用 ---
            // (回転が適用された「後」の transform を使って速度を計算)

            // (弾がX軸90度回転している前提なら transform.up、そうでなければ transform.forward)
            Vector3 forwardVelocity = transform.up * speed;

            // 揺れる方向（軸）を決める
            Vector3 oscillationAxis = wiggleHorizontal ? transform.right : transform.up;

            // 横揺れの速度ベクトル
            Vector3 waveVelocity = oscillationAxis * waveVelocityMagnitude;

            // --- 4. 2つの速度を合成 ---
            rb.linearVelocity = forwardVelocity + waveVelocity;
        }
    }
}