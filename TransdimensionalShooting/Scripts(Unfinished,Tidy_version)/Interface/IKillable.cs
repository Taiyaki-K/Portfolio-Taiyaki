namespace Taiyaki
{
    /// <summary>
    /// これを実装するオブジェクトは、
    /// 弾などが当たった時に即死（またはヒット）処理を呼び出される
    /// </summary>
    public interface IKillable
    {
        void OnKill();
    }
}