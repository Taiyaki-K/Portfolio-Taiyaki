using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BulletManager
{
    public Queue<BulletType> bullets = new Queue<BulletType>();
    private const int MIN_BULLETS = 3;
    private const int MAX_BULLETS = 10;

    public void SetRandomBulletSequence()
    {
        List<BulletType> bulletsList = new List<BulletType>();//この関数内のみでの初期化のための弾のList
        int totalBullets = Random.Range(MIN_BULLETS, MAX_BULLETS + 1);
        int minLiveBullets = Mathf.CeilToInt(totalBullets / 3f);
        int maxLiveBullets = Mathf.CeilToInt(totalBullets * 2 / 3f);
        int liveBullets = Random.Range(minLiveBullets, maxLiveBullets + 1);
        int emptyBullets = totalBullets - liveBullets;
        for (int i = 0; i < liveBullets; i++) bulletsList.Add(BulletType.Live);
        for (int i = 0; i < emptyBullets; i++) bulletsList.Add(BulletType.Empty);
        ShuffleList(bulletsList);
        bullets = new Queue<BulletType>(bulletsList);
    }

    //シャッフルアルゴリズム
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }


    //実際に撃つ
    public BulletType Fire()
    {
        if (bullets.Count == 0) return BulletType.None;
        return bullets.Dequeue();
    }

    //実際には撃たないが今の最初の弾を確認
    public BulletType PeekNextBullet()
    {
        return bullets.Count > 0 ? bullets.Peek() : BulletType.None;
    }
}