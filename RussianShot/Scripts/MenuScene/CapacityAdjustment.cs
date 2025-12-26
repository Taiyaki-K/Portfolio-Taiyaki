using KanKikuchi.AudioManager;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class CapacityAdjustment : MonoBehaviour
{
    public GameObject nextLvUpNum;
    public GameObject nowLv;
    public GameObject lvBar;
    public GameObject radarChart;
    public GameObject[] adjustmentObjs;
    public GameObject[] skillToggles;
    public TextMeshProUGUI capacityPointNumText;
    public GameObject enableSkillText;

    private TriangleRadar radarComp;
    private CharacterCapacity playerCapacity;

    public void PlayIncreaseSound()
    {
        SEManager.Instance.Play(SEPath.INCREASE);
    }
    public void PlayDecreaseSound()
    {
        SEManager.Instance.Play(SEPath.DECREASE);
    }
    public void PlayGetSkillSound()
    {
        SEManager.Instance.Play(SEPath.GET_SKILL);
    }
    private void Start()
    {
        foreach (var toggleObj in skillToggles)
        {
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            toggle.onValueChanged.AddListener((_) => UpdateToggleState());
        }
    }
    private void UpdateToggleState()
    {
        int maxSelect = GetSkillMax();
        int onCount = skillToggles.Count(t => t.GetComponent<Toggle>().isOn);

        // ONがmaxSelect未満なら → 全部触れる
        if (onCount < maxSelect)
        {
            enableSkillText.SetActive(true);
            foreach (var toggleObj in skillToggles)
            {
                toggleObj.GetComponent<Toggle>().interactable = true;
            }
        }
        else
        {
            // maxSelectに達したら → OFFのやつは選べなくする
            enableSkillText.SetActive(false);
            foreach (var toggleObj in skillToggles)
            {
                Toggle toggle = toggleObj.GetComponent<Toggle>();
                if (!toggle.isOn)
                    toggle.interactable = false;
            }
        }
    }

    public int GetSkillMax()
    {
        int lv = GameManager.Instance.lv;
        if (lv < 4) return 0;
        else if (lv < 8) return 1;
        else if (lv < 12) return 2;
        else if (lv < 16) return 3;
        else if (lv < 20) return 4;
        else if (lv < 25) return 5;
        else if (lv < 30) return 6;
        else if (lv < 31) return 7;
        Debug.LogWarning("レベルが30を超えています");
        return 0;
    }

    public void OnCallingScreen()
    {
        playerCapacity = GameManager.Instance.playerCapacity;
        radarComp = radarChart.GetComponent<TriangleRadar>();
        radarComp.paramA = playerCapacity.HpInCap/10f;
        radarComp.paramB = playerCapacity.Fortune/10f;
        radarComp.paramC = playerCapacity.Stealth / 10f;

        AdjustBoxes(Capacity.Hp);
        AdjustBoxes(Capacity.Fortune);
        AdjustBoxes(Capacity.Stealth);

        lvBar.GetComponent<Image>().fillAmount = GameManager.Instance.lv / 30f;

        nextLvUpNum.GetComponent<TextMeshProUGUI>().text = ProcessLvAndExp.GetNeedExpToNextLv(GameManager.Instance.CumulativeExp).ToString() + "exp.";

        nowLv.GetComponent<TextMeshProUGUI>().text = "Lv." + GameManager.Instance.lv.ToString();

        int point = GameManager.Instance.lv - (playerCapacity.HpInCap + playerCapacity.Fortune + playerCapacity.Stealth);
        string text = (point == 0) ? "<b><color=white>" + point.ToString() + "</color></b>" + "pt." : "<b><color=green>" + point.ToString() + "</color></b>" + "pt.";
        capacityPointNumText.text = text;

        DisplaySkillToggles();
        UpdateToggleState(); // 初期化時にも反映
    }

    public void IncreaseCapacity(int capNum)
    {
        Capacity cap = (Capacity)capNum;
        switch (cap)
        {
            case Capacity.Hp:
                playerCapacity.HpInCap++;
                break;
            case Capacity.Fortune:
                playerCapacity.Fortune++;
                break;
            case Capacity.Stealth:
                playerCapacity.Stealth++;
                break;
        }
    }

    public void DecreaseCapacity(int capNum)
    {
        Capacity cap = (Capacity)capNum;
        switch (cap)
        {
            case Capacity.Hp:
                playerCapacity.HpInCap--;
                break;
            case Capacity.Fortune:
                playerCapacity.Fortune--;
                break;
            case Capacity.Stealth:
                playerCapacity.Stealth--;
                break;
        }
    }

    public void AdjustAllBox()
    {
        AdjustBoxes(Capacity.Hp);
        AdjustBoxes(Capacity.Fortune);
        AdjustBoxes(Capacity.Stealth);
        radarComp.paramA = playerCapacity.HpInCap / 10f;
        radarComp.paramB = playerCapacity.Fortune / 10f;
        radarComp.paramC = playerCapacity.Stealth / 10f;
        radarComp.SetAllDirty();

        int point = GameManager.Instance.lv - (playerCapacity.HpInCap + playerCapacity.Fortune + playerCapacity.Stealth);
        string text = (point == 0) ? "<b><color=white>" + point.ToString() + "</color></b>" + "pt." : "<b><color=green>" + point.ToString() + "</color></b>" + "pt.";
        capacityPointNumText.text = text;
    }

    private void DisplaySkillToggles()
    {
        foreach (GameObject obj in skillToggles)
        {
            obj.GetComponent<Toggle>().SetIsOnWithoutNotify(false);
        }
        foreach (SkillType skill in playerCapacity.skills)
        {
            skillToggles[(int)skill].GetComponent<Toggle>().SetIsOnWithoutNotify(true);
        }
    }

    public void OnSkillToggleChanged(int skillTypeNum)
    {
        SkillType skill = (SkillType)skillTypeNum;
        if (skillToggles[skillTypeNum].GetComponent<Toggle>().isOn)
        {
            if (!playerCapacity.skills.Contains(skill))
            { playerCapacity.skills.Add(skill); }
            else
            { Debug.LogWarning("プレイヤーのSkillリストが異常"); }
        }else
        {
            if (playerCapacity.skills.Contains(skill))
            { playerCapacity.skills.Remove(skill); }
            else
            { Debug.LogWarning("プレイヤーのSkillリストが異常"); }
        }
        SaveDataManager.Instance.SaveGame();
    }


    public void AdjustBoxes(Capacity cap)
    {
        GameObject obj = GetAdjObj(cap);
        Transform parentTransform = obj.transform.Find("Boxes");
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            if (i < GetCapPoint(cap))
            {
                parentTransform.GetChild(i).GetComponent<Image>().color = Color.white;
            }
            else
            {
                parentTransform.GetChild(i).GetComponent<Image>().color = Color.gray;
            }
        }
        SaveDataManager.Instance.SaveGame();
    }

    private GameObject GetAdjObj(Capacity cap)
    {
        return cap switch
        {
            Capacity.Hp => adjustmentObjs[0],
            Capacity.Fortune => adjustmentObjs[1],
            Capacity.Stealth => adjustmentObjs[2],
            _ => null,
        };
    }

    private int GetCapPoint(Capacity cap)
    {
        return cap switch
        {
            Capacity.Hp => playerCapacity.HpInCap,
            Capacity.Fortune => playerCapacity.Fortune,
            Capacity.Stealth => playerCapacity.Stealth,
            _ => 0,
        };
    }
}