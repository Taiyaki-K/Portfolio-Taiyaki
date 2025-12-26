using UnityEngine;

public static class ProcessLvAndExp
{
    private static int GetRequiredExp(int lv)
    {
        return 10 * lv * lv;
    }

    // 累計expからレベルを返す
    public static int CalculateLv(int cumulativeExp)
    {
        int tempExp = cumulativeExp;
        int lv = 1;

        while (true)
        {
            int need = GetRequiredExp(lv);
            if (tempExp >= need)
            {
                tempExp -= need;
                lv++;
                if (lv > 30) // 最大レベル制限
                {
                    lv = 30;
                    break;
                }
            }
            else break;
        }

        return lv;
    }

    public static int GetNeedExpToNextLv(int cumulativeExp)
    {
        int currentLevel = CalculateLv(cumulativeExp);

        if (currentLevel >= 30) return 0; // 最大レベルなら0

        int totalExpForNext = GetRequiredExp(currentLevel);
        int expToNext = totalExpForNext - (cumulativeExp - GetTotalExpUpToLevel(currentLevel));

        return expToNext;
    }

    // 現在のレベルまでに必要な累計経験値を計算する補助関数
    private static int GetTotalExpUpToLevel(int lv)
    {
        int total = 0;
        for (int i = 1; i < lv; i++)
        {
            total += GetRequiredExp(i);
        }
        return total;
    }
}
