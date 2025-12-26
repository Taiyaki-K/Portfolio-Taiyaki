using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using KanKikuchi.AudioManager;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScreenManager : MonoBehaviour
{
    //Scripts
    public CapacityAdjustment capacityAdjustment;
    //Screens
    public GameObject titleScreen;
    public GameObject stageSelectionScreen;
    public GameObject capacityAdjustmentScreen;
    public GameObject ruleDescriptionScreen;
    public TextMeshProUGUI capacityAdjustmentButtonText;
    //TitleScreen
    public Button exitButton;
    //BattleSettingScreen
    public GameObject upperStages;
    public GameObject lowerStages;
    public List<GameObject> stageSelectionButtons;
    public StageInfomation[] stageInfomations;
    public GameObject stageButtonPrefab;
    public GameObject NPCInfoCardsParent;
    public GameObject NPCInfoCardPrefab;
    public GameObject stageNumText;
    public GameObject earnedExpText;
    private Color normalColor = Color.white;
    private Color selectedColor = new Color(0.7f, 0.7f, 0.7f);
    private int selectedStageNumber = 0;//1から10
    public Image fadeImage;
    //CapacityAdjustmentScreen


    public GameObject star;

    //Common
    async void Start()
    {
        await UniTask.Yield();
        BGMManager.Instance.Play(
                           audioPath: BGMPath.MENU_BGM, //再生したいオーディオのパス
                           volumeRate: 0.6f,                //音量の倍率
                           delay: 0,                //再生されるまでの遅延時間
                           pitch: 1,                //ピッチ
                           isLoop: true,             //ループ再生するか
                           allowsDuplicate: false             //他のBGMと重複して再生させるか
                         );
        ResetStageDescription();
        selectedStageNumber = 0;
        stageSelectionButtons = new List<GameObject>();
        exitButton.onClick.AddListener(AppTools.ExitGame);
        for (int i = 0; i < 10; i++)
        {
            int index = i;
            GameObject obj;
            if (i<5)
            {
                obj = Instantiate(stageButtonPrefab, upperStages.transform);
            }else
            {
                obj = Instantiate(stageButtonPrefab, lowerStages.transform);
            }
            stageSelectionButtons.Add(obj);
            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (index+1).ToString();
            obj.GetComponent<Button>().onClick.AddListener(() => OnStageButtonClicked(index));
            obj.GetComponent<Button>().onClick.AddListener(() => SEManager.Instance.Play(SEPath.DECIDE));
        }
        //foreach (GameObject obj in stageSelectionButtons)
        //{
        //    obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (Array.IndexOf(stageSelectionButtons, obj) + 1).ToString();
        //    obj.GetComponent<Button>().onClick.AddListener(() => OnStageButtonClicked(obj));
        //}
        if(GameManager.Instance.clearedStageNum == 10)
        {
            star.SetActive(true);
        }
        else
        {
            star.SetActive(false);
        }
        JumpToTitle();
    }
    void ChangeColorCapacityAdjustmentButton()
    {
        CharacterCapacity playerCapacity = GameManager.Instance.playerCapacity;
        bool bool1 = playerCapacity.skills.Count < capacityAdjustment.GetSkillMax();
        bool bool2 = (GameManager.Instance.lv > (playerCapacity.HpInCap + playerCapacity.Fortune + playerCapacity.Stealth));
        if(bool1 || bool2)
            capacityAdjustmentButtonText.color = Color.green;
        else 
            capacityAdjustmentButtonText.color = new Color(50f/255,50f/255,50f/255);
    }
    public void PlayDecedeSound()
    {
        SEManager.Instance.Play(SEPath.DECIDE);
    }
    public void PlayCancelSound()
    {
        SEManager.Instance.Play(SEPath.CANCEL);
    }
    public void PlayBattleStartSound()
    {
        SEManager.Instance.Play(SEPath.BATTLE_START);
    }
    private void EnableButton()
    {
        for (int i = 0;i < 10;i++)
        {
            GameObject obj = stageSelectionButtons[i];
            if(i <= GameManager.Instance.clearedStageNum)
            {
                obj.GetComponent<Image>().color = Color.white;
                obj.GetComponent<Button>().interactable = true;
            }else
            {
                obj.GetComponent<Image>().color = Color.black;
                obj.GetComponent<Button>().interactable = false;
            }
        }
    }
    public void JumpToTitle()
    {
        ChangeColorCapacityAdjustmentButton();
        RemoveAllScreens();
        titleScreen.SetActive(true);
    }
    public void SetScreen(GameObject screen)
    {
        RemoveAllScreens();
        screen.SetActive(true);
        if(screen == capacityAdjustmentScreen)
        { capacityAdjustment.OnCallingScreen(); }
        if (screen == stageSelectionScreen)
        { EnableButton(); }
    }
    void RemoveAllScreens()
    {
        titleScreen.SetActive(false);
        stageSelectionScreen.SetActive(false);
        capacityAdjustmentScreen.SetActive(false);
        ruleDescriptionScreen.SetActive(false);
        ResetStageDescription();
    }

    //BattleSettingScreen
    public void OnStageButtonClicked(int buttonNum)//buttonNumは0から9
    {
        ResetStageDescription();
        foreach (var buttonObj in stageSelectionButtons)
        {
            if (buttonObj == null) continue;
            var image = buttonObj.GetComponent<Image>();
            if (image != null&& buttonObj.GetComponent<Button>().interactable)
            {
                image.color = (buttonNum == stageSelectionButtons.IndexOf(buttonObj)) ? selectedColor : normalColor;
            }
        }
        //selectedStageNumberの処理(1から10)
        selectedStageNumber = buttonNum + 1;
        stageNumText.GetComponent<TextMeshProUGUI>().text = "ステージ" + selectedStageNumber.ToString();
        earnedExpText.GetComponent<TextMeshProUGUI>().text = "獲得Exp:" + stageInfomations[selectedStageNumber-1].earnedExp.ToString();
        for (int i = 0; i < stageInfomations[selectedStageNumber - 1].NPCCapacities.Length; i++)
        {
            CharacterCapacity cc = stageInfomations[selectedStageNumber - 1].NPCCapacities[i];
            GameObject obj = Instantiate(NPCInfoCardPrefab,NPCInfoCardsParent.transform);
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
    }
    private void ResetStageDescription()
    {
        stageNumText.GetComponent<TextMeshProUGUI>().text = "";
        earnedExpText.GetComponent<TextMeshProUGUI>().text = "";
        foreach (Transform child in NPCInfoCardsParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void ConfirmStage()
    {
        if (selectedStageNumber == 0) return;
        List<CharacterCapacity> temp = new List<CharacterCapacity>();
        temp.Add(GameManager.Instance.playerCapacity);
        temp.AddRange(stageInfomations[selectedStageNumber-1].NPCCapacities);
        GameManager.Instance.reservedCharacterCapacities = temp;
        GameManager.Instance.earnedExpInBattle = stageInfomations[selectedStageNumber - 1].earnedExp;
        GameManager.Instance.currentStageNum = selectedStageNumber;
        GameManager.Instance.currentInfo = stageInfomations[selectedStageNumber - 1];
        fadeImage.DOFade(1f, 1f)
            .OnComplete(() => SceneManager.LoadScene("BattleScene"));
    }
}
