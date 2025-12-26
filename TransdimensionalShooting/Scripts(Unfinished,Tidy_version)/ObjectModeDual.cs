using System;
using UnityEngine;

namespace Taiyaki
{
    public class ObjectModeDual : MonoBehaviour, IDestructible
    {
        //変数
        [SerializeField] private GameObject object3D;
        [SerializeField] private GameObject object2D;

        //プロパティ
        public GameObject Object3D => object3D;
        public GameObject Object2D => object2D;

        //Unityイベント関数
        private void Awake()
        {
            // 生成時に現在のモードで初期化
            SetMode(GameModeManager.Instance.Is2DMode);
        }
        private void OnEnable()
        {
            EventBus.OnModeChanged += SetMode;
        }
        private void OnDisable()
        {
            EventBus.OnModeChanged -= SetMode;
        }

        //消去時
        public void DestroySelf()
        {
            if (this == null) return;

            if (object3D != null)
                Destroy(object3D);
            if (object2D != null)
                Destroy(object2D);

            Destroy(gameObject);
        }

        //2Dと3D切り替え
        private void SetMode(bool is2DMode)
        {
            object2D.SetActive(is2DMode);
            object3D.SetActive(!is2DMode);
        }
    }
}