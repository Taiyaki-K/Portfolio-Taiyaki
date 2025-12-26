using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
public class CharacterSkill : MonoBehaviour
{
    //DeadlineTurnInfo
    private TurnInfo? bulletAnalysisDeadline;
    private TurnInfo? skillSensingDeadline;
    private TurnInfo? shieldDeadline;
    private TurnInfo? counterDeadline;
    private TurnInfo? liveBulletTransferDeadline;
    private TurnInfo? checkmateDeadline;
    //実弾転移関連
    public int? liveBulletTransferProtectedId;
    public int? liveBulletTransferRecipientId;
    //その他
    private CharacterCurrentStatus currentStatus;
    private BattleManager currentBattleManager;
    //プロパティ
    private ObservableDictionary<SkillType, SkillState> skillUsedDictionary;
    public ObservableDictionary<SkillType, SkillState> SkillUsedDictionary
    {
        get { return skillUsedDictionary; }
        set 
        {
            skillUsedDictionary = value;
        }
    }

    public void Initialize(BattleManager _currentBattleManager)//タイミング的にほかのキャラの変数が必要になる操作はできない、BattleManager.Instanceの準備も整ってない
    {
        //キャッシュ
        currentStatus = gameObject.GetComponent<Character>().characterCurrentStatus;
        currentBattleManager = _currentBattleManager;
        //Dictionary初期化
        SkillUsedDictionary = new ObservableDictionary<SkillType, SkillState>();
        foreach (SkillType skill in gameObject.GetComponent<Character>().characterCapacity.skills) // skills は List<SkillType>
        {
            SkillUsedDictionary.Add(skill, SkillState.NotUsed);
        }
        if (currentStatus.characterId == 0)
        {
            skillUsedDictionary.OnChanged += () =>
            {
                BattleSceneManager.Instance.battleUIManager.UpdateSkillMenu();
            };
        }
        else
        {
            skillUsedDictionary.OnChanged += () =>
            {
                var skillDict = BattleSceneManager.Instance.currentBattleManager.characters[0].GetComponent<CharacterSkill>().SkillUsedDictionary;
                if (skillDict.ContainsKey(SkillType.SkillSensing) &&
                    skillDict[SkillType.SkillSensing] == SkillState.InUse)
                {
                    BattleSceneManager.Instance.charaIndicatorsManager.UpdateNPCSkillState(currentStatus.characterId);
                }
            };
        }
    }

    public async UniTask FakeShot(int targetId)
    {
        if (!DebugTools.IsValidIndex(currentBattleManager.characters,targetId))
        {
            Debug.LogWarning("targetIdが範囲外です");
            return;
        }
        CharacterAnimation shooterAnimation = currentBattleManager.characters[currentBattleManager.currentActionCharacterId].GetComponent<CharacterAnimation>();
        CharacterAnimation targetAnimation = currentBattleManager.characters[targetId].GetComponent<CharacterAnimation>();

        if (currentBattleManager.currentActionCharacterId == targetId)
        {
            ShotAnimationBase sab = new SelfEmpty();
            await sab.Play(shooterAnimation);
        }
        else
        {
            ShotAnimationBase sab = new OtherEmpty();
            await sab.Play(shooterAnimation, targetAnimation);
        }

        SkillUsedDictionary[SkillType.FakeShot] = SkillState.Used;
        Debug.Log("以下のログはFakeShotによるものです");
        DebugTools.MakeShotLog(currentBattleManager.currentActionCharacterId,targetId, BulletType.Empty);
    }

    public async Task BulletAnalysis()
    {
        //SkillUsedDictionary[SkillType.BulletAnalysis] = SkillState.Used;
        //BulletManager bm = BattleSceneManager.Instance.currentBattleManager.bulletManager;
        //int liveCount = bm.bullets.Count(b => b == BulletType.Live);
        //int emptyCount = bm.bullets.Count(b => b == BulletType.Empty);
        //await BattleSceneManager.Instance.animationManager.PlayBulletAnalysisAnimation(liveCount, emptyCount);

        SkillUsedDictionary[SkillType.BulletAnalysis] = SkillState.InUse;
        BattleSceneManager.Instance.battleUIManager.DisplayBulletAnalysisField();
        BattleSceneManager.Instance.battleUIManager.AdjustBulletAnalysisField();
        bulletAnalysisDeadline = currentBattleManager.GetTurnInfo(5);

    }

    public void SkillSensing()
    {
        SkillUsedDictionary[SkillType.SkillSensing] = SkillState.InUse;
        BattleSceneManager.Instance.charaIndicatorsManager.SetUpSkillSensingField();

        skillSensingDeadline = currentBattleManager.GetTurnInfo(3);
    }


