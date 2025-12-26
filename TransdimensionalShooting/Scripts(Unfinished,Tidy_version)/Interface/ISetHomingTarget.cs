using UnityEngine;

namespace Taiyaki
{
    /// <summary>
    /// ホーミング弾のターゲットを設定するためのインターフェース
    /// （3D用と2D用の両方を受け取る）
    /// </summary>
    public interface ISetHomingTarget
    {
        void InitializeTargets(Transform newTarget3D, Transform newTarget2D);
    }
}