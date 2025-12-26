using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Taiyaki
{
    [RequireComponent(typeof(Rigidbody))]
    public class HomingBullet3DController : MonoBehaviour
    {
        [Header("3D設定")]
        [SerializeField] private float moveSpeed = 30.0f;
        [SerializeField] private float turnSpeed = 10.0f;
        [SerializeField] private float targetOffset = 0f;

        [SerializeField] private Transform target3D;
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void SetTarget(Transform newTarget)
        {
            target3D = newTarget;
        }

        private void FixedUpdate()
        {
            if (target3D == null) return; // まっすぐ飛ぶ

            // --- 3Dホーミング処理 ---
            Vector3 targetPoint = target3D.position + (Vector3.up * targetOffset);
            Vector3 directionToTarget = (targetPoint - rb.position).normalized;

            if (directionToTarget == Vector3.zero) return;

            Quaternion zAxisRotation = Quaternion.LookRotation(directionToTarget);
            Quaternion yAxisOffset = Quaternion.Euler(90, 0, 0);
            Quaternion targetRotation = zAxisRotation * yAxisOffset;

            Quaternion newRotation = Quaternion.RotateTowards(
                rb.rotation, //今の自分の回転
                targetRotation, //目指す回転
                turnSpeed * Time.fixedDeltaTime * 10f //一回で回転できる最大量
            );
            rb.MoveRotation(newRotation);

            Vector3 newPosition = transform.position + (transform.up * moveSpeed);
            rb.MovePosition(newPosition);
        }
    }
}