    //ここからのクラスは、CharacterのTakeDamageとこのクラスのCheckSkill関数で成る
    public void Shield()
    {
        SkillUsedDictionary[SkillType.Shield] = SkillState.InUse;
        currentStatus.isShieldActive = true;
        shieldDeadline = currentBattleManager.GetTurnInfo(2);
    }

    public void Counter()
    {
        SkillUsedDictionary[SkillType.Counter] = SkillState.InUse;
        currentStatus.isCounterActive = true;
        counterDeadline = currentBattleManager.GetTurnInfo(1);
    }

    public void LiveBulletTransfer(int _defenderId, int _redirectTargetId)
    {
        SkillUsedDictionary[SkillType.LiveBulletTransfer] = SkillState.InUse;
        currentStatus.isLiveBulletTransferActive = true;
        liveBulletTransferDeadline = currentBattleManager.GetTurnInfo(1);
        liveBulletTransferProtectedId = _defenderId;
        liveBulletTransferRecipientId = _redirectTargetId;
    }

    public void Checkmate()
    {
        SkillUsedDictionary[SkillType.Checkmate] = SkillState.InUse;
        currentStatus.isCheckmateActive = true;
        checkmateDeadline = currentBattleManager.GetTurnInfo(1);
    }

    public async UniTask CheckSkillActive()
    {
        TurnInfo currentTurnInfo = currentBattleManager.GetTurnInfo(0);
        if (bulletAnalysisDeadline != null && currentTurnInfo == bulletAnalysisDeadline)
        {
            ProcessBulletAnalysis();
        }
        if (skillSensingDeadline != null && currentTurnInfo == skillSensingDeadline)
        {
            ProcessSkillSensing();
        }
        if (shieldDeadline != null && currentTurnInfo == shieldDeadline)
        {
            ProcessShield();
        }
        if (counterDeadline != null && currentTurnInfo == counterDeadline)
        {
            ProcessCounter();
        }
        if (liveBulletTransferDeadline != null && currentTurnInfo == liveBulletTransferDeadline)
        {
            ProcessLiveBulletTransfer();
        }
        if(checkmateDeadline != null && currentTurnInfo == checkmateDeadline)
        {
            ProcessCheckmate();
            await BattleSceneManager.Instance.animationManager.PlayDeadAnimation(currentStatus.characterId);
            currentStatus.Hp = 0;
        }
    }
    public void ProcessBulletAnalysis()
    {
        if (currentStatus.characterId == 0)
        {SEManager.Instance.Play(SEPath.SKILL_EXPIRED);}
        bulletAnalysisDeadline = null;
        SkillUsedDictionary[SkillType.BulletAnalysis] = SkillState.Used;
        BattleSceneManager.Instance.battleUIManager.RemoveBulletAnalysisField();
    }
    public void ProcessSkillSensing()
    {
        if (currentStatus.characterId == 0)
        { SEManager.Instance.Play(SEPath.SKILL_EXPIRED); }
        bulletAnalysisDeadline = null;
        SkillUsedDictionary[SkillType.SkillSensing] = SkillState.Used;
        BattleSceneManager.Instance.charaIndicatorsManager.RemoveSkillSensingField();
    }
    public void ProcessShield()
    {
        if (currentStatus.characterId == 0)
        { SEManager.Instance.Play(SEPath.SKILL_EXPIRED); }
        shieldDeadline = null;
        SkillUsedDictionary[SkillType.Shield] = SkillState.Used;
        currentStatus.isShieldActive = false;
    }
    public void ProcessCounter()
    {
        if (currentStatus.characterId == 0)
        { SEManager.Instance.Play(SEPath.SKILL_EXPIRED); }
        counterDeadline = null;
        SkillUsedDictionary[SkillType.Counter] = SkillState.Used;
        currentStatus.isCounterActive = false;
    }
    public void ProcessLiveBulletTransfer()
    {
        if (currentStatus.characterId == 0)
        { SEManager.Instance.Play(SEPath.SKILL_EXPIRED); }
        liveBulletTransferDeadline = null;
        SkillUsedDictionary[SkillType.LiveBulletTransfer] = SkillState.Used;
        currentStatus.isLiveBulletTransferActive = false;
        liveBulletTransferProtectedId = null;
        liveBulletTransferRecipientId = null;
    }
    public void ProcessCheckmate()
    {
        if (currentStatus.characterId == 0)
        { SEManager.Instance.Play(SEPath.SKILL_EXPIRED); }
        checkmateDeadline = null;
        SkillUsedDictionary[SkillType.Checkmate] = SkillState.Used;
        currentStatus.isCheckmateActive = false;
    }
}