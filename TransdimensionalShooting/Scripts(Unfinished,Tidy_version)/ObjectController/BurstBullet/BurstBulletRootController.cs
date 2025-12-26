using System;
using System.Net.Http.Headers;
using UnityEngine;

namespace Taiyaki
{
    public class BurstBulletRootController : MonoBehaviour
    {
        [Header("親弾の設定")]
        [SerializeField] private float initialSpeed = 20f; // 初速
        [SerializeField] private float decelerationRate = 5f; // 減速率 (1秒間にどれだけ遅くなるか)
        [Header("子弾の設定")]
        [SerializeField] private GameObject childBullet2DPrefab;
        [SerializeField] private GameObject childBullet3DPrefab;
        [SerializeField] private int burstAmount2D = 20; // 2Dで発射する数
        [SerializeField] private int burstAmount3D = 100; // 3Dで発射する数
        //保持するもの
        private ObjectModeDual dualMode;
        private GameObject object2D;
        private GameObject object3D;
        //2D/3DObjで使うプロパティ
        public float InitialSpeed => initialSpeed;
        public float DecelerationRate => decelerationRate;
        public float CurrentSpeed { get; set; }
        public bool HasBurst { get; set; } = false; // 発射処理を一度だけ行うためのフラグ

        private void Awake()
        {
            dualMode = GetComponent<ObjectModeDual>();
            object2D = dualMode.Object2D;
            object3D = dualMode.Object3D;
        }
        private void Start()
        {
            CurrentSpeed = InitialSpeed;
        }
        private void OnEnable()
        {
            EventBus.OnModeChanged += CustomProjection;
        }
        private void OnDisable()
        {
            EventBus.OnModeChanged -= CustomProjection;
        }
        //locationは弾がBurstした地点
        public void TriggerBurstAndDestroy(Vector3 location)
        {
            SpawnBurst2D(location);
            SpawnBurst3D(location);
            dualMode.DestroySelf();
        }

        private void CustomProjection(bool is2DMode)
        {
            if (is2DMode)
            {
                Vector3 pos = object3D.transform.position;
                object2D.transform.position = new Vector3(pos.x, 0, pos.z);
            }
            else
            {
                Vector3 pos = object2D.transform.position;
                object3D.transform.position = new Vector3(pos.x, 0, pos.z);
            }
        }
        private void SpawnBurst2D(Vector3 location)
        {
            // --- 2D: 円形（XZ平面）に発射 ---
            for (int i = 0; i < burstAmount2D; i++)
            {
                // 360度を burstAmount2D で均等に割る
                float angle = i * (360f / burstAmount2D);

                // 位置と回転設定
                Vector3 position = location;
                position.y = 0f;
                Quaternion rotation = Quaternion.Euler(90, angle, 0);

                // 生成
                Instantiate(childBullet2DPrefab, position, rotation);
            }
        }

        private void SpawnBurst3D(Vector3 location)
        {
            // --- 3D: 球状に発射 ---
            // (Fibonacci Sphere を使った均等な配置。burstAmount3D が増えても綺麗に配置されます)
            float points = burstAmount3D;
            float phi = Mathf.PI * (3f - Mathf.Sqrt(5f)); // 黄金角

            for (int i = 0; i < points; i++)
            {
                float y = 1 - (i / (points - 1)) * 2; // y は 1 から -1 まで変化
                float radius = Mathf.Sqrt(1 - y * y); // y地点での半径
                float theta = phi * i;
                float x = Mathf.Cos(theta) * radius;
                float z = Mathf.Sin(theta) * radius;

                // 中心から(x, y, z)方向への向きを計算
                Vector3 direction = new Vector3(x, y, z);
                Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90,0,0);

                // 生成
                Instantiate(childBullet3DPrefab, location, rotation);
            }
        }
    }
}