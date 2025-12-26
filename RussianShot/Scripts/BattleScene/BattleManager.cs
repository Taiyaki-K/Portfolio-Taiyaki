using Unity.VisualScripting;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using KanKikuchi.AudioManager;
public class BattleManager
{
    [System.NonSerialized] public BulletManager bulletManager;
    [System.NonSerialized] public GameObject[] characters;
    [System.NonSerialized] public bool isValidMuzzle;
    [System.NonSerialized] public int currentActionCharacterId = 0;//0からcharacters.Count - 1の間
    private int round = 1;//何周したか（１から）
    private Vector3[] characterPositions = new Vector3[4] //各キャラのlocalPosition
    {
        new Vector3(0, 0, -4.5f),
        new Vector3(-4.5f, 0, 0),
        new Vector3(0, 0, 4.5f),
        new Vector3(4.5f, 0, 0)
    };
    //RunBattleAsyncで使用
    private CharacterChoice currentChoice;
    private bool EmptyOrFakeShot;
    private bool consecutiveTurns;
    public GameObject shotGunMuzzle;
    //コンストラクタ
    public BattleManager()
    {
        //bulletのセットアップ
        bulletManager = new BulletManager();
        //charactersのセットアップ
        characters = new GameObject[4];
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i] = UnityEngine.Object.Instantiate(GameManager.Instance.reservedCharacterCapacities[i].selfObject, characterPositions[i], Quaternion.identity);

            characters[i].GetComponent<Character>().Initialize(GameManager.Instance.reservedCharacterCapacities[i], i);
            characters[i].GetComponent<CharacterAnimation>().Initialize(i);
            characters[i].GetComponent<CharacterItem>().Initialize(i);
            characters[i].GetComponent<CharacterSkill>().Initialize(this);

            characters[i].AddComponent<CharacterHate>().Initialize();

