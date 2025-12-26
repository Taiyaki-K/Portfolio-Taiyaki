using UnityEngine;

namespace Taiyaki
{
    public class CameraSwitcher : MonoBehaviour
    {
        [SerializeField] private Camera perspCam;
        [SerializeField] private Camera orthoCam;

        private void Awake()
        {
            // ãNìÆéûÇÃèâä˙âª
            if (GameModeManager.Instance != null)
            {
                SwitchCamera(GameModeManager.Instance.Is2DMode);
            }
        }

        private void OnEnable()
        {
            EventBus.OnModeChanged += SwitchCamera;
        }

        private void OnDisable()
        {
            EventBus.OnModeChanged -= SwitchCamera;
        }

        private void SwitchCamera(bool _is2DMode)
        {
            if (_is2DMode)
            {
                perspCam.gameObject.SetActive(false);
                orthoCam.gameObject.SetActive(true);
            }
            else
            {
                perspCam.gameObject.SetActive(true);
                orthoCam.gameObject.SetActive(false);
            }
        }
    }
}