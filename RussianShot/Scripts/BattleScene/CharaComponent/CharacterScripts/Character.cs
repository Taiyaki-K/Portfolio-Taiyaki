using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using KanKikuchi.AudioManager;

public class Character : MonoBehaviour
{
    [System.NonSerialized] public CharacterCapacity characterCapacity;
    [System.NonSerialized] public CharacterCurrentStatus characterCurrentStatus;

    public void Initialize(CharacterCapacity _characterCapacity, int _characterId)//タイミング的にほかのキャラの変数が必要になる操作はできない、BattleManager.Instanceの準備も整ってない
    {
        characterCapacity = _characterCapacity;
        characterCurrentStatus = new CharacterCurrentStatus
        {
            characterId = _characterId,
        };
        if (characterCapacity.HpInCap == 0)
            characterCurrentStatus.maxHp = 1;
        else if (characterCapacity.HpInCap <= 2)
            characterCurrentStatus.maxHp = 2;
        else if (characterCapacity.HpInCap <= 4)
            characterCurrentStatus.maxHp = 3;
        else if (characterCapacity.HpInCap <= 6)
            characterCurrentStatus.maxHp = 4;
        else if (characterCapacity.HpInCap <= 8)
            characterCurrentStatus.maxHp = 5;
        else if (characterCapacity.HpInCap <= 9)
            characterCurrentStatus.maxHp = 6;
        else if (characterCapacity.HpInCap <= 10)
            characterCurrentStatus.maxHp = 7;

        characterCurrentStatus.Hp = characterCurrentStatus.maxHp;
    }

    public async UniTask<CharacterChoice> RunNPCTurnAsync()
    {  
        return await gameObject.GetComponent<NPCRunTurn>().RunNPCTurnAsync();
    }

    public async UniTask TakeDamage(int shooterId, int damagePoint, bool fromSkill)
    {
        ShotAnimationBase smb;
        int targetId = characterCurrentStatus.characterId;
        CharacterAnimation shooterAnimation = BattleSceneManager.Instance.currentBattleManager.characters[shooterId].GetComponent<CharacterAnimation>();
        CharacterAnimation targetAnimation = gameObject.GetComponent<CharacterAnimation>();
        if (shooterId == targetId)
        {
            if (fromSkill)
            { Debug.LogWarning("fromスキルかつセルフショットが発生しています"); }
            await TakeDamageSelf(shooterId , damagePoint);
            return;
        }
        //チェックメイト
        if (IsValidCheckmate(damagePoint))
        {
            smb = fromSkill ? new CheckmateFromSkill() : new CheckmateFromShooter();
            await smb.Play(shooterAnimation, targetAnimation);
            UseCheckmate(shooterId, targetId);
            return;
        }
        //カウンター
        if (characterCurrentStatus.isCounterActive)
        {
            smb = fromSkill ? new CounterFromSkill() : new CounterFromShooter();
            await smb.Play(shooterAnimation, targetAnimation);
            await UseCounter(shooterId, targetId, damagePoint);
            return;
        }
        //実弾転移
        GameObject liveBulletTransferUser = GetValidLiveBulletTransferUser();
        if (liveBulletTransferUser != null)
        {
            int recipientId;
            if (liveBulletTransferUser.GetComponent<CharacterSkill>().liveBulletTransferRecipientId is int id)
            { recipientId = id; }
            else
            {
                Debug.LogWarning("実弾転移が使用されていますが、RecipientIdが存在していません");
                return;
            }
            CharacterAnimation redirectAnimation = BattleSceneManager.Instance.currentBattleManager.characters[recipientId].GetComponent<CharacterAnimation>();
            smb = fromSkill ? new LiveBulletTransferFromSkill() : new LiveBulletTransferFromShooter();
            await smb.Play(shooterAnimation, targetAnimation, redirectAnimation);
            await UseLiveBulletTransfer(liveBulletTransferUser, targetId, recipientId, damagePoint);
            return;
        }
        //シールド
        if (characterCurrentStatus.isShieldActive)
        {
            smb = fromSkill ? new ShieldFromSkill() : new ShieldFromShooter();
            await smb.Play(shooterAnimation, targetAnimation);
            UseShield();
            return;
        }
        //デフォルト
        if (characterCurrentStatus.Hp - damagePoint <= 0)
        { smb = fromSkill ? new DeathFromSkill() : new DeathFromShooter(); }
        else
        { smb = fromSkill ? new NotDeathFromSkill() : new NotDeathFromShooter(); }
        await smb.Play(shooterAnimation, targetAnimation);
        characterCurrentStatus.Hp -= damagePoint;
    }
    async UniTask TakeDamageSelf(int selfId , int damagePoint)
    {
        //カウンターとチェックメイトは無視
        ShotAnimationBase smb;
        CharacterAnimation selfAnimation = gameObject.GetComponent<CharacterAnimation>();
        //実弾転移
        GameObject liveBulletTransferUser = GetValidLiveBulletTransferUser();
        if (liveBulletTransferUser != null)
        {
            int recipientId;
            if (liveBulletTransferUser.GetComponent<CharacterSkill>().liveBulletTransferRecipientId is int id)
            { recipientId = id; }
            else
            {
                Debug.LogWarning("実弾転移が使用されていますが、RecipientIdが存在していません");
                return;
            }
            CharacterAnimation redirectAnimation = BattleSceneManager.Instance.currentBattleManager.characters[recipientId].GetComponent<CharacterAnimation>();
            smb = new SelfLiveBulletTransfor();
            await smb.Play(selfAnimation, redirectAnimation);
            await UseLiveBulletTransfer(liveBulletTransferUser, selfId, recipientId, damagePoint);
            return;
        }
        //シールド
        if (characterCurrentStatus.isShieldActive)
        {
            smb = new SelfShield();
            await smb.Play(selfAnimation);
            UseShield();
            return;
        }
        if (characterCurrentStatus.Hp - damagePoint <= 0)
        { smb = new SelfDeath(); }
        else
        { smb = new SelfNotDeath(); }
        await smb.Play(selfAnimation);
        characterCurrentStatus.Hp -= damagePoint;
    }