            characters[i].GetComponent<CharacterAnimation>().LookCenter();
            if (i != 0)
            {
                characters[i].AddComponent<NPCRunTurn>().Initialize();
            }
        }
    }

    //メインループ
    public async UniTask RunBattleAsync()
    {
        bool isFirst = true;
        while (true)
        {
            EmptyOrFakeShot = false;
            isValidMuzzle = false;

            if (IsBattleOver()) break;
            if (!consecutiveTurns&&!isFirst)
            { await NextTurnAndSkillCheck(); }
            if (IsBattleOver()) break;
            isFirst = false;
            await CheckBulletLoadingAndAddItems();
            await RunTurnAsync();
            await ShotOrFakeShotAsync();
            SEManager.Instance.Play(SEPath.PUT_GUN);
            CalculateHateInShot();

            if (shotGunMuzzle != null)
            {UnityEngine.Object.Destroy(shotGunMuzzle);}

            await UniTask.WaitForSeconds(0.5f);
            if (currentActionCharacterId == currentChoice.targetId && EmptyOrFakeShot)
            {
                consecutiveTurns = true;
                continue;
            }
            consecutiveTurns = false;
        }
        Debug.Log("バトル終了");
        if (characters[0].GetComponent<Character>().characterCurrentStatus.isAlive)
        {
            if (GameManager.Instance.currentStageNum == 10)
            {
                BattleSceneManager.Instance.battlePostProcess.PlayEndingAnimation();
            }
            else
            {
                await BattleSceneManager.Instance.battlePostProcess.PlayVictoryAnimation();
                BattleSceneManager.Instance.battlePostProcess.ProcessVictory();
                BattleSceneManager.Instance.battlePostProcess.DisplayVictoryScreen();
            }
        }
    }

    private bool IsBattleOver()
    {
        int aliveCount = characters.Count(c => c.GetComponent<Character>().characterCurrentStatus.isAlive);
        return aliveCount <= 1 || !characters[0].GetComponent<Character>().characterCurrentStatus.isAlive;
    }

    public async UniTask CheckBulletLoadingAndAddItems()
    {
        if (bulletManager.bullets.Count == 0)
        {
            bulletManager.SetRandomBulletSequence();
            int liveCount = bulletManager.bullets.Count(b => b == BulletType.Live);
            int emptyCount = bulletManager.bullets.Count(b => b == BulletType.Empty);
            BattleSceneManager.Instance.battleUIManager.AdjustBulletAnalysisField();
            await BattleSceneManager.Instance.animationManager.PlayBulletLoadingAnimation(liveCount, emptyCount);
            Debug.Log("弾が補充されました");
            foreach (GameObject chara in characters)
            {
                if(!chara.GetComponent<Character>().characterCurrentStatus.isAlive) 
                { continue; }
                int itemNum = GetWeightedRandomByFortune(chara.GetComponent<Character>().characterCapacity.Fortune);
                chara.GetComponent<CharacterItem>().AddItems(itemNum);
            }
        }
        var skillDict = characters[0].GetComponent<CharacterSkill>().SkillUsedDictionary;
        if (skillDict.ContainsKey(SkillType.BulletAnalysis) &&
            skillDict[SkillType.BulletAnalysis] == SkillState.InUse)
        {
            BattleSceneManager.Instance.battleUIManager.AdjustBulletAnalysisField();
        }
        DebugTools.MakeBulletLog();
    }
    int GetWeightedRandomByFortune(int fortune)
    {
        // 0~10 の Fortune を 1~3 の target に線形マップ
        float target = 1f + (fortune / 10f) * 2f; // 1～3の間の小数

        int[] candidates = { 1, 2, 3 };
        List<float> weights = new List<float>();

        foreach (int i in candidates)
        {
            // target との差を距離として扱う
            float distance = Mathf.Abs(i - target);
            float weight = Mathf.Pow(0.2f, distance); // 距離が大きいほど小さい
            weights.Add(weight);
        }

        float totalWeight = weights.Sum();
        float r = UnityEngine.Random.value * totalWeight;

        float accum = 0f;
        for (int i = 0; i < candidates.Length; i++)
        {
            accum += weights[i];
            if (r <= accum) return candidates[i];
        }

        return candidates[candidates.Length - 1];
    }

    private async UniTask RunTurnAsync()
    {
        if (currentActionCharacterId == 0) { currentChoice = await BattleSceneManager.Instance.battleUIManager.RunPlayerTurnAsync(); }
        else { currentChoice = await characters[currentActionCharacterId].GetComponent<Character>().RunNPCTurnAsync(); }
    }

    private void CalculateHateInShot()
    {
        if (currentActionCharacterId == currentChoice.targetId)
        {
            characters[currentActionCharacterId].GetComponent<CharacterHate>().OnShotSelf();
        }
        else
        {
            if(!consecutiveTurns)
            characters[currentActionCharacterId].GetComponent<CharacterHate>().OnShotOther();
        }
    }

    private async UniTask ShotOrFakeShotAsync()
    {
        switch (currentChoice.actionType)
        {
            case ActionType.Shot:
                await ShotAsync(currentChoice.targetId);
                break;
            case ActionType.FakeShot:
                EmptyOrFakeShot = true;
                await characters[currentActionCharacterId].GetComponent<CharacterSkill>().FakeShot(currentChoice.targetId);
                break;
            default:
                Debug.LogWarning($"ターン実行でShotとFakeShot以外が指定されました：{currentChoice.actionType}");
                break;
        }
    }

    public async UniTask NextTurnAndSkillCheck()
    {
        do
        {
            if ((currentActionCharacterId + 1) == characters.Length)
            {
                currentActionCharacterId = 0;
                round++;
                CalculateHateInRound();
            }
            else
            {
                currentActionCharacterId++;
            }
            await characters[currentActionCharacterId].GetComponent<CharacterSkill>().CheckSkillActive();
        } while (!characters[currentActionCharacterId].GetComponent<Character>().characterCurrentStatus.isAlive);
    }


    private async UniTask ShotAsync(int targetId)
    {
        if(!DebugTools.IsValidIndex(characters,targetId))
        {
            Debug.LogWarning("targetIdが範囲外です");
            return;
        }
        BulletType item = bulletManager.Fire();
        int damagePoint = CalculateDamagePoint();
        DebugTools.MakeShotLog(currentActionCharacterId,targetId, item);
        switch (item)
        {
            case BulletType.Live:
                await characters[targetId].GetComponent<Character>().TakeDamage(currentActionCharacterId, damagePoint, false);
                break;
            case BulletType.Empty:
                EmptyOrFakeShot = true;

                CharacterAnimation shooterAnimation = characters[currentActionCharacterId].GetComponent<CharacterAnimation>();
                CharacterAnimation targetAnimation = characters[targetId].GetComponent<CharacterAnimation>();

                if (currentActionCharacterId == targetId)
                {
                    ShotAnimationBase sab = new SelfEmpty();
                    await sab.Play(shooterAnimation);
                }
                else
                {
                    ShotAnimationBase sab = new OtherEmpty();
                    await sab.Play(shooterAnimation, targetAnimation);
                }

                break;
            case BulletType.None:
                Debug.LogWarning("弾丸がNoneです");
                break;
        }
    }
    private void CalculateHateInRound()
    {
        // Hpの最大値と最小値を取得
        int maxHp = characters.Where(c => c.GetComponent<Character>().characterCurrentStatus.isAlive == true)
                              .Max(c => c.GetComponent<Character>().characterCurrentStatus.Hp);
  
        int minHp = characters.Where(c => c.GetComponent<Character>().characterCurrentStatus.isAlive == true)
                              .Min(c => c.GetComponent<Character>().characterCurrentStatus.Hp);

        // 最大Hpのキャラを全員列挙
        List<GameObject> maxHpCharacters = characters
            .Where(c => c.GetComponent<Character>().characterCurrentStatus.Hp == maxHp)
            .ToList();

        // 最小Hpのキャラを全員列挙
        List<GameObject> minHpCharacters = characters
            .Where(c => c.GetComponent<Character>().characterCurrentStatus.Hp == minHp)
            .ToList();

        foreach (GameObject c in maxHpCharacters)
        {
            c.GetComponent<CharacterHate>().OnHpFirstPlaceInRound();
        }
        foreach (GameObject c in minHpCharacters)
        {
            c.GetComponent<CharacterHate>().OnHpLastPlaceInRound();
        }
    }

    private int CalculateDamagePoint()
    {
        return isValidMuzzle ? 2 : 1;
    }

    public TurnInfo GetTurnInfo(int numberToAddToRound)
    {
        return new TurnInfo()
        {
            round = this.round + numberToAddToRound,
            currentActionCharacterId = this.currentActionCharacterId
        };
    }
}