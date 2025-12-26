using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Taiyaki
{
    [RequireComponent(typeof(Rigidbody))]
    public class HomingBullet2DController : MonoBehaviour
    {
        [Header("2D設定")]
        [SerializeField] private float moveSpeed = 30.0f;
        [SerializeField] private float turnSpeed = 10.0f;
        [SerializeField] private float targetOffset = 0f; // 2Dでは通常 0

        [SerializeField] private Transform target2D;
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // --- 「グローバルY位置」と「グローバルXZ回転」を止める設定 ---
            // (オブジェクトがローカルX軸で90度回転している前提)
            // 2Dなので、グローバルにおけるY軸の物理移動と、X/Z軸の物理回転をロックする

            // 止めるもの:
            // 1. グローバルY位置 = グローバルY位置
            // 2. グローバルX回転 = ローカルX回転
            // 3. グローバルZ回転 = ローカルY回転
            //
            // 許可するもの (2Dの動き):
            // 1. グローバルXZ移動 (ローカルXY移動)
            // 2. グローバルY回転 (ローカルZ回転) <- ホーミングの回転
            rb.constraints = RigidbodyConstraints.FreezePositionY |
                             RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationY;
        }

        public void SetTarget(Transform newTarget)
        {
            target2D = newTarget;
        }

        private void FixedUpdate()
        {
            if (target2D == null) return;

            // --- 2Dホーミング処理 (Y軸を0として計算) ---
            Vector3 targetPoint = target2D.position + (Vector3.up * targetOffset);
            targetPoint.y = 0; // 2D

            Vector3 rbPosition = rb.position;
            rbPosition.y = 0; // 2D

            Vector3 directionToTarget = (targetPoint - rbPosition).normalized;

            if (directionToTarget == Vector3.zero) return;

            // Y軸回転（水平方向の向き）のみ
            Quaternion zAxisRotation = Quaternion.LookRotation(directionToTarget);
            Quaternion targetRotation = Quaternion.Euler(90, zAxisRotation.eulerAngles.y, 0);

            Quaternion newRotation = Quaternion.RotateTowards(
                rb.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime * 10f
            );

            rb.MoveRotation(newRotation);

            // 速度のY軸も0に固定
            Vector3 newPosition = rb.position + (transform.up * moveSpeed);
            newPosition.y = 0;
            rb.MovePosition(newPosition);
        }
    }
}