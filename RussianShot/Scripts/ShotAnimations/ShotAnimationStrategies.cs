using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using KanKikuchi.AudioManager;

public abstract class ShotAnimationBase
{
    public virtual UniTask Play(CharacterAnimation selfAnimation)
    {
        Debug.LogWarning("使ってはいけないオーバーロードです");
        return UniTask.CompletedTask;
    }
    public virtual UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation)
    {
        Debug.LogWarning("使ってはいけないオーバーロードです");
        return UniTask.CompletedTask;
    }
    public virtual UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation, CharacterAnimation redirectAnimation)
    {
        Debug.LogWarning("使ってはいけないオーバーロードです");
        return UniTask.CompletedTask;
    }
    //銃打つなら全部
    protected virtual UniTask BeforeShotAsync(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation)
    {
        int shooterId = shooterAnimation.gameObject.GetComponent<Character>().characterCurrentStatus.characterId;
        int targetId = targetAnimation.gameObject.GetComponent<Character>().characterCurrentStatus.characterId;

        UniTask shotOtherTask = shooterAnimation.PlayShotOtherAsync(targetId);
        targetAnimation.LookCharacter(shooterId);
        targetAnimation.PlayReadyIdleAsync();

        return shotOtherTask;
    }
    //死者がない版
    protected virtual void AfterShotAsync(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation)
    {
        shooterAnimation.LookCenter();
        shooterAnimation.PlayIdleAsync();
        targetAnimation.LookCenter();
        targetAnimation.PlayIdleAsync();
        BattleSceneManager.Instance.animationManager.ProcessShotGun(); // 共通の銃リセット処理
    }
    //死者が出る版
    protected virtual void AfterShotAsyncInDeath(CharacterAnimation deadAnimation, CharacterAnimation attackerAnimation)
    {
        attackerAnimation.LookCenter();
        attackerAnimation.PlayIdleAsync();
        deadAnimation.gameObject.GetComponent<Animator>().applyRootMotion = false;
        deadAnimation.PlayDeadAsync();
        deadAnimation.transform.Translate(0, 1.968065f, 0);
        deadAnimation.transform.Rotate(5.93f, 0, 0);
        BattleSceneManager.Instance.animationManager.ProcessShotGun(); // 共通の銃リセット処理
    }
}
//SelfShot
public class SelfEmpty : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation selfAnimation)
    {
        SEManager.Instance.Play
            (
                audioPath: SEPath.SHOT_EMPTY, //再生したいオーディオのパス
                volumeRate: 1,                //音量の倍率
                delay: 1f,                //再生されるまでの遅延時間
                pitch: 1,                //ピッチ
                isLoop: false,             //ループ再生するか
                callback: null              //再生終了後の処理
            );
        await selfAnimation.PlayShotSelfAsync();
        //後
        selfAnimation.LookCenter();
        selfAnimation.PlayIdleAsync();
        BattleSceneManager.Instance.animationManager.ProcessShotGun();
    }
}
public class SelfNotDeath : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation selfAnimation)
    {
        await selfAnimation.PlayShotSelfAsync();
        SEManager.Instance.Play(SEPath.SHOT_LIVE);
        await selfAnimation.PlayHitReactionAsync();
        //後
        selfAnimation.LookCenter();
        selfAnimation.PlayIdleAsync();
        BattleSceneManager.Instance.animationManager.ProcessShotGun();
    }
}
public class SelfDeath : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation selfAnimation)
    {
        await selfAnimation.PlayShotSelfAsync();
        SEManager.Instance.Play(SEPath.SHOT_LIVE);
        selfAnimation.gameObject.GetComponent<Animator>().applyRootMotion = true;
        selfAnimation.SetShotGunRoot();
        await selfAnimation.PlayDyingBackwardsAsync();
        //後
        selfAnimation.gameObject.GetComponent<Animator>().applyRootMotion = false;
        selfAnimation.PlayDeadAsync();
        selfAnimation.transform.Translate(0, 1.968065f, 0);
        selfAnimation.transform.Rotate(5.93f, 0, 0);
        BattleSceneManager.Instance.animationManager.ProcessShotGun();
    }
}
public class SelfShield : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation selfAnimation)
    {
        await selfAnimation.PlayShotSelfAsync();
        SEManager.Instance.Play(SEPath.SHOT_LIVE);
        await selfAnimation.PlayShieldEffectAsync();
        //後
        selfAnimation.LookCenter();
        selfAnimation.PlayIdleAsync();
        BattleSceneManager.Instance.animationManager.ProcessShotGun();
    }
}
public class SelfLiveBulletTransfor : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation selfAnimation,CharacterAnimation redirectAnimation)
    {
        await selfAnimation.PlayShotSelfAsync();
        SEManager.Instance.Play(SEPath.SHOT_LIVE);
        selfAnimation.PlayLiveBulletTransferEffectAsync();
        await UniTask.WaitForSeconds(0.5f);
        redirectAnimation.PlayLiveBulletTransferEffectAsync();
        //後
        selfAnimation.LookCenter();
        selfAnimation.PlayIdleAsync();
        BattleSceneManager.Instance.animationManager.ProcessShotGun();
    }
}

