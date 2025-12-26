using DG.Tweening;
using KanKikuchi.AudioManager;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject NPCInfoCardsParent;
    public GameObject NPCInfoCardPrefab;
    public GameObject stageNumText;
    public GameObject earnedExpText;

    //スクリーン
    public GameObject firstMenuScreen;
    public GameObject ruleScreen;

    private int selectedStageNumber;

    public Image fadeImage;

    private void Start()
    {
        selectedStageNumber = GameManager.Instance.currentInfo.StageNum;
        stageNumText.GetComponent<TextMeshProUGUI>().text = "ステージ" + selectedStageNumber.ToString();
        earnedExpText.GetComponent<TextMeshProUGUI>().text = "獲得Exp:" + GameManager.Instance.currentInfo.earnedExp.ToString();
        for (int i = 0; i < GameManager.Instance.currentInfo.NPCCapacities.Length; i++)
        {
            CharacterCapacity cc = GameManager.Instance.currentInfo.NPCCapacities[i];
            GameObject obj = Instantiate(NPCInfoCardPrefab, NPCInfoCardsParent.transform);
            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = cc.characterName;
            TriangleRadar tr = obj.transform.GetChild(1).GetComponent<TriangleRadar>();
            tr.paramA = cc.HpInCap / 10f;
            tr.paramB = cc.Fortune / 10f;
            tr.paramC = cc.Stealth / 10f;
            string[] skillPathArrays = cc.GetSkillSpritePath();
            Transform skillIconParent = obj.transform.GetChild(2);
            for (int j = 0; j < skillPathArrays.Length; j++)
            {
                skillIconParent.GetChild(j).AddComponent<Image>().sprite = Resources.Load<Sprite>(skillPathArrays[j]);
            }
        }
        RemoveAllScreen();
    }

    public void LoadTitle()
    {
        SEManager.Instance.Play(SEPath.DECIDE);
        fadeImage.DOFade(1f, 1f)
.OnComplete(() => SceneManager.LoadScene("MenuScene"));
    }

    public void LoadCurrentBattle()
    {
        SEManager.Instance.Play(SEPath.DECIDE);
        fadeImage.DOFade(1f, 1f)
.OnComplete(() => SceneManager.LoadScene("BattleScene"));
    }

    public void SetScreen(GameObject screen)
    {
        RemoveAllScreen();
        screen.SetActive(true);
    }

    public void RemoveAllScreen()
    {
        firstMenuScreen.SetActive(false);
        ruleScreen.SetActive(false);
    }

    public void PlayDecedeSound()
    {
        SEManager.Instance.Play(SEPath.DECIDE);
    }
    public void PlayCancelSound()
    {
        SEManager.Instance.Play(SEPath.CANCEL);
    }
}
