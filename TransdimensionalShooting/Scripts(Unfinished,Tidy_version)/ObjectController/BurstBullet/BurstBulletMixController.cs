using Taiyaki;
using UnityEngine;

namespace Taiyaki
{
    [RequireComponent(typeof(Rigidbody))]
    public class BurstBulletMixController : MonoBehaviour
    {

        private Rigidbody rb;
        private BurstBulletRootController burstBulletRoot;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            burstBulletRoot = GetComponentInParent<BurstBulletRootController>();
        }

        private void FixedUpdate()
        {
            if (burstBulletRoot.HasBurst) return;

            // 1. 減速処理
            burstBulletRoot.CurrentSpeed -= burstBulletRoot.DecelerationRate * Time.fixedDeltaTime;

            if (burstBulletRoot.CurrentSpeed <= 0f)
            {
                // 2. 停止したら、一度だけ発射処理を呼ぶ
                burstBulletRoot.CurrentSpeed = 0f;
                burstBulletRoot.HasBurst = true;
                rb.linearVelocity = Vector3.zero; // 完全に停止

                // 3. あとはRootのクラスで、BurstからDestroyまで処理
                burstBulletRoot.TriggerBurstAndDestroy(transform.position);
            }
            else
            {
                // 速度を更新
                rb.linearVelocity = transform.up * burstBulletRoot.CurrentSpeed;
            }
        }
    }
}