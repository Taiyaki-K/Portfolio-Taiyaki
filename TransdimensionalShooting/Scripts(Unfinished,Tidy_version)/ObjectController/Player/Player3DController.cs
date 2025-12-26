using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Taiyaki
{
    public class Player3DController : MonoBehaviour
    {
        [SerializeField] private float speedZ;//z軸移動の速さ(固定)
        [SerializeField] private float speedXY;//上下左右移動の速さ(操作)
        [SerializeField] private float rotate;//回転の強度
        [SerializeField] private Vector3 moveLimit;//移動制限
        [SerializeField] private float smoothTime;//move値を1にするのにどれくらい時間がかかるか
        [Header("beamRootにはIBeamが割りついてること")]
        [SerializeField] private GameObject beamRootPrefab;
        //EventBusから受け取ったVector2を保持
        private Vector2 currentMoveInput;
        //実際に反映させる滑らかな値
        private Vector2 actualMoveValue;

        private void OnEnable()
        {
            EventBus.OnMove += StoreMoveInput;
            EventBus.OnShoot += Shoot;
            actualMoveValue = Vector2.zero;
        }
        private void OnDisable()
        {
            // 3. 登録解除
            EventBus.OnMove -= StoreMoveInput;
            EventBus.OnShoot -= Shoot;
        }
        private void StoreMoveInput(Vector2 move)
        {
            currentMoveInput = move;
        }

        private void Update()
        {
            //なめらかにmove値を変化
            actualMoveValue.x = Mathf.MoveTowards(actualMoveValue.x, currentMoveInput.x, smoothTime * Time.deltaTime);
            actualMoveValue.y = Mathf.MoveTowards(actualMoveValue.y, currentMoveInput.y, smoothTime * Time.deltaTime);
            float x = actualMoveValue.x;
            float y = actualMoveValue.y;

            //移動と回転
            transform.position += new Vector3(x * speedXY, y * speedXY, speedZ) * Time.deltaTime;
            transform.rotation = Quaternion.Euler(-y * rotate, x * rotate, -x * rotate);

            //移動制限
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, -moveLimit.x, moveLimit.x);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, -moveLimit.y, moveLimit.y);
            transform.position = clampedPosition;
        }

        private void Shoot(Vector2 mousePosition)
        {
            Vector3 firePoint = transform.position + transform.forward * 2f;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(100f);
            }

            // プレイヤー → 目標方向
            Vector3 direction = (targetPoint - firePoint).normalized;
            //rootを生成
            GameObject br = Instantiate(beamRootPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero));
            //object3Dの場所、向きの設定
            IBeamInit bi = br.GetComponent<IBeamInit>();
            bi.Initialize(firePoint, direction);
        }
    }
}