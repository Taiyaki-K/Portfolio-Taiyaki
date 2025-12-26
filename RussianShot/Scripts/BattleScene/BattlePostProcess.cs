using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using KanKikuchi.AudioManager;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class BattlePostProcess : MonoBehaviour
{
    public GameObject gameOverScreen;
    public GameObject victoryScreen;
    public Image fadeImage;
    public GameObject skillReleaseText;
    public TextMeshProUGUI upLv;
    public TextMeshProUGUI earnedExp;

    //勝利アニメ
    public GameObject mainCamra;
    public GameObject endFadeImgWh;
    public GameObject endFadeImgBk;
    public GameObject ThankYouFroPlaying;
    public Light directionLight;
    public Button MenuButton;
    public void DisplayGameOverScreen()
    {
        BGMManager.Instance.Stop();
        SEManager.Instance.Play(SEPath.LOSE);
        SEManager.Instance.FadeOut(SEPath.LOSE, 7f, () => {
            Debug.Log("BGMフェードアウト終了");
        });
        gameOverScreen.SetActive(true);
    }

    public void JumpToMenuScene()
    {
        SEManager.Instance.Play(SEPath.DECIDE);
        fadeImage.DOFade(1f, 1f)
    .OnComplete(() => SceneManager.LoadScene("MenuScene"));
    }
    public void JumpToBattleScene()
    {
        SEManager.Instance.Play(SEPath.DECIDE);
        fadeImage.DOFade(1f, 1f)
            .OnComplete(() => SceneManager.LoadScene("BattleScene"));
    }
    public void DisplayVictoryScreen()
    {
        BGMManager.Instance.Stop();
        SEManager.Instance.Play(SEPath.WIN);
        SEManager.Instance.FadeOut(SEPath.WIN, 7f, () => {
            Debug.Log("BGMフェードアウト終了");
        });
        victoryScreen.SetActive(true);
    }
    public void ProcessVictory()
    {
        int backLv = GameManager.Instance.lv;
        GameManager.Instance.CumulativeExp += GameManager.Instance.earnedExpInBattle;
        int nowLv = GameManager.Instance.lv;
        if(IsBetweenSpecialLevels(backLv, nowLv))
        {
            skillReleaseText.SetActive(true);
        }
        upLv.text = backLv.ToString()+ "→<b><color=green>" + nowLv.ToString()+ "</color></b>";
        earnedExp.text = GameManager.Instance.earnedExpInBattle.ToString();

        if (GameManager.Instance.clearedStageNum < GameManager.Instance.currentStageNum)
        { GameManager.Instance.clearedStageNum = GameManager.Instance.currentStageNum; }
        SaveDataManager.Instance.SaveGame();
    }




    public static bool IsBetweenSpecialLevels(int backLv, int nowLv)
    {
        // 同じレベルなら false
        if (backLv == nowLv) return false;

        int[] specialLevels = { 4, 8, 12, 16, 20, 25, 30 };

        foreach (int k in specialLevels)
        {
            int diffBack = backLv - k;
            int diffNow = nowLv - k;

            // 片方負、片方正 → true
            if ((diffBack < 0 && diffNow > 0) || (diffBack > 0 && diffNow < 0))
            {
                return true;
            }

            // 0と負 → true
            if ((diffBack == 0 && diffNow < 0) || (diffNow == 0 && diffBack < 0))
            {
                return true;
            }

            // 0と正 → false （何もしない）
        }

        return false;
    }

    public async UniTask PlayVictoryAnimation()
    {
        MenuButton.interactable = false;
        GameObject playerObj = BattleSceneManager.Instance.currentBattleManager.characters[0];
        playerObj.transform.LookAt(new Vector3(-2.77f, 0, -8.29f));
        playerObj.GetComponent<Animator>().Play("Walk");
        playerObj.transform.DOMove(new Vector3(-2.77f, 0, -8.29f), 3f).SetEase(Ease.Linear);
        mainCamra.transform.DORotate(new Vector3(37.88f, -114.58f, 0f), 3f);
        await UniTask.WaitForSeconds(3f);
        mainCamra.transform.DORotate(new Vector3(-10f, -114.58f, 0f), 3f);
        playerObj.transform.LookAt(new Vector3(-6.806f, 0, -8.29f));
        playerObj.transform.DOMove(new Vector3(-6.806f, 3.82f, -8.29f), 5f).SetEase(Ease.Unset);
        await UniTask.WaitForSeconds(3f);
        MenuButton.interactable = true;
    }

    public async void PlayEndingAnimation()
    {
        MenuButton.interactable = false;
        BGMManager.Instance.FadeOut(BGMPath.BATTLE_BGM, 3f, () => {
            Debug.Log("BGMフェードアウト終了");
        });
        GameObject playerObj = BattleSceneManager.Instance.currentBattleManager.characters[0];
        playerObj.transform.LookAt(new Vector3(-2.77f, 0, -8.29f));
        playerObj.GetComponent<Animator>().Play("Walk");
        playerObj.transform.DOMove(new Vector3(-2.77f, 0, -8.29f), 3f).SetEase(Ease.Linear);
        mainCamra.transform.DORotate(new Vector3(37.88f, -114.58f, 0f), 3f);
        await UniTask.WaitForSeconds(3f);
        mainCamra.transform.DORotate(new Vector3(-10f, -114.58f, 0f), 3f);
        playerObj.transform.LookAt(new Vector3(-6.806f, 0, -8.29f));
        playerObj.transform.DOMove(new Vector3(-11.083f, 7.64f, -8.29f), 5f).SetEase(Ease.Unset);
        await UniTask.WaitForSeconds(3f);

        DOTween.To
        (
            () => directionLight.intensity,          // getter
            x => directionLight.intensity = x,       // setter
            10000f,                                    // 目標値
            10f                                       // 所要時間
        );
        endFadeImgWh.GetComponent<Image>().DOFade(1f, 3f);
        await UniTask.WaitForSeconds(4f);
        ThankYouFroPlaying.SetActive(true);
        endFadeImgBk.GetComponent<Image>().DOFade(1f, 6f);
        ProcessVictory();
        await UniTask.WaitForSeconds(10f);
        fadeImage.DOFade(1f, 3f)
    .OnComplete(() => SceneManager.LoadScene("MenuScene"));
    }
}
