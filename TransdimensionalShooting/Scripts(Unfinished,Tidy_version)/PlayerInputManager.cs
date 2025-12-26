using UnityEngine;
using UnityEngine.InputSystem;

namespace Taiyaki
{
    public class PlayerInputManager : MonoBehaviour
    {
        private TaiyakiActions inputActions;

        private void Awake()
        {
            inputActions = new TaiyakiActions();
        }

        private void OnEnable()
        {
            inputActions.Player.Enable();

            inputActions.Player.Shoot.performed += OnShoot;
            inputActions.Player.ChangeMode.performed += OnChangeMode;
        }

        private void OnDisable()
        {
            inputActions.Player.Shoot.performed -= OnShoot;
            inputActions.Player.ChangeMode.performed -= OnChangeMode;

            inputActions.Player.Disable();
        }
        private void Update()
        {
            Vector2 currentMove = inputActions.Player.Move.ReadValue<Vector2>();
            EventBus.PublishMove(currentMove);
        }

        private void OnShoot(InputAction.CallbackContext context)
        {
            var pointer = Pointer.current; // 現在のマウス/タッチ
            if (pointer != null)
            {
                Vector2 mousePos = pointer.position.ReadValue(); // スクリーン座標
                EventBus.PublishShoot(mousePos);
            }
            else
            {
                EventBus.PublishShoot(Vector2.zero);
                Debug.LogWarning("現在のマウス/タッチを取得できませんでした");
            }
        }

        private void OnChangeMode(InputAction.CallbackContext context)
        {
            EventBus.PublishSpacePressed();
        }
    }
}