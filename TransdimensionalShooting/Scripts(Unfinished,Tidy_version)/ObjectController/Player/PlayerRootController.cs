using UnityEngine;

namespace Taiyaki
{
    [RequireComponent(typeof(ObjectModeDual))]
    public class PlayerRootController : MonoBehaviour, IKillable
    {
        [SerializeField] private GameObject deathEffect;

        private ObjectModeDual dualMode;
        private GameObject object3D;
        private GameObject object2D;

        private void Awake()
        {
            dualMode = GetComponent<ObjectModeDual>();
            object3D = dualMode.Object3D;
            object2D = dualMode.Object2D;
        }
        private void OnEnable()
        {
            EventBus.OnModeChanged += CustomProjection;
            EventBus.OnPlayerDied += PlayDeathDirection;
        }
        private void OnDisable()
        {
            EventBus.OnModeChanged -= CustomProjection;
            EventBus.OnPlayerDied -= PlayDeathDirection;
        }

        public void OnKill()
        {
            EventBus.PublishPlayerDied();
        }

        private void CustomProjection(bool is2DMode)
        {
            if (is2DMode)
            {
                Vector3 pos = object3D.transform.position;
                object2D.transform.position = new Vector3(pos.x, 0, pos.z);
            }
            else
            {
                Vector3 pos = object2D.transform.position;
                object3D.transform.position = new Vector3(pos.x, 0, pos.z);
            }
        }

        private void PlayDeathDirection()
        {
            //エフェクト出す位置を決めるため、現在のモードのPlayerを取る
            GameObject currentPlayerObj;
            if (GameModeManager.Instance.Is2DMode)
            { currentPlayerObj = object2D; }
            else
            { currentPlayerObj = object3D; }

            Renderer[] allRenderers = gameObject.GetComponentsInChildren<Renderer>();
            Collider[] allCollider = gameObject.GetComponentsInChildren<Collider>();

            foreach (Renderer renderer in allRenderers)
            {
                renderer.enabled = false;
            }
            foreach (Collider collider in allCollider)
            {
                collider.enabled = false;
            }

            Instantiate(deathEffect, currentPlayerObj.transform.position, Quaternion.identity);
        }
    }
}