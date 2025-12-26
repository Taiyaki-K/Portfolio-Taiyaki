using System;
using UnityEngine;

namespace Taiyaki
{
    public static class EventBus
    {
        //移動入力(wasd)
        public static event Action<Vector2> OnMove;
        public static void PublishMove(Vector2 move)
        {
            OnMove?.Invoke(move);
        }
        //弾丸発射
        public static event Action<Vector2> OnShoot;
        public static void PublishShoot(Vector2 mousePosition)
        {
            OnShoot?.Invoke(mousePosition);
        }
        //「2D/3Dモードが切り替わった」というイベント(Space押下時)
        public static event Action OnSpacePressed;
        public static void PublishSpacePressed()
        {
            OnSpacePressed?.Invoke();
        }
        public static event Action<bool> OnModeChanged;
        public static void PublishModeChanged(bool is2DMode)
        {
            OnModeChanged?.Invoke(is2DMode);
        }
        //死亡時
        public static event Action OnPlayerDied;
        public static void PublishPlayerDied()
        {
            OnPlayerDied?.Invoke();
        }
        //DragonのHPが初期化された時
        public static event Action<int> OnDragonHpInitialized;
        public static void PublishDragonHpInitialized(int maxHp)
        {
            OnDragonHpInitialized?.Invoke(maxHp);
        }
        //Dragonの体力が減った時
        public static event Action<int> OnTakeDragonDamage;
        public static void PublishTakeDragonDamage(int dragonHp)
        {
            OnTakeDragonDamage?.Invoke(dragonHp);
        }
        //ドラゴン死亡時
        public static event Action OnDragonDied;
        public static void PublishDragonDied()
        {
            OnDragonDied?.Invoke();
        }
        // モード切替のクールダウンが開始された
        public static event Action<float> OnModeCooldownStarted;
        public static void PublishModeCooldownStarted(float duration)
        {
            OnModeCooldownStarted?.Invoke(duration);
        }
    }
}