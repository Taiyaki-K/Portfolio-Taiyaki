using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class MaterialColorChanger : MonoBehaviour
{
    [SerializeField] private float duration = 2.0f; // 2秒かけて変更
    [SerializeField] private Color targetColor = Color.red;

    private Renderer _renderer;
    private Material _materialInstance; // このオブジェクト専用のマテリアル
    private CancellationTokenSource _cts;

    private void Awake()
    {
        // 1. Renderer を取得
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
        {
            // 2. ★重要★
            // 共有マテリアル(sharedMaterial)を変更すると他の全オブジェクトの色が変わってしまうため、
            // このオブジェクト専用の「マテリアルインスタンス」を取得します。
            _materialInstance = _renderer.material;
        }

        _cts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        // オブジェクト破棄時にタスクをキャンセル
        _cts?.Cancel();
        _cts?.Dispose();
    }

    // (例) ゲームが始まったら色変更を開始
    private void Start()
    {
        // .Forget() で非同期処理を実行しっぱなしにする
        ChangeColorSmoothly(_materialInstance.color, targetColor, duration).Forget();
    }

    /// <summary>
    /// マテリアルの色を duration 秒かけてスムーズに変更する
    /// </summary>
    private async UniTask ChangeColorSmoothly(Color startColor, Color endColor, float transitionDuration)
    {
        if (_materialInstance == null) return;

        float elapsedTime = 0f; // 経過時間

        try
        {
            // 経過時間が duration を超えるまでループ
            while (elapsedTime < transitionDuration)
            {
                // 経過時間を加算
                // (Time.timeScale の影響を受けたいなら Time.deltaTime)
                // (Time.timeScale を無視したいなら UniTask.DeltaTime)
                elapsedTime += Time.deltaTime;

                // 0.0 ～ 1.0 の割合(t)を計算
                // (Clamp01で 1.0 を超えないようにする)
                float t = Mathf.Clamp01(elapsedTime / transitionDuration);

                // 3. Color.Lerp で中間色を計算し、マテリアルに適用
                _materialInstance.color = Color.Lerp(startColor, endColor, t);

                // 4. 次のフレームまで待機
                await UniTask.Yield(PlayerLoopTiming.Update, _cts.Token);
            }

            // ループが終了したら、確実に最終色(targetColor)に設定する
            _materialInstance.color = endColor;
        }
        catch (OperationCanceledException)
        {
            // (オブジェクトが破棄されるなどでタスクがキャンセルされた場合)
        }
    }
}