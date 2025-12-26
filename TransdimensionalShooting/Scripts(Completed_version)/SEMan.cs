using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using System;
using System.Threading.Tasks;
using Taiyaki;
using UnityEngine;
using UnityEngine.Rendering;

public class SEMan : MonoBehaviour
{
    private async void OnEnable()
    {
        SEManager.Instance.ChangeBaseVolume(0.5f);
        BGMManager.Instance.Play(BGMPath.JAM_SHOOTING_BGM);
        EventBus.OnDragonDied += DragonDeath;
        EventBus.OnPlayerDied += PlayerDeath;
        EventBus.OnShoot += ShootAud;

        try
        {
            // 2. ★ await に CancellationToken を渡す (安全装置)
            //    もし2フレーム待つ前にオブジェクトが破棄されたら、
            //    CancellationToken が作動し、catch に飛ぶ
            await UniTask.DelayFrame(2, cancellationToken: this.GetCancellationTokenOnDestroy());

            // 3. （オブジェクトが生存していた場合のみ）await 後の処理を実行
            EventBus.OnModeChanged += Kirikae;
        }
        catch (OperationCanceledException)
        {
            // 4. (待機中にオブジェクトが破棄された場合。正常な動作)
            //    エラーログを出さずに、静かに処理を中断する
        }
    }
    private void OnDisable()
    {
        EventBus.OnDragonDied -= DragonDeath;
        EventBus.OnPlayerDied -= PlayerDeath;
        EventBus.OnShoot -= ShootAud;
        EventBus.OnModeChanged -= Kirikae;
    }
    private void PlayerDeath()
    {
        BGMManager.Instance.Stop();
        if (GameModeManager.Instance.Is2DMode)
        {
            SEManager.Instance.Play(SEPath.BIT_PLAYER_DEATH,0.5f);
        }
        else
        {
            SEManager.Instance.Play(SEPath.PLAYER_DEATH);
        }
    }
    private void DragonDeath()
    {
        BGMManager.Instance.Stop();
        if (GameModeManager.Instance.Is2DMode)
        {
            SEManager.Instance.Play(SEPath.BIT_DORAGON_DEATH, 0.5f);
        }
        else
        {
            SEManager.Instance.Play(SEPath.DORAGON_DEATH);
        }
    }
    private void Kirikae(bool is2DMode)
    {
        if (is2DMode)
        {
            SEManager.Instance.Play(SEPath.TO2_D, 0.5f);
        }
        else
        {
            SEManager.Instance.Play(SEPath.TO3_D);
        }
    }
    private void ShootAud(Vector2 _)
    {
        if (GameModeManager.Instance.Is2DMode)
        {
            SEManager.Instance.Play(SEPath.BIT_SHOT1, 0.3f);
        }
        else
        {
            SEManager.Instance.Play(SEPath.SHOT);
        }
    }

    public void KetteiAud()
    {
        SEManager.Instance.Play(SEPath.T_FIRST_CLICK);
    }
    public void YameruAud()
    {
        SEManager.Instance.Play(SEPath.T_QUIT);
    }
}
