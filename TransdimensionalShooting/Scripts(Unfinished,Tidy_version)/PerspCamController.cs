using UnityEngine;

namespace Taiyaki
{
    public class PerspCamController : MonoBehaviour
    {
        [Header("フォローする対象")]
        [SerializeField] private Transform target; // プレイヤーのTransform

        [Header("カメラの基本設定")]
        [SerializeField] private float distance = 5.0f; // ターゲットからの距離
        [SerializeField] private float heightOffset = 1.5f; // ターゲットの「高さ」オフセット（この地点をカメラが見つめる）

        [Header("マウス感度")]
        [SerializeField] private float sensitivityX = 200.0f;
        [SerializeField] private float sensitivityY = 200.0f;

        [Header("角度制限")]
        [SerializeField] private float minVerticalAngle = -40.0f; // 見下ろす角度（マイナス値）
        [SerializeField] private float maxVerticalAngle = 80.0f;  // 見上げる角度

        // 現在のカメラの回転角度
        private float currentRotationX = 0.0f;
        private float currentRotationY = 0.0f;

        private void Start()
        {
            // マウスカーソルを非表示にして、画面中央にロックする
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            // ターゲットが設定されていなければ処理を中断
            if (target == null)
            {
                return;
            }

            // 1. マウスの入力を取得
            // Mouse X（左右）の動きは、Y軸周りの回転
            currentRotationY += Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
            // Mouse Y（上下）の動きは、X軸周りの回転
            currentRotationX -= Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime;

            // 2. 回転角度を制限する
            currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);

            // 3. 回転と位置を計算
            // オイラー角（X, Y, Z）からクォータニオン（回転）を生成
            Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);

            // カメラが「見つめるべきポイント」を計算
            // ターゲットの座標 + 高さオフセット（例: プレイヤーの頭上）
            Vector3 targetLookAtPoint = target.position + Vector3.up * heightOffset;

            // 4. カメラの位置を計算
            // 「見つめるポイント」から、回転（rotation）の「後ろ(Vector3.forward)」に「距離(distance)」だけ離れた位置
            Vector3 cameraPosition = targetLookAtPoint - (rotation * Vector3.forward * distance);

            // 5. カメラのTransformに適用
            transform.position = cameraPosition;
            transform.LookAt(targetLookAtPoint); // 常に「見つめるポイント」を見る
        }
    }
}