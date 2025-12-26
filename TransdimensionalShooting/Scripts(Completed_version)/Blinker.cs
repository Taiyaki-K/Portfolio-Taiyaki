using UnityEngine;
using Cysharp.Threading.Tasks; // UniTask
using System;                   // TimeSpan
using System.Threading;         // CancellationToken
using TMPro;                    // ★ TextMeshPro を使うために必要

public class Blinker : MonoBehaviour
{
    [Tooltip("点滅の間隔（秒）。0.5なら 0.5秒表示、0.5秒非表示")]
    [SerializeField]
    private float blinkInterval = 0.5f;

    // ★ 修正: Renderer -> TextMeshPro
    private TextMeshProUGUI _textComponent;

    private CancellationTokenSource _cts;

    private void Awake()
    {
        // ★ 修正: GetComponentInChildren<TextMeshPro>()
        // (もし UI の TextMeshProUGUI を使う場合は <TextMeshProUGUI> に変更)
        _textComponent = GetComponentInChildren<TextMeshProUGUI>();

        _cts = new CancellationTokenSource();
    }

    private void Start()
    {
        if (_textComponent != null)
        {
            BlinkLoop(_cts.Token).Forget();
        }
        else
        {
            Debug.LogWarning("点滅させる TextMeshPro コンポーネントが見つかりません。", this);
        }
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    /// <summary>
    /// オブジェクトが破棄されるまで、テキストのON/OFFを繰り返す
    /// </summary>
    private async UniTask BlinkLoop(CancellationToken token)
    {
        try
        {
            while (true)
            {
                // ★ 修正: _textComponent.enabled を切り替える
                _textComponent.enabled = !_textComponent.enabled;

                // B. blinkInterval（例: 0.5秒）だけ実時間で待機
                await UniTask.Delay(TimeSpan.FromSeconds(blinkInterval), ignoreTimeScale: true, cancellationToken: token);
            }
        }
        catch (OperationCanceledException)
        {
            // (正常なキャンセル)
        }
    }
}