//EmptyOther
public class OtherEmpty : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation)
    {
        //前
        SEManager.Instance.Play
        (
            audioPath: SEPath.SHOT_EMPTY, //再生したいオーディオのパス
            volumeRate: 1,                //音量の倍率
            delay: 2f,                //再生されるまでの遅延時間
            pitch: 1,                //ピッチ
            isLoop: false,             //ループ再生するか
            callback: null              //再生終了後の処理
        );
        await BeforeShotAsync(shooterAnimation, targetAnimation);
        //後
        AfterShotAsync(shooterAnimation, targetAnimation);
    }
}
//FromShooter
public class NotDeathFromShooter : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation)
    {
        //前
        UniTask shotOtherTask = BeforeShotAsync(shooterAnimation, targetAnimation);
        //中核
        await UniTask.Delay(TimeSpan.FromSeconds(1.833f));//銃打つ瞬間
        SEManager.Instance.Play(SEPath.SHOT_LIVE);
        UniTask task = targetAnimation.PlayHitReactionAsync();
        await UniTask.WhenAll(shotOtherTask, task);
        //後
        AfterShotAsync(shooterAnimation, targetAnimation); 
    }
}

public class DeathFromShooter : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation)
    {
        //前
        UniTask shotOtherTask = BeforeShotAsync(shooterAnimation, targetAnimation);
        //中核
        await UniTask.Delay(TimeSpan.FromSeconds(1.833f));//銃打つ瞬間
        SEManager.Instance.Play(SEPath.SHOT_LIVE);
        targetAnimation.gameObject.GetComponent<Animator>().applyRootMotion = true;
        UniTask task = targetAnimation.PlayDyingBackwardsAsync();
        await UniTask.WhenAll(shotOtherTask, task);
        //後
        AfterShotAsyncInDeath(targetAnimation, shooterAnimation);
    }
}

public class ShieldFromShooter : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation)
    {
        //前
        UniTask shotOtherTask = BeforeShotAsync(shooterAnimation, targetAnimation);
        //中核
        await UniTask.Delay(TimeSpan.FromSeconds(1.833f));
        SEManager.Instance.Play(SEPath.SHOT_LIVE);
        UniTask task = targetAnimation.PlayShieldEffectAsync();
        await UniTask.WhenAll(shotOtherTask, task);
        //後
        AfterShotAsync(shooterAnimation, targetAnimation);
    }
}

