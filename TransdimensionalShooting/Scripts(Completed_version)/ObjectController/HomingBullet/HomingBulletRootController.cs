using UnityEngine;

namespace Taiyaki
{
    [RequireComponent(typeof(ObjectModeDual))]
    public class HomingBulletRootController : MonoBehaviour, ISetHomingTarget
    {
        private ObjectModeDual dualMode;
        private GameObject object3D;
        private GameObject object2D;
        private HomingBullet3DController controller3D;
        private HomingBullet2DController controller2D;
        private void Awake()
        {
            dualMode = GetComponent<ObjectModeDual>();
            object3D = dualMode.Object3D;
            object2D = dualMode.Object2D;
            controller3D = object3D.GetComponent<HomingBullet3DController>();
            controller2D = object2D.GetComponent<HomingBullet2DController>();
        }
        private void OnEnable()
        {
            EventBus.OnModeChanged += CustomProjection;
        }
        private void OnDisable()
        {
            EventBus.OnModeChanged -= CustomProjection;
        }

        /// <summary>
        /// スポナー（例：Doragon）は、InstantiateしたRootのこの関数を呼ぶだけ
        /// </summary>
        public void InitializeTargets(Transform newTarget3D, Transform newTarget2D)
        {
            // 2Dと3Dの両方にターゲットをセットする
            controller3D?.SetTarget(newTarget3D);
            controller2D?.SetTarget(newTarget2D);
        }

        private void CustomProjection(bool is2DMode)
        {
            if (is2DMode)
            {
                Vector3 pos = object3D.transform.position;
                object2D.transform.position = new Vector3(pos.x, 0, pos.z);
                Quaternion rot = Quaternion.Euler(0, object3D.transform.rotation.eulerAngles.y, 0);
                object2D.transform.rotation = rot;
            }
            else
            {
                Vector3 pos = object2D.transform.position;
                object3D.transform.position = new Vector3(pos.x, 0, pos.z);
                Quaternion rot = Quaternion.Euler(0, object2D.transform.rotation.eulerAngles.y, 0);
                object3D.transform.rotation = rot;
            }
        }
    }
}