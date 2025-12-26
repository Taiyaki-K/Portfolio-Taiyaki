using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Taiyaki
{
    public class Constant3DSpawner : MonoBehaviour
    {
        [Header("弾のプレハブ（Root）")]
        [Tooltip("3Dモードで発射する、弾のプレハブ")]
        [SerializeField] private GameObject bullet3DPrefab;

        [Header("発射設定")]
        [Tooltip("1秒間に発射する弾の数")]
        [SerializeField] private float fireRate = 5f;
        [Tooltip("発射する方向、弾が向く方向")]
        [SerializeField] private Vector3 lookRot = Vector3.zero;

        [Header("スポナーの寿命")]
        [Tooltip("このスポナー自体が消滅するまでの時間（秒）。0以下なら消滅しない。")]
        [SerializeField] private float lifetime = 10f;

        private float fireTimer = 0f; // 発射用タイマー

        async void Start()
        {
            if (lifetime > 0)
            {
                try
                {
                    // lifetime秒、実時間で待機 (timeScale無視)
                    await UniTask.Delay(TimeSpan.FromSeconds(lifetime), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());
                    Destroy(gameObject);
                }
                catch (OperationCanceledException)
                {
                    // (自分自身が寿命より先に破棄された場合。正常な動作)
                }
            }
        }
        void Update()
        {
            fireTimer += Time.deltaTime;
            float fireInterval = 1.0f / fireRate;

            if (fireTimer >= fireInterval)
            {
                fireTimer -= fireInterval;
                SpawnBullet();
            }
        }

        private void SpawnBullet()
        {
            Vector3 spawnPosition = transform.position;
            Quaternion rotation = Quaternion.LookRotation(lookRot) * Quaternion.Euler(90,0,0);
            Instantiate(bullet3DPrefab, spawnPosition, rotation);
        }
    }
}