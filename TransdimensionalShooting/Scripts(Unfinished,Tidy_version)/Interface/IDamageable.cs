namespace Taiyaki
{
    /// <summary>
    /// HPを持っており、ダメージ（数値）を受け取ることができる
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }
}