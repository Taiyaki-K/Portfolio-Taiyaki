using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour
{
    public Button menuButton;
    public GameObject shotGun;
    public GameObject liveBulletPrefab;
    public GameObject emptyBulletPrefab;

    public GameObject checkBulletButton;
    private UniTaskCompletionSource tcs;

    private GameObject[] characters;

    private Vector3 shotGunPos = new Vector3(0.486f, 2.18f, -0.369f);
    Quaternion shotGunRotation = Quaternion.Euler(0f, -90f, -90f);
    public void Initialize()
    { 
        characters = BattleSceneManager.Instance.currentBattleManager.characters;
    }

    public void OncheckBulletButton()
    {
        if(tcs == null) 
        {
            return;
        }
        SEManager.Instance.Play(SEPath.CANCEL);
        tcs.TrySetResult();
        checkBulletButton.SetActive(false);
    }
    //装填アニメーション
    public async UniTask PlayBulletLoadingAnimation(int liveCount, int emptyCount)
    {
        int totalCount = liveCount + emptyCount;
        float spacing = 0.5f;
        Vector3 centerPos = new Vector3(0f, 2.637f, 0f);
        float startX = centerPos.x - (spacing * (totalCount - 1) / 2f);
        Quaternion rotation = Quaternion.Euler(-90f, 0f, 0f); // x軸90度回転
        List<GameObject> bulletsList = new List<GameObject>();
        for (int i = 0; i < liveCount; i++)
        {
            Vector3 pos = new Vector3(startX + i * spacing, centerPos.y, centerPos.z);
            GameObject bullet = GameObject.Instantiate(liveBulletPrefab, pos, rotation);
            bulletsList.Add(bullet);
            SEManager.Instance.Play(SEPath.PUT_BULLET);
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        }
        for (int i = 0; i < emptyCount; i++)
        {
            Vector3 pos = new Vector3(startX + (liveCount + i) * spacing, centerPos.y, centerPos.z);
            GameObject bullet = GameObject.Instantiate(emptyBulletPrefab, pos, rotation);
            bulletsList.Add(bullet);
            SEManager.Instance.Play(SEPath.PUT_BULLET);
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        }
        // 配置完了後待つ
        tcs = new UniTaskCompletionSource();
        checkBulletButton.SetActive(true);
        menuButton.interactable = true;
        await tcs.Task;
        menuButton.interactable = false;
        // 左から順に 0.2 秒間隔で消す
        foreach (var bullet in bulletsList)
        {
            GameObject.Destroy(bullet);
            SEManager.Instance.Play(SEPath.BULLET_LOAD);
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        }
    }

    public async UniTask PlayBulletAnalysisAnimation(int liveCount, int emptyCount)
    {
        await BattleSceneManager.Instance.volumeController.FadeInVignette(0.5f, 3f);
        int totalCount = liveCount + emptyCount;
        float spacing = 0.5f;
        Vector3 centerPos = new Vector3(0f, 2.637f, -1f);
        float startX = centerPos.x - (spacing * (totalCount - 1) / 2f);
        Quaternion rotation = Quaternion.Euler(-90f, 0f, 0f); // x軸90度回転
        List<GameObject> bulletsList = new List<GameObject>();
        for (int i = 0; i < liveCount; i++)
        {
            Vector3 pos = new Vector3(startX + i * spacing, centerPos.y, centerPos.z);
            GameObject bullet = GameObject.Instantiate(liveBulletPrefab, pos, rotation);
            bulletsList.Add(bullet);
        }
        for (int i = 0; i < emptyCount; i++)
        {
            Vector3 pos = new Vector3(startX + (liveCount + i) * spacing, centerPos.y, centerPos.z);
            GameObject bullet = GameObject.Instantiate(emptyBulletPrefab, pos, rotation);
            bulletsList.Add(bullet);
        }
        // 配置完了後に 2 秒待つ
        await UniTask.Delay(TimeSpan.FromSeconds(3f));
        foreach (var bullet in bulletsList)
        {
            GameObject.Destroy(bullet);
        }
        await BattleSceneManager.Instance.volumeController.FadeOutVignette(2f);
    }
    //チェックメイトが発動しなかったときに使用
    public async UniTask PlayDeadAnimation(int id)
    {
        CharacterAnimation characterAnimation = characters[id].GetComponent<CharacterAnimation>();

        characterAnimation.gameObject.GetComponent<Animator>().applyRootMotion = true;
        await characterAnimation.PlayDyingBackwardsAsync();

        characterAnimation.gameObject.GetComponent<Animator>().applyRootMotion = false;
        characterAnimation.PlayDeadAsync();
        characterAnimation.transform.Translate(0, 1.968065f, 0);
        characterAnimation.transform.Rotate(5.93f, 0, 0);
    }

    //キャラモーション
    public async UniTask PlayEmptyBulletAnimation(int shooterId, int targetId)
    {
        CharacterAnimation shooterAnimation = characters[shooterId].GetComponent<CharacterAnimation>();
        CharacterAnimation targetAnimation = characters[targetId].GetComponent<CharacterAnimation>();

        UniTask shotOtherTask;

        if (shooterId == targetId)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            //後に追加
            return;
        }
        //基本動作（銃打つ、かまえる）
        shotOtherTask = shooterAnimation.PlayShotOtherAsync(targetId);
        targetAnimation.LookCharacter(shooterId);
        targetAnimation.PlayReadyIdleAsync();

        await shotOtherTask;

        //後処理
        shooterAnimation.LookCenter();
        shooterAnimation.PlayIdleAsync();
        targetAnimation.LookCenter();
        targetAnimation.PlayIdleAsync();
        ProcessShotGun();
    }

    public async UniTask PlayLiveBulletAnimationFromShooter(int shooterId, int targetId, ShotAnimationType shotAnimationType)
    {
        CharacterAnimation shooterAnimation = characters[shooterId].GetComponent<CharacterAnimation>();
        CharacterAnimation targetAnimation = characters[targetId].GetComponent<CharacterAnimation>();

        UniTask shotOtherTask;


        if (shooterId == targetId)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            //後に追加
            return;
        }

        //基本動作（銃打つ、かまえる）
        shotOtherTask = shooterAnimation.PlayShotOtherAsync(targetId);
        targetAnimation.LookCharacter(shooterId);
        targetAnimation.PlayReadyIdleAsync();

        switch (shotAnimationType)
        {
            case ShotAnimationType.NotDeath:
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1.833f));//銃打つ瞬間
                    UniTask task = targetAnimation.PlayHitReactionAsync();
                    await UniTask.WhenAll(shotOtherTask, task);
                    break;
                }
            case ShotAnimationType.Death:
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1.833f));//銃打つ瞬間
                    targetAnimation.gameObject.GetComponent<Animator>().applyRootMotion = true;
                    UniTask task = targetAnimation.PlayDyingBackwardsAsync();
                    await UniTask.WhenAll(shotOtherTask, task);
                    break;
                }
            case ShotAnimationType.Shield:
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1.833f));
                    UniTask task = targetAnimation.PlayShieldEffectAsync();
                    await UniTask.WhenAll(shotOtherTask, task);
                    break;
                }
            case ShotAnimationType.Counter:
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1.833f));
                    UniTask task = targetAnimation.PlayCounterEffectAsync();
                    await UniTask.WhenAll(shotOtherTask, task);
                    break;
                }
            case ShotAnimationType.LiveBulletTransfer:
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1.833f));
                    UniTask task = targetAnimation.PlayLiveBulletTransferEffectAsync();
                    await UniTask.WhenAll(shotOtherTask, task);
                    break;
                }
            case ShotAnimationType.Checkmate:
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1.833f));
                    shooterAnimation.gameObject.GetComponent<Animator>().applyRootMotion = true;
                    UniTask task1 = shooterAnimation.PlayCheckmateRecipientEffectAsync();
                    UniTask task2 = targetAnimation.PlayCheckmateUserEffectAsync();
                    UniTask task3 = shooterAnimation.PlayDyingBackwardsAsync();
                    await UniTask.WhenAll(task3);
                    break;
                }
        }
        //後処理
        if (shotAnimationType == ShotAnimationType.Death)
        {
            shooterAnimation.LookCenter();
            shooterAnimation.PlayIdleAsync();
            targetAnimation.gameObject.GetComponent<Animator>().applyRootMotion = false;
            targetAnimation.PlayDeadAsync();
            targetAnimation.transform.Translate(0, 1.968065f, 0);
            targetAnimation.transform.Rotate(5.93f, 0, 0);
        }
        else if (shotAnimationType == ShotAnimationType.Checkmate)
        {
            targetAnimation.LookCenter();
            targetAnimation.PlayIdleAsync();
            shooterAnimation.gameObject.GetComponent<Animator>().applyRootMotion = false;
            shooterAnimation.PlayDeadAsync();
            shooterAnimation.transform.Translate(0, 1.968065f, 0);
            shooterAnimation.transform.Rotate(5.93f, 0, 0);
        }
        else
        {
            shooterAnimation.LookCenter();
            shooterAnimation.PlayIdleAsync();
            targetAnimation.LookCenter();
            targetAnimation.PlayIdleAsync();
        }
        ProcessShotGun();
    }
    public async UniTask PlayLiveBulletAnimationFromSkill(int shooterId, int targetId, ShotAnimationType shotAnimationType)
    {
        CharacterAnimation shooterAnimation = characters[shooterId].GetComponent<CharacterAnimation>();
        CharacterAnimation targetAnimation = characters[targetId].GetComponent<CharacterAnimation>();

        if (shooterId == targetId)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            //後に追加
            return;
        }

        targetAnimation.LookCharacter(shooterId);

        switch (shotAnimationType)
        {
            case ShotAnimationType.NotDeath:
                {
                    await targetAnimation.PlayHitReactionAsync();
                    break;
                }
            case ShotAnimationType.Death:
                {
                    targetAnimation.gameObject.GetComponent<Animator>().applyRootMotion = true;
                    await targetAnimation.PlayDyingBackwardsAsync();
                    break;
                }
            case ShotAnimationType.Shield:
                {
                    await targetAnimation.PlayShieldEffectAsync();
                    break;
                }
            case ShotAnimationType.Counter:
                {
                    await targetAnimation.PlayCounterEffectAsync();
                    break;
                }
            case ShotAnimationType.LiveBulletTransfer:
                {
                    await targetAnimation.PlayLiveBulletTransferEffectAsync();
                    break;
                }
            case ShotAnimationType.Checkmate:
                {
                    shooterAnimation.gameObject.GetComponent<Animator>().applyRootMotion = true;
                    UniTask task1 = shooterAnimation.PlayCheckmateRecipientEffectAsync();
                    UniTask task2 = targetAnimation.PlayCheckmateUserEffectAsync();
                    UniTask task3 = shooterAnimation.PlayDyingBackwardsAsync();
                    await UniTask.WhenAll(task3);
                    break;
                }
        }
        //後処理
        if (shotAnimationType == ShotAnimationType.Death)
        {
            shooterAnimation.LookCenter();
            shooterAnimation.PlayIdleAsync();
            targetAnimation.gameObject.GetComponent<Animator>().applyRootMotion = false;
            targetAnimation.PlayDeadAsync();
            targetAnimation.transform.Translate(0, 1.968065f, 0);
            targetAnimation.transform.Rotate(5.93f, 0, 0);
        }
        else if (shotAnimationType == ShotAnimationType.Checkmate)
        {
            targetAnimation.LookCenter();
            targetAnimation.PlayIdleAsync();
            shooterAnimation.gameObject.GetComponent<Animator>().applyRootMotion = false;
            shooterAnimation.PlayDeadAsync();
            shooterAnimation.transform.Translate(0, 1.968065f, 0);
            shooterAnimation.transform.Rotate(5.93f, 0, 0);
        }
        else
        {
            shooterAnimation.LookCenter();
            shooterAnimation.PlayIdleAsync();
            targetAnimation.LookCenter();
            targetAnimation.PlayIdleAsync();
        }
        ProcessShotGun();
    }
    //LiveBulletTransferAnimation
    public async UniTask PlayLiveBulletAnimationFromShooter(int shooterId, int targetId,int redirectId, ShotAnimationType shotAnimationType)
    {
        UniTask shotOtherTask;
        CharacterAnimation shooterAnimation = characters[shooterId].GetComponent<CharacterAnimation>();
        CharacterAnimation targetAnimation = characters[targetId].GetComponent<CharacterAnimation>();
        CharacterAnimation redirectAnimation = characters[redirectId].GetComponent<CharacterAnimation>();

        if (shooterId == targetId)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            //後に追加
            return;
        }

        shotOtherTask = shooterAnimation.PlayShotOtherAsync(targetId);
        targetAnimation.LookCharacter(shooterId);
        targetAnimation.PlayReadyIdleAsync();
        await UniTask.Delay(TimeSpan.FromSeconds(1.833f));

        targetAnimation.PlayLiveBulletTransferEffectAsync();
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        redirectAnimation.PlayLiveBulletTransferEffectAsync();

        shooterAnimation.LookCenter();
        shooterAnimation.PlayIdleAsync();
        targetAnimation.LookCenter();
        targetAnimation.PlayIdleAsync();
        ProcessShotGun();
    }

    public async UniTask PlayLiveBulletAnimationFromSkill(int shooterId, int targetId, int redirectId, ShotAnimationType shotAnimationType)
    {
        UniTask shotOtherTask;
        CharacterAnimation shooterAnimation = characters[shooterId].GetComponent<CharacterAnimation>();
        CharacterAnimation targetAnimation = characters[targetId].GetComponent<CharacterAnimation>();
        CharacterAnimation redirectAnimation = characters[redirectId].GetComponent<CharacterAnimation>();

        if (shooterId == targetId)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            //後に追加
            return;
        }

        targetAnimation.LookCharacter(shooterId);

        targetAnimation.PlayLiveBulletTransferEffectAsync();
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        redirectAnimation.PlayLiveBulletTransferEffectAsync();

        shooterAnimation.LookCenter();
        shooterAnimation.PlayIdleAsync();
        targetAnimation.LookCenter();
        targetAnimation.PlayIdleAsync();
        ProcessShotGun();
    }

    public void ProcessShotGun()
    {
        shotGun.transform.SetParent(null);
        shotGun.transform.rotation = shotGunRotation;
        shotGun.transform.position = shotGunPos;
    }
}