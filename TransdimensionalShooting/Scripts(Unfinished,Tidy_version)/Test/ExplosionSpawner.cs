using Cysharp.Threading.Tasks;
using System;
using Taiyaki;
using UnityEngine;
using UnityEngine.UIElements;

namespace Taiyaki
{
    public class ExplosionSpawner : MonoBehaviour
    {
        [Header("弾のプレハブ（Root）")]
        [Tooltip("2Dモードで発射する、波状弾のRootプレハブ")]
        [SerializeField] private GameObject bullet2DPrefab;
        [Tooltip("3Dモードで発射する、波状弾のRootプレハブ")]
        [SerializeField] private GameObject bullet3DPrefab;

        [Header("爆発設定")]
        [SerializeField] private float delayTime = 0f;
        [SerializeField] private int bullet3DNumber = 40;
        [SerializeField] private int bullet2DNumber = 20;
        [SerializeField] private float explosionForce = 10f;

        async void Start()
        {
            try
            {
                await UniTask.WaitForSeconds(delayTime);

                Spawn3DBullet();
                Spawn2DBullet();

                Destroy(gameObject);
            }
            catch (OperationCanceledException)
            {
                // (正常なキャンセル)
            }
        }

        private void Spawn3DBullet()
        {
            for (int i = 0; i < bullet3DNumber; i++)
            {
                // 全員を「同じ場所」に生成
                GameObject bullet = Instantiate(bullet3DPrefab, transform.position, Quaternion.identity);

                Rigidbody rb = bullet.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    // 「全方位ランダム」な方向ベクトルを取得
                    //    (normalized で、方向だけ(長さ1)にする)
                    Vector3 randomDirection = UnityEngine.Random.insideUnitSphere.normalized;

                    // その方向に「爆発力（衝撃）」を加える
                    rb.AddForce(randomDirection * explosionForce, ForceMode.Impulse);
                }
                else
                {
                    Debug.LogWarning("Explosionで生成されたオブジェクトにRigidBodyがありません");
                }
            }
        }
        private void Spawn2DBullet()
        {
            for (int i = 0; i < bullet2DNumber; i++)
            {
                Vector3 pos = transform.position;
                pos.y = 0;
                GameObject bullet = Instantiate(bullet2DPrefab, pos, Quaternion.Euler(90,0,0));

                Rigidbody rb = bullet.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    Vector2 randomDir2D = UnityEngine.Random.insideUnitCircle.normalized;
                    Vector3 randomDirection = new Vector3(randomDir2D.x, 0, randomDir2D.y);

                    rb.AddForce(randomDirection * explosionForce, ForceMode.Impulse);
                }
                else
                {
                    Debug.LogWarning("Explosionで生成されたオブジェクトにRigidBodyがありません");
                }
            }
        }
    }
}