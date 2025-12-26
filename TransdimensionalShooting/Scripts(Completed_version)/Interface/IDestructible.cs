namespace Taiyaki
{
    /// <summary>
    /// オブジェクトが自身の破棄ロジックを定義するためのインターフェース
    /// </summary>
    public interface IDestructible
    {
        void DestroySelf();
    }
}