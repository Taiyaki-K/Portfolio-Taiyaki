using UnityEngine;

namespace Taiyaki
{
    public class Player2DController : MonoBehaviour
    {
        [SerializeField] private float speedXZ;//上下左右移動の速さ(操作)
        [SerializeField] private float xLimit;
        [SerializeField] private float zMaxLimit;
        [SerializeField] private float zMinLimit;
        [Header("beamRootにはIBeamが割りついてること")]
        [SerializeField] private GameObject beamRootPrefab;
        //EventBusから受け取ったVector2を保持
        private Vector2 currentMoveInput;

        private void OnEnable()
        {
            EventBus.OnMove += StoreMoveInput;
            EventBus.OnShoot += Shoot;
        }
        private void OnDisable()
        {
            EventBus.OnMove -= StoreMoveInput;
            EventBus.OnShoot -= Shoot;
        }
        private void StoreMoveInput(Vector2 move)
        {
            currentMoveInput = move;
        }
        private void Update()
        {
            float x = currentMoveInput.x;
            float z = currentMoveInput.y;
            //移動と回転
            transform.position += new Vector3(x * speedXZ, 0, z * speedXZ) * Time.deltaTime;

            //移動制限(xのみ)
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, -xLimit, xLimit);
            clampedPosition.z = Mathf.Clamp(clampedPosition.z, zMinLimit, zMaxLimit);
            transform.position = clampedPosition;
        }

        private void Shoot(Vector2 mousePosition)
        {
            //mousePositionはOnShootに追加するためだけにつけてる

            //Player2Dは90度回転してるので、transform.upを使う
            Vector3 firePoint = transform.position + transform.up * 2f;
            // プレイヤー → 目標方向
            Vector3 direction = transform.up;
            //rootを生成
            GameObject br = Instantiate(beamRootPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero));
            //object3Dの場所、向きの設定
            IBeamInit bi = br.GetComponent<IBeamInit>();
            bi.Initialize(firePoint, direction);
        }
    }
}