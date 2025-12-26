using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DebugTools
{
    public static bool IsValidIndex<T>(T[] array, int index)
    {
        return !(index < 0 || array == null || index >= array.Length);
    }

    public static bool IsValidIndex<T>(List<T> list, int index)
    {
        return !(index < 0 || list == null || index >= list.Count);
    }

    public static void MakeShotLog(int shooterId,int _targetId, BulletType bullet)
    {
        Debug.Log($"{shooterId}‚ª{_targetId}‚É{bullet}‚ğŒ‚‚Á‚½");
    }

    public static void MakeBulletLog()
    {
        Queue<BulletType> bullets = BattleSceneManager.Instance.currentBattleManager.bulletManager.bullets;
        int liveCount = bullets.Count(b => b == BulletType.Live);
        int emptyCount = bullets.Count(b => b == BulletType.Empty);
        Debug.Log($"À’eF{liveCount}@‹ó–CF{emptyCount}");
    }
}
