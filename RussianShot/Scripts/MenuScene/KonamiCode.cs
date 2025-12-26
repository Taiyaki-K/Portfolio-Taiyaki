using UnityEngine;

public class KonamiCode : MonoBehaviour
{
    public GameObject konamiScreen;
    // コナミコマンドのキー列
    private KeyCode[] konamiSequence = new KeyCode[]
    {
        KeyCode.UpArrow,
        KeyCode.UpArrow,
        KeyCode.DownArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.B,
        KeyCode.A
    };

    private int progress = 0; // どこまで入力できたか

    void Update()
    {
        if (Input.anyKeyDown)
        {
            // 今回押されたキーを確認
            if (Input.GetKeyDown(konamiSequence[progress]))
            {
                progress++;
                Debug.Log(progress);
                // 全部正しく入力された！
                if (progress == konamiSequence.Length)
                {
                    Debug.Log("コナミコマンド成功！");
                    konamiScreen.SetActive(true);
                    progress = 0; // リセットしておく
                }
            }
            else
            {
                // 入力ミス → 進捗をリセット
                progress = 0;
            }
        }
    }

    public void QuitKonamiScreen()
    {
        konamiScreen.SetActive(false);
    }

    public void SetCompleteData()
    {
        SaveDataManager.Instance.SetCompleteSaveData();
    }

    public void ResetSaveData()
    {
        SaveDataManager.Instance.ResetSaveData();
    }
}