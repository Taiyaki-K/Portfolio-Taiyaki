using System;
using UnityEngine;

namespace Taiyaki
{
    [RequireComponent(typeof(ObjectModeDual))]
    public class BeamRootController : MonoBehaviour,IBeamInit
    {
        //変数
        private ObjectModeDual dualMode;
        private GameObject object3D;
        private GameObject object2D;

        //Unityイベント関数
        private void Awake()
        {
            dualMode = GetComponent<ObjectModeDual>();
            object3D = dualMode.Object3D;
            object2D = dualMode.Object2D;
        }
        private void OnEnable()
        {
            //2Dと3D切り替え時に特殊処理追加
            EventBus.OnModeChanged += CustomProjection;
        }
        private void OnDisable()
        {
            EventBus.OnModeChanged -= CustomProjection;
        }
        //関数
        public void Initialize(Vector3 position, Vector3 direction)
        {
            if (GameModeManager.Instance.Is2DMode)
            {
                position.y = 0;
                object2D.transform.position = position;
                object2D.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);
            }
            else
            {
                object3D.transform.position = position;
                object3D.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);
            }
        }

        private void CustomProjection(bool is2DMode)
        {
            if (is2DMode)
            {
                // --- 3D -> 2D への切り替え ---
                Vector3 direction3D = object3D.transform.up;
                Vector3 direction2D = new Vector3(direction3D.x, 0, direction3D.z).normalized;

                // 進行方向が真上/真下だった場合（エラー防止）
                if (direction2D == Vector3.zero)
                {
                    direction2D = Vector3.forward;
                }

                Quaternion rot = Quaternion.LookRotation(direction2D) * Quaternion.Euler(90, 0, 0);
                Vector3 pos = new Vector3(object3D.transform.position.x, 0, object3D.transform.position.z);
                object2D.transform.position = pos;
                object2D.transform.rotation = rot;
            }
            else
            {
                // --- 2D -> 3D への切り替え ---
                Vector3 direction2D = object2D.transform.up;

                Quaternion rot = Quaternion.LookRotation(direction2D) * Quaternion.Euler(90, 0, 0);
                Vector3 pos = new Vector3(object2D.transform.position.x, 0, object2D.transform.position.z);
                object3D.transform.position = pos;
                object3D.transform.rotation = rot;
            }
        }
    }
}