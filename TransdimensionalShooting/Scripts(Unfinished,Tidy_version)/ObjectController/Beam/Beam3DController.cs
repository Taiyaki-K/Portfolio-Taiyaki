using UnityEngine;

namespace Taiyaki
{
    public class Beam3DController : MonoBehaviour
    {
        [SerializeField] float speed = 50f;
        private void Update()
        {
            transform.position += transform.up * speed * Time.deltaTime;
        }
    }
}