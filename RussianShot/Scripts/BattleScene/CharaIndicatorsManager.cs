using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharaIndicatorsManager : MonoBehaviour
{
    public GameObject charaIndicatorsParent;
    public GameObject charaIndicatorPrefab;
    private GameObject[] charaIndicators = new GameObject[4];
    //HP用
    public GameObject hpBoxPrefab;
    //Hate用
    private TextMeshProUGUI[] hateTexts = new TextMeshProUGUI[4];
    public TextMeshProUGUI hateSumText;
    //スキル感知用
    public GameObject stateCardsEmpPrefab;//stateCardをまとめたやつ
    public GameObject stateCardPrefab;//実際に色変えるカード
    public GameObject skillSensingField;
    public GameObject[] skillStateCards;
    public Dictionary<SkillType, GameObject>[] skillToStateCardDics;
    public void Initialize()
    {
        for (int i = 0; i < charaIndicators.Length; i++)
        {
            charaIndicators[i] = Instantiate(charaIndicatorPrefab, charaIndicatorsParent.transform);
            charaIndicators[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = BattleSceneManager.Instance.currentBattleManager.characters[i].GetComponent<Character>().characterCapacity.characterName;
            for (int j = 0; j < BattleSceneManager.Instance.currentBattleManager.characters[i].GetComponent<Character>().characterCurrentStatus.maxHp; j++)
            {
                Instantiate(hpBoxPrefab, charaIndicators[i].transform.GetChild(1));
            }
            hateTexts[i] = charaIndicators[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            hateTexts[i].text = BattleSceneManager.Instance.currentBattleManager.characters[i].GetComponent<CharacterHate>().Hate.ToString();
        }
    }

    public void AdjustHpBoxes(int characterId, int hp)
    {
        Transform parentTransform = charaIndicatorsParent.transform.GetChild(characterId).GetChild(1);
        if (hp == 0)//死んでるときは赤
        {
            for (int i = 0; i < parentTransform.childCount; i++)
            {
                parentTransform.GetChild(i).GetComponent<Image>().color = Color.red;
            }
            return;
        }

        for (int i = 0; i < parentTransform.childCount; i++)
        {
            if (i < hp)
            {
                parentTransform.GetChild(i).GetComponent<Image>().color = Color.white;
            }
            else
            {
                parentTransform.GetChild(i).GetComponent<Image>().color = Color.gray;
            }
        }
    }
    public void AdjustHateText(int characterId, int hate)
    {
        hateTexts[characterId].text = hate.ToString();
        AdjustHateSum();
    }
    public void HateTextDeath(int charaId)
    {
        hateTexts[charaId].text = "0";
        hateTexts[charaId].color = Color.red;
        charaIndicators[charaId].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.red;
    }
    public void AdjustHateSum()
    {
        int sum = 0;
        List<int> hates = new List<int>();
        hates = BattleSceneManager.Instance.currentBattleManager.characters
            .Where(obj => obj.GetComponent<Character>().characterCurrentStatus.isAlive)
            .Select(i => i.GetComponent<CharacterHate>().Hate)
            .ToList();
        foreach (int hate in hates)
        {
            sum += hate;
        }
        hateSumText.text = sum.ToString();
    }

    public void SetUpSkillSensingField()
    {
        skillStateCards = new GameObject[4];
        skillToStateCardDics = new Dictionary<SkillType, GameObject>[4];
        for (int i = 0; i < 4; i++)
        {
            skillToStateCardDics[i] = new Dictionary<SkillType, GameObject>();
        }
        for (int i = 1; i < 4; i++)
        {
            GameObject chara = BattleSceneManager.Instance.currentBattleManager.characters[i];
            skillStateCards[i] = Instantiate(stateCardsEmpPrefab, skillSensingField.transform);
            skillStateCards[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = chara.GetComponent<Character>().characterCapacity.characterName;
            foreach (SkillType skillType in chara.GetComponent<Character>().characterCapacity.skills)
            {
                GameObject stateCard = Instantiate(stateCardPrefab, skillStateCards[i].transform);
                skillToStateCardDics[i].Add(skillType, stateCard);
                //後でスキルアイコンを張り替える処理を追加
                switch (skillType)
                {
                    case SkillType.FakeShot:
                        stateCard.GetComponent<Image>().sprite = Resources.Load<Sprite>("SkillIcons/FakeShot");
                        break;

                    case SkillType.BulletAnalysis:
                        stateCard.GetComponent<Image>().sprite = Resources.Load<Sprite>("SkillIcons/BulletAnalysis");
                        break;

                    case SkillType.SkillSensing:
                        stateCard.GetComponent<Image>().sprite = Resources.Load<Sprite>("SkillIcons/SkillSensing");
                        break;

                    case SkillType.Shield:
                        stateCard.GetComponent<Image>().sprite = Resources.Load<Sprite>("SkillIcons/Shield");
                        break;

                    case SkillType.Counter:
                        stateCard.GetComponent<Image>().sprite = Resources.Load<Sprite>("SkillIcons/Counter");
                        break;

                    case SkillType.LiveBulletTransfer:
                        stateCard.GetComponent<Image>().sprite = Resources.Load<Sprite>("SkillIcons/LiveBulletTransfer");
                        break;

                    case SkillType.Checkmate:
                        stateCard.GetComponent<Image>().sprite = Resources.Load<Sprite>("SkillIcons/Checkmate");
                        break;

                    default:
                        Debug.LogWarning($"未定義のSkillTypeが指定されました:{skillType}");
                        break;
                }
                UpdateNPCSkillState(i);
            }
        }
    }
    public void UpdateNPCSkillState(int charaId)
    {
        GameObject chara = BattleSceneManager.Instance.currentBattleManager.characters[charaId];
        foreach (KeyValuePair<SkillType, GameObject> pair in skillToStateCardDics[charaId])
        {
            switch (chara.GetComponent<CharacterSkill>().SkillUsedDictionary[pair.Key])
            {
                case SkillState.NotUsed:
                    pair.Value.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                    pair.Value.transform.GetChild(1).GetComponent<Image>().color = Color.white;
                    break;

                case SkillState.InUse:
                    pair.Value.transform.GetChild(0).GetComponent<Image>().color = Color.blue;
                    pair.Value.transform.GetChild(1).GetComponent<Image>().color = Color.blue ;
                    break;

                case SkillState.Used:
                    pair.Value.transform.GetChild(0).GetComponent<Image>().color = Color.red;
                    pair.Value.transform.GetChild(1).GetComponent<Image>().color = Color.red;
                    break;
            }
        }
    }
    public void RemoveSkillSensingField()
    {
        for (int i = 1; i < 4; i++)
        {
            Destroy(skillStateCards[i].gameObject);
        }
    }
}
