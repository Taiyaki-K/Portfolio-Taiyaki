using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace Taiyaki
{
    public class DragonAttackManager : MonoBehaviour
    {
        private enum DragonAttackPatterns
        {
            Homing,
            Constant,
            Spiral,
            Burst,
            Explosion,
        }
        [Header("攻撃設定")]
        [SerializeField] private GameObject homingBullet;
        [SerializeField] private GameObject constantMixSpawner;
        [SerializeField] private GameObject constant3DSpawner;
        [SerializeField] private GameObject spiralSpawner;
        [SerializeField] private GameObject burstSpawner;
        [SerializeField] private GameObject explosionSpawner;
        [Header("子弾幕設定")]
        [SerializeField] private GameObject forwardBullet3D;
        [SerializeField] private GameObject forwardBullet2D;
        [SerializeField] private GameObject sineBullet3D;
        [SerializeField] private GameObject sineBullet2D;
        [Header("攻撃タイミング設定")]
        [Tooltip("ボスが登場してから最初の攻撃を開始するまでの待機時間")]
        [SerializeField] private float initialDelay = 3.0f;

        [Tooltip("1回の攻撃が持続する時間")]
        [SerializeField] private float attackDuration = 5.0f;

        [Tooltip("次の攻撃に移るまでのクールダウン（待機時間）")]
        [SerializeField] private float attackInterval = 2.0f;

        // このスクリプト（ボス）が破棄された時に、非同期処理を中断するためのトークン
        private CancellationTokenSource cts;

        private void Awake()
        {
            attackDuration = GameManager.Instance.attackDuration;
            attackInterval = GameManager.Instance.attackInterval;
        }
        async void Start()
        {
            // このオブジェクトが破棄されたらキャンセル通知を送る
            cts = new CancellationTokenSource();

            // 1. ボス登場時の待機
            await UniTask.Delay(TimeSpan.FromSeconds(initialDelay), ignoreTimeScale: true, cancellationToken: cts.Token);

            // 2. メインの攻撃ループを開始（.Forget()で実行しっぱなしにする）
            AttackLoop().Forget();
        }

        private void OnDestroy()
        {
            // オブジェクトが破棄されたら、AttackLoopを確実に中断する
            cts?.Cancel();
            cts?.Dispose();
        }

        /// <summary>
        /// ボスが生きている間、ランダムに攻撃を繰り返すメインループ
        /// </summary>
        private async UniTask AttackLoop()
        {
            // cts.Token がキャンセルされる（ボスが死ぬ）まで無限ループ
            while (!cts.IsCancellationRequested)
            {
                // --- 1. 攻撃の選択 ---
                int maxIndex = Enum.GetNames(typeof(DragonAttackPatterns)).Length;
                int randomIndex = UnityEngine.Random.Range(0, maxIndex);
                DragonAttackPatterns currentPattern = (DragonAttackPatterns)randomIndex;

                // ★ デフォルトの持続時間(attackDuration)をスキップするかどうか
                bool skipDefaultDuration = false;

                switch (currentPattern)
                {
                    case DragonAttackPatterns.Homing:
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                GameObject homing = Instantiate(homingBullet, new Vector3(0, 0, 34), Quaternion.Euler(90, 180, 0));
                                homing.GetComponent<ISetHomingTarget>().InitializeTargets(GameModeManager.Instance.playerTarget3D, GameModeManager.Instance.playerTarget2D);
                                // 1秒待機 (cts.Tokenを渡して、ボスが死んだら中断できるようにする)
                                await UniTask.Delay(TimeSpan.FromSeconds(1.0f), ignoreTimeScale: true, cancellationToken: cts.Token);
                            }
                            // ★ Homing攻撃は独自の持続時間(3秒)を持ったので、
                            // ★ デフォルトの attackDuration (5秒) はスキップする
                            skipDefaultDuration = true;
                            break;
                        }
                    case DragonAttackPatterns.Constant:
                        {
                            //xy平面
                            Vector3 pos;
                            if (UnityEngine.Random.value < 0.5f)
                            {
                                int x = 30;
                                int y = UnityEngine.Random.Range(-30, 30);
                                int z = UnityEngine.Random.Range(0, 30);
                                pos = new Vector3(x, y, z);
                                GameObject constantMix = Instantiate(constantMixSpawner, pos, Quaternion.identity);
                                constantMix.GetComponent<ConstantMixSpawner>().yRotation = -90;
                            }
                            else
                            {
                                int x = -30;
                                int y = UnityEngine.Random.Range(-30, 30);
                                int z = UnityEngine.Random.Range(0, 30);
                                pos = new Vector3(x, y, z);
                                GameObject constantMix = Instantiate(constantMixSpawner, pos, Quaternion.identity);
                                constantMix.GetComponent<ConstantMixSpawner>().yRotation = 90;
                            }
                            //3Dのみ、垂直
                            Vector3 pos3D;
                            if (UnityEngine.Random.value < 0.5f)
                            {
                                int x = UnityEngine.Random.Range(-20, 20);
                                int y = -30;
                                float z = GameModeManager.Instance.playerTarget3D.position.z;
                                pos3D = new Vector3(x, y, z);
                                GameObject constant3D = Instantiate(constant3DSpawner, pos3D, Quaternion.identity);
                                constant3D.GetComponent<Constant3DSpawner>().lookRot = new Vector3(0, 1, 0);
                            }
                            else
                            {
                                int x = UnityEngine.Random.Range(-20, 20);
                                int y = 30;
                                float z = GameModeManager.Instance.playerTarget3D.position.z;
                                pos3D = new Vector3(x, y, z);
                                GameObject constant3D = Instantiate(constant3DSpawner, pos3D, Quaternion.identity);
                                constant3D.GetComponent<Constant3DSpawner>().lookRot = new Vector3(0, -1, 0);
                            }
                            break;
                        }
                    case DragonAttackPatterns.Spiral:
                        {
                            GameObject spiral;
                            if (UnityEngine.Random.value < 0.5f)
                            {
                                int x = UnityEngine.Random.Range(-20, 20);
                                int y = -30;
                                int z = UnityEngine.Random.Range(10, 20);
                                Vector3 pos = new Vector3(x, y, z);
                                spiral = Instantiate(spiralSpawner, pos, Quaternion.identity);
                                spiral.GetComponent<SpiralSpawner>().upwardSpeed = 6;
                            }
                            else
                            {
                                int x = UnityEngine.Random.Range(-20, 20);
                                int y = 30;
                                int z = UnityEngine.Random.Range(10, 20);
                                Vector3 pos = new Vector3(x, y, z);
                                spiral = Instantiate(spiralSpawner, pos, Quaternion.identity);
                                spiral.GetComponent<SpiralSpawner>().upwardSpeed = -6;
                            }
                            //子弾幕設定
                            if (UnityEngine.Random.value < 0.5f)
                            {
                                spiral.GetComponent<SpiralSpawner>().spiralChildBullet3DPrefab = forwardBullet3D;
                                spiral.GetComponent<SpiralSpawner>().spiralChildBullet2DPrefab = forwardBullet2D;
                            }
                            else
                            {
                                spiral.GetComponent<SpiralSpawner>().spiralChildBullet3DPrefab = sineBullet3D;
                                spiral.GetComponent<SpiralSpawner>().spiralChildBullet2DPrefab = sineBullet2D;
                            }
                            break;
                        }
                    case DragonAttackPatterns.Burst:
                        {
                            int x = UnityEngine.Random.Range(-25, 25);
                            int y = 0;
                            int z = 34;
                            Vector3 pos = new Vector3(x, y, z);
                            GameObject burst = Instantiate(burstSpawner, pos, Quaternion.identity);
                            //子弾幕の設定
                            if (UnityEngine.Random.value < 0.5f)
                            {
                                burst.GetComponent<BurstBulletRootController>().childBullet3DPrefab = forwardBullet3D;
                                burst.GetComponent<BurstBulletRootController>().childBullet2DPrefab = forwardBullet2D;
                            }
                            else
                            {
                                burst.GetComponent<BurstBulletRootController>().childBullet3DPrefab = sineBullet3D;
                                burst.GetComponent<BurstBulletRootController>().childBullet2DPrefab = sineBullet2D;
                            }
                            break;
                        }
                    case DragonAttackPatterns.Explosion:
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                int x = UnityEngine.Random.Range(-25, 25);
                                int y = UnityEngine.Random.Range(-25, 25);
                                int z = UnityEngine.Random.Range(0, 25);
                                Vector3 pos = new Vector3(x, y, z);
                                Instantiate(explosionSpawner, pos, Quaternion.identity);
                                await UniTask.Delay(TimeSpan.FromSeconds(0.5f), ignoreTimeScale: true, cancellationToken: cts.Token);
                            }
                            // ★ Homing攻撃は独自の持続時間(3秒)を持ったので、
                            // ★ デフォルトの attackDuration (5秒) はスキップする
                            skipDefaultDuration = true;
                            break;
                        }
                }

                // --- 3. 攻撃の持続 ---
                // ★ スキップフラグが false の場合のみ、デフォルトの持続時間待機
                if (!skipDefaultDuration)
                {
                    // 「attackDuration」（例: 5秒）待つ
                    await UniTask.Delay(TimeSpan.FromSeconds(attackDuration), ignoreTimeScale: true, cancellationToken: cts.Token);
                }

                // --- 5. クールダウン ---
                // 「attackInterval」（例: 2秒）待って、次のループ（攻撃）へ
                await UniTask.Delay(TimeSpan.FromSeconds(attackInterval), ignoreTimeScale: true, cancellationToken: cts.Token);
            }
        }
    }
}