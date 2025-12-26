using UnityEngine;
using System.Collections.Generic; // List を使うために必要
using UnityEngine.SceneManagement;
using KanKikuchi.AudioManager;
using Taiyaki;

public class TitleManager : MonoBehaviour
{
    // インスペクターで設定する画面のリスト
    // 0: BaseScreen, 1: HowToPlayScreen1, ... 4: ModeChoiceScreen の順で設定
    [SerializeField]
    private List<GameObject> screens = new List<GameObject>();
    [SerializeField]
    private GameObject blinkText;

    // 現在表示している画面のインデックス番号
    private int _currentScreenIndex = 0;

    private void Start()
    {
        // 起動時は、最初の画面 (screens[0]) だけをアクティブにし、残りを非表示にする
        ShowScreenAtIndex(0);
        SEManager.Instance.ChangeBaseVolume(0.5f);
    }

    private void Update()
    {
        // 左クリックが「押された瞬間」を検知
        if (Input.GetMouseButtonDown(0))
        {
            // 1. 現在が最後の画面 (ModeChoiceScreen) かどうかをチェック
            if (_currentScreenIndex >= screens.Count - 1)
            {
                // 最後の画面なら、何もせずに処理を終了
                return;
            }

            // 2. 次の画面のインデックス番号を計算
            _currentScreenIndex++;

            // 3. 次の画面を表示する
            ShowScreenAtIndex(_currentScreenIndex);

            if(_currentScreenIndex == 1)
            {
                SEManager.Instance.Play(SEPath.T_FIRST_CLICK);
            }else
            {
                SEManager.Instance.Play(SEPath.T_RULE);
            }
        }
    }

    /// <summary>
    /// 指定されたインデックスの画面だけをアクティブにし、他をすべて非アクティブにする
    /// </summary>
    private void ShowScreenAtIndex(int index)
    {
        if (index == 0)
        {
            blinkText.SetActive(true);
        }
        else
        {
            blinkText.SetActive(false);
        }
        for (int i = 0; i < screens.Count; i++)
        {
            if (screens[i] == null) continue; // 念のためのNullチェック
            // i が index と一致するかどうか (true / false)
            bool isActive = (i == index);

            screens[i].SetActive(isActive);
        }

        // （現在のインデックスを更新）
        _currentScreenIndex = index;
    }

    public void Quit()
    {
        SceneManager.LoadScene("Title");
        SEManager.Instance.Play(SEPath.T_QUIT);
    }

    public void ChoiceExtremeHard()
    {
        GameManager.Instance.dragonHp = 300;
        GameManager.Instance.attackDuration = 2;
        GameManager.Instance.attackInterval = 0;
        GameManager.Instance.modeName = "<color=#EA00FF>ExtremeHard</color>";
        GameManager.Instance.GoToMainGame();
    }
    public void ChoiceHard()
    {
        GameManager.Instance.dragonHp = 300;
        GameManager.Instance.attackDuration = 2;
        GameManager.Instance.attackInterval = 2;
        GameManager.Instance.modeName = "<color=#FF0000>Hard</color>";
        GameManager.Instance.GoToMainGame();
    }
    public void ChoiceNormal()
    {
        GameManager.Instance.dragonHp = 200;
        GameManager.Instance.attackDuration = 2;
        GameManager.Instance.attackInterval = 6;
        GameManager.Instance.modeName = "<color=white>Normal</color>";
        GameManager.Instance.GoToMainGame();
    }
    public void ChoiceEasy()
    {
        GameManager.Instance.dragonHp = 150;
        GameManager.Instance.attackDuration = 2;
        GameManager.Instance.attackInterval = 10;
        GameManager.Instance.modeName = "<color=#001EFF>Easy</color>";
        GameManager.Instance.GoToMainGame();
    }
}