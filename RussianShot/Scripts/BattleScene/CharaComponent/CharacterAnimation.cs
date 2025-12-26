using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.TextCore.Text;
using KanKikuchi.AudioManager;

public class CharacterAnimation : MonoBehaviour
{
    //その他
    private Animator animator;
    private GameObject shotGun;
    private Transform rightHand;
    private Vector3[] effectPositions = new Vector3[4]
    {
        new Vector3(0f,2.5f,-4.5f),
        new Vector3(-4.5f,2.5f,0f),
        new Vector3(0f,2.5f,4.5f),
        new Vector3(4.5f,2.5f,0f),
    };
    private Vector3 effectPosition;
    //Effects
    public GameObject checkmateUserEffect;
    public GameObject checkmateRecipientEffect;
    public GameObject counterEffect;
    public GameObject healingEffect;
    public GameObject liveBulletTransferEffect;
    public GameObject shieldEffect;
    //tcs処理デリゲート
    private Action onShotOtherEnd;
    private Action onHitReactionEnd;
    private Action onDyingBackwardsEnd;

    public void Initialize(int id)//タイミング的にほかのキャラの変数が必要になる操作はできない、BattleManager.Instanceの準備も整ってない
    {
        animator = GetComponent<Animator>();
        rightHand = transform.Find("root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r");
        shotGun = GameObject.Find("ShotGun_E");
        effectPosition = effectPositions[id];
    }
    public void HaveShotGun()
    {
        SEManager.Instance.Play(SEPath.HOLDING_GUN);
        shotGun.transform.SetParent(rightHand);
        shotGun.transform.localPosition = new Vector3(0.0844f, -0.0402f, 0.0477f);
        shotGun.transform.localEulerAngles = new Vector3(22.612f, 34.615f, 37.03f);
    }

    //キャラのモーション
    public async UniTask PlayShotOtherAsync(int targetId)
    {
        Vector3 direction = BattleSceneManager.Instance.currentBattleManager.characters[targetId].transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Euler(0, transform.localEulerAngles.y + 70, 0);
        shotGun.transform.SetParent(rightHand);
        shotGun.transform.localPosition = new Vector3(0.0844f, -0.0402f, 0.0477f);
        shotGun.transform.localEulerAngles = new Vector3(22.612f, 34.615f, 37.03f);
        animator.CrossFade("Shot", 0.1f);
        SEManager.Instance.Play(SEPath.HOLDING_GUN);
        //終了待ち
        UniTaskCompletionSource tcs = new UniTaskCompletionSource();
        onShotOtherEnd = () => { tcs.TrySetResult(); };
        await tcs.Task;
    }
    public async UniTask PlayHitReactionAsync()
    {
        SEManager.Instance.Play(SEPath.BLOOD);
        animator.CrossFade("HitReaction", 0.1f);
        //終了待ち
        UniTaskCompletionSource tcs = new UniTaskCompletionSource();
        onHitReactionEnd = () => { tcs.TrySetResult(); };
        await tcs.Task;
    }
    public async UniTask PlayDyingBackwardsAsync()
    {
        SEManager.Instance.Play
            (
                audioPath: SEPath.DYING, //再生したいオーディオのパス
                volumeRate: 1,                //音量の倍率
                delay: 2.5f,                //再生されるまでの遅延時間
                pitch: 1,                //ピッチ
                isLoop: false,             //ループ再生するか
                callback: null              //再生終了後の処理
            );
        animator.CrossFade("DyingBackwards",0.3f);
        //終了待ち
        UniTaskCompletionSource tcs = new UniTaskCompletionSource();
        onDyingBackwardsEnd = () => { tcs.TrySetResult(); };
        await tcs.Task;
    }

    //これらの関数をアニメーションイベントの最後に呼ぶ
    public void OnShotOtherEnd()
    {
        Action callback = onShotOtherEnd;
        onShotOtherEnd = null;
        callback?.Invoke();
    }
    public void OnHitReactionEnd()
    {
        Action callback = onHitReactionEnd;
        onHitReactionEnd = null;
        callback?.Invoke();
    }
    public void OnDyingBackwardsEnd()
    {
        Action callback = onDyingBackwardsEnd;
        onDyingBackwardsEnd = null;
        callback?.Invoke();
    }
    //終わりのないモーション
    public void PlayIdleAsync()
    {
        animator.CrossFade("HumanF@Idle01", 0.1f);
    }
    public void PlayReadyIdleAsync()
    {
        animator.CrossFade("ReadyIdle", 0.1f);
    }
    public void PlayDeadAsync()
    {
        animator.Play("Dead");
    }
    //エフェクト
    public async UniTask PlayShieldEffectAsync()
    {
        SEManager.Instance.Play(SEPath.SHIELD);
        GameObject obj = Instantiate(shieldEffect,transform);
        //終了待ち
        await UniTask.Delay(TimeSpan.FromSeconds(3f));
        Destroy(obj);
    }
    public async UniTask PlayCounterEffectAsync()
    {
        SEManager.Instance.Play(SEPath.COUNTER);
        Instantiate(counterEffect,effectPosition, Quaternion.identity);
        //終了待ち
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
    }
    public async UniTask PlayLiveBulletTransferEffectAsync()
    {
        SEManager.Instance.Play(SEPath.LIVE_BULLET_TRANSFER);
        Instantiate(liveBulletTransferEffect, effectPosition, Quaternion.identity);
        //終了待ち
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
    }
    public async UniTask PlayCheckmateRecipientEffectAsync()
    {
        SEManager.Instance.Play(SEPath.CHECKMATE);
        Instantiate(checkmateRecipientEffect, effectPosition, Quaternion.identity);
        //終了待ち
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
    }
    public async UniTask PlayCheckmateUserEffectAsync()
    {
        Instantiate(checkmateUserEffect, effectPosition, Quaternion.identity);
        //終了待ち
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
    }
    public async UniTask PlayHealingEffectAsync()
    {
        SEManager.Instance.Play(SEPath.SPRAY);
        GameObject obj = Instantiate(healingEffect, effectPosition, Quaternion.identity);
        //終了待ち
        await UniTask.Delay(TimeSpan.FromSeconds(2f));
        Destroy(obj);
    }

    //自分系モーション(仮)
    public async UniTask PlayShotSelfAsync()
    {
        Vector3 spawnPos = transform.position + transform.forward * 2f;

        shotGun.transform.SetParent(transform);
        shotGun.transform.position = spawnPos;
        shotGun.transform.rotation = Quaternion.LookRotation(transform.forward);
        shotGun.transform.Translate(0, 2.7f, 0);
        shotGun.transform.Rotate(-15, 180, 0);
        
        //終了待ち
        await UniTask.Delay(TimeSpan.FromSeconds(2f));
    }

    public void SetShotGunRoot()
    {
        shotGun.transform.SetParent(null);
    }
    public void LookCenter()
    {
        Vector3 direction = Vector3.zero - transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }
    public void LookCharacter(int characterId)
    {
        Vector3 direction = BattleSceneManager.Instance.currentBattleManager.characters[characterId].transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
