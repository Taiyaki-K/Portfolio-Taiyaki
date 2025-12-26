using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Taiyaki
{
    public class SpiralSpawner : MonoBehaviour
    {
        [Header("弾のプレハブ")]
        [SerializeField] public GameObject spiralChildBullet3DPrefab;
        [SerializeField] public GameObject spiralChildBullet2DPrefab;

        [Header("発射設定")]
        [Tooltip("1秒間に発射する弾の数")]
        [SerializeField] private float fireRate = 10f;

        [Tooltip("渦が回転する速度（1秒間の角度）")]
        [SerializeField] private float rotationSpeed = 180f;

        [Header("3D設定 (2Dの場合は 0 のまま)")]
        [Tooltip("3D（らせん）用のY軸上昇速度。2Dなら 0 にする。")]
        [SerializeField] public float upwardSpeed = 6f;

        [Header("発射設定")]
        [Tooltip("スポナーの有効期限")]
        [SerializeField] private float spawnerLifetime = 10f;

        private float fireTimer = 0f;    // 発射用タイマー
        private float currentAngle = 0f; // 現在の回転角度
        private Vector3 spawnPositionOffset; // 3D用のY軸オフセット

        private async void Start()
        {
            spawnPositionOffset = Vector3.zero;
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(spawnerLifetime), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());
                Destroy(gameObject);
            }
            catch (OperationCanceledException)
            {
                // オブジェクトが破棄された場合、Delayがキャンセルされてここに来る。
                // これは正常な動作なので、エラーログを出す必要はない。
            }
        }

        void Update()
        {
            // --- 1. 角度と位置を更新 ---

            // 時間経過で角度を回転させる
            currentAngle += rotationSpeed * Time.deltaTime;

            // 時間経過でYオフセットを上昇させる (upwardSpeedが0なら変化しない)
            spawnPositionOffset.y += upwardSpeed * Time.deltaTime;

            // --- 2. 発射タイマーの処理 ---
            fireTimer += Time.deltaTime;

            // 1秒間に fireRate 回発射するための計算 (例: 1.0 / 20 = 0.05秒ごと)
            float fireInterval = 1.0f / fireRate;

            if (fireTimer >= fireInterval)
            {
                fireTimer -= fireInterval; // タイマーをリセット

                // --- 3. 弾の発射 ---
                SpawnBullet3D();
                SpawnBullet2D();
            }
        }

        private void SpawnBullet3D()
        {
            // 1. スポナーの現在位置 + 3D用のYオフセット
            Vector3 finalSpawnPosition = transform.position + spawnPositionOffset;

            // 2. 現在の角度（Y軸回転）で回転を生成
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0) * Quaternion.Euler(90,0,0);

            // 3. 弾のRootを生成
            GameObject bullet = Instantiate(spiralChildBullet3DPrefab, finalSpawnPosition, rotation);
        }
        private void SpawnBullet2D()
        {
            // 1. スポナーの現在位置 + 3D用のYオフセット
            Vector3 finalSpawnPosition = transform.position;
            finalSpawnPosition.y = 0;

            // 2. 現在の角度（Y軸回転）で回転を生成
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0) * Quaternion.Euler(90, 0, 0);

            // 3. 弾のRootを生成
            GameObject bullet = Instantiate(spiralChildBullet2DPrefab, finalSpawnPosition, rotation);
        }
    }
}