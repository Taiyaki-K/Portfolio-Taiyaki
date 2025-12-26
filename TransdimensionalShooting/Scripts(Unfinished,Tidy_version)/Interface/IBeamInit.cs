using UnityEngine;
using UnityEngine.UIElements;

namespace Taiyaki
{
    public interface IBeamInit
    {
        void Initialize(Vector3 position, Vector3 direction);
    }
}