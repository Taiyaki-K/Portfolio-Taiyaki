using Taiyaki;
using UnityEngine;

namespace Taiyaki
{
    public enum ActiveMode
    {
        ActiveIn2D, // 2Dモードで有効
        ActiveIn3D  // 3Dモードで有効
    }
    // 2Dモードでのみ表示・衝突するオブジェクト
    public class ObjectModeSingle : MonoBehaviour, IDestructible
    {
        [Header("モード設定")]
        [Tooltip("このオブジェクトが、どちらのモードで有効になるか")]
        [SerializeField] private ActiveMode activeMode = ActiveMode.ActiveIn3D;

        private Renderer myRenderer;
        private Collider myCollider;
        // (もしあれば、ParticleSystem や Light なども対象)

        private void Awake()
        {
            myRenderer = GetComponent<Renderer>();
            myCollider = GetComponent<Collider>();
        }
        private void Start()
        {
            // 起動時のモードで初期化
            HandleModeChanged(GameModeManager.Instance.Is2DMode);
        }
        private void OnEnable()
        {
            EventBus.OnModeChanged += HandleModeChanged;
        }

        private void OnDisable()
        {
            EventBus.OnModeChanged -= HandleModeChanged;
        }

        public void DestroySelf()
        {
            if (this == null) return;

            Destroy(gameObject);
        }
        private void HandleModeChanged(bool is2DMode)
        {
            bool isVisible;
            if (activeMode == ActiveMode.ActiveIn2D)
            {
                isVisible = is2DMode; // 2Dの時 true
            }
            else // activeMode == ActiveMode.ActiveIn3D
            {
                isVisible = !is2DMode; // 3Dの時 true
            }

            if (myRenderer != null) 
                myRenderer.enabled = isVisible;
            if (myCollider != null) 
                myCollider.enabled = isVisible;
        }
    }
}