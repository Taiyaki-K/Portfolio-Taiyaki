using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cysharp.Threading.Tasks;

public class VolumeController : MonoBehaviour
{
    private Vignette vignette;

    void Start()
    {
        if (gameObject.GetComponent<Volume>().profile.TryGet<Vignette>(out var v))
        {
            vignette = v;
        }
        else
        {
            Debug.LogWarning("vignetteが取得できませんでした");
        }
    }

    public async UniTask FadeInVignette(float targetIntensity, float duration)
    {
        if (vignette == null) return;

        float startIntensity = vignette.intensity.value;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            vignette.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, elapsed / duration);
            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }
        vignette.intensity.value = targetIntensity;
    }

    // 逆にフェードアウトさせたい場合
    public async UniTask FadeOutVignette(float duration)
    {
        if (vignette == null) return;

        float startIntensity = vignette.intensity.value;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            vignette.intensity.value = Mathf.Lerp(startIntensity, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }
        vignette.intensity.value = 0f;
    }
}


    // 白黒にする
    //public void SetBlackAndWhite(bool enable)
    //{
    //    if (colorAdjustments != null)
    //    {
    //        colorAdjustments.saturation.value = enable ? -100f : 0f;
    //    }
    //}

    //public async UniTask HueShiftEffect()
    //{
    //    float elapsed = 0f;
    //    while (elapsed < 5)
    //    {
    //        colorAdjustments.hueShift.value = Mathf.Lerp(0f, 180f, elapsed / 5);
    //        elapsed += Time.deltaTime;
    //        await UniTask.Yield();
    //    }
    //    colorAdjustments.hueShift.value = 0f;
    //}