public class CounterFromShooter : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation)
    {
        //前
        UniTask shotOtherTask = BeforeShotAsync(shooterAnimation, targetAnimation);
        //中核
        await UniTask.Delay(TimeSpan.FromSeconds(1.833f));
        SEManager.Instance.Play(SEPath.SHOT_LIVE);
        UniTask task = targetAnimation.PlayCounterEffectAsync();
        //await UniTask.WhenAll(shotOtherTask, task);
        //後
        AfterShotAsync(shooterAnimation, targetAnimation);
    }
}
public class LiveBulletTransferFromShooter : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation, CharacterAnimation redirectAnimation)
    {
        //前
        UniTask shotOtherTask = BeforeShotAsync(shooterAnimation, targetAnimation);
        //中核
        await UniTask.Delay(TimeSpan.FromSeconds(1.833f));
        SEManager.Instance.Play(SEPath.SHOT_LIVE);
        targetAnimation.PlayLiveBulletTransferEffectAsync();
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        redirectAnimation.PlayLiveBulletTransferEffectAsync();
        //後
        AfterShotAsync(shooterAnimation, targetAnimation);
    }
}
public class CheckmateFromShooter : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation)
    {
        //前
        UniTask shotOtherTask = BeforeShotAsync(shooterAnimation, targetAnimation);
        //中核
        await UniTask.Delay(TimeSpan.FromSeconds(1.833f));
        SEManager.Instance.Play(SEPath.SHOT_LIVE);
        shooterAnimation.gameObject.GetComponent<Animator>().applyRootMotion = true;
        UniTask task1 = shooterAnimation.PlayCheckmateRecipientEffectAsync();
        UniTask task2 = targetAnimation.PlayCheckmateUserEffectAsync();
        UniTask task3 = shooterAnimation.PlayDyingBackwardsAsync();
        await UniTask.WhenAll(task3);
        //後
        AfterShotAsyncInDeath(shooterAnimation,targetAnimation);
    }
}





//FromSkill
public class NotDeathFromSkill : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation)
    {
        //前
        int shooterId = shooterAnimation.gameObject.GetComponent<Character>().characterCurrentStatus.characterId;
        targetAnimation.LookCharacter(shooterId);
        //中核
        await targetAnimation.PlayHitReactionAsync();
        //後
        AfterShotAsync(shooterAnimation, targetAnimation);
    }
}

public class DeathFromSkill : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation)
    {
        //前
        int shooterId = shooterAnimation.gameObject.GetComponent<Character>().characterCurrentStatus.characterId;
        targetAnimation.LookCharacter(shooterId);
        //中核
        targetAnimation.gameObject.GetComponent<Animator>().applyRootMotion = true;
        await targetAnimation.PlayDyingBackwardsAsync();
        //後
        AfterShotAsyncInDeath(targetAnimation, shooterAnimation);
    }
}

public class ShieldFromSkill : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation)
    {
        //前
        int shooterId = shooterAnimation.gameObject.GetComponent<Character>().characterCurrentStatus.characterId;
        targetAnimation.LookCharacter(shooterId);
        //中核
        await targetAnimation.PlayShieldEffectAsync();
        //後
        AfterShotAsync(shooterAnimation, targetAnimation);
    }
}

public class CounterFromSkill : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation)
    {
        //前
        int shooterId = shooterAnimation.gameObject.GetComponent<Character>().characterCurrentStatus.characterId;
        targetAnimation.LookCharacter(shooterId);
        //中核
        targetAnimation.PlayCounterEffectAsync();
        ////後
        AfterShotAsync(shooterAnimation, targetAnimation);
    }
}
public class LiveBulletTransferFromSkill : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation, CharacterAnimation redirectAnimation)
    {
        //前
        int shooterId = shooterAnimation.gameObject.GetComponent<Character>().characterCurrentStatus.characterId;
        targetAnimation.LookCharacter(shooterId);
        //中核
        targetAnimation.PlayLiveBulletTransferEffectAsync();
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        redirectAnimation.PlayLiveBulletTransferEffectAsync();
        //後
        AfterShotAsync(shooterAnimation, targetAnimation);
    }
}
public class CheckmateFromSkill : ShotAnimationBase
{
    public async override UniTask Play(CharacterAnimation shooterAnimation, CharacterAnimation targetAnimation)
    {
        //前
        int shooterId = shooterAnimation.gameObject.GetComponent<Character>().characterCurrentStatus.characterId;
        targetAnimation.LookCharacter(shooterId);
        //中核
        shooterAnimation.gameObject.GetComponent<Animator>().applyRootMotion = true;
        UniTask task1 = shooterAnimation.PlayCheckmateRecipientEffectAsync();
        UniTask task2 = targetAnimation.PlayCheckmateUserEffectAsync();
        UniTask task3 = shooterAnimation.PlayDyingBackwardsAsync();
        await UniTask.WhenAll(task3);
        //後
        AfterShotAsyncInDeath(shooterAnimation, targetAnimation);
    }
}