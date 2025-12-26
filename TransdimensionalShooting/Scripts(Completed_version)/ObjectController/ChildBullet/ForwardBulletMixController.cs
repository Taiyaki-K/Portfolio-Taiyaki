using UnityEngine;

namespace Taiyaki
{
    // Rigidbodyコンポーネントを必須にする
    [RequireComponent(typeof(Rigidbody))]
    public class ForwardBulletMixController : MonoBehaviour
    {
        [Header("弾の速度")]
        [SerializeField]
        private float speed = 20f;

        private Rigidbody rb;

        private void Awake()
        {
            // 最初に Rigidbody を取得する
            rb = GetComponent<Rigidbody>();
        }

        // 物理演算の更新は FixedUpdate で行う
        private void FixedUpdate()
        {
            // Rigidbody の速度(velocity)を直接設定する
            // これにより、他の力（重力や空気抵抗）を無視して、
            // 常に「自身のローカル上方向（transform.up）」に「speed」の速さで飛び続ける
            rb.linearVelocity = transform.up * speed;
        }
    }
}