    void UseShield()
    {
        //シールドは使用しても切れない
        Debug.Log($"{characterCurrentStatus.characterId}はシールドでダメージを防いだ");
    }

    async UniTask UseCounter(int shooterId,int targetId,int damagePoint)
    {
        gameObject.GetComponentInChildren<CharacterSkill>().ProcessCounter();

        Debug.Log($"{targetId}は{shooterId}に対してカウンターを成功させた");
        await BattleSceneManager.Instance.currentBattleManager.characters[shooterId].GetComponent<Character>().TakeDamage(targetId, damagePoint*2,true);
    }

    async UniTask UseLiveBulletTransfer(GameObject user, int protectedId, int recipientId, int damagePoint)
    {
        //前処理
        int userId = user.GetComponent<Character>().characterCurrentStatus.characterId;
        user.GetComponent<CharacterSkill>().ProcessLiveBulletTransfer();

        Debug.Log($"{characterCurrentStatus.characterId}は{userId}守られていた！実弾が{recipientId}に転送された！");
        if (!BattleSceneManager.Instance.currentBattleManager.characters[recipientId].GetComponent<Character>().characterCurrentStatus.isAlive)
        {
            Debug.Log($"しかし{recipientId}はすでに死んでいるので弾丸は宙を舞った");
            return;
        }
        await BattleSceneManager.Instance.currentBattleManager.characters[recipientId].GetComponent<Character>().TakeDamage(protectedId,damagePoint,true);
    }

    void UseCheckmate(int shooterId,int targetId)
    {
        gameObject.GetComponentInChildren<CharacterSkill>().ProcessCheckmate();

        Debug.Log($"{targetId}は{shooterId}に対してチェックメイトを成功させた");
        BattleSceneManager.Instance.currentBattleManager.characters[shooterId].GetComponent<Character>().characterCurrentStatus.Hp = 0;
    }

    bool IsValidCheckmate(int damagePoint)
    {
        if (!characterCurrentStatus.isCheckmateActive) return false;
        if (!(characterCurrentStatus.Hp - damagePoint <= 0)) return false;
        return true;
    }

    GameObject GetValidLiveBulletTransferUser()
    {
        foreach (GameObject liveBulletTransferUser in BattleSceneManager.Instance.currentBattleManager.characters)
        {
            if (liveBulletTransferUser.GetComponent<Character>().characterCurrentStatus.isLiveBulletTransferActive &&
                liveBulletTransferUser.GetComponent<CharacterSkill>().liveBulletTransferProtectedId == characterCurrentStatus.characterId)
            {
                return liveBulletTransferUser;
            }
        }
        return null;
    }
}