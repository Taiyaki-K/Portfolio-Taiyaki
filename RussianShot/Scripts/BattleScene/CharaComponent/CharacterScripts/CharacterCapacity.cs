using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/CharacterCapacity")]
public class CharacterCapacity : ScriptableObject
{
    public string characterName;
    public GameObject selfObject;
    [SerializeField] private int hpInCap;
    [SerializeField] private int fortune;
    [SerializeField] private int stealth;
    public List<SkillType> skills;
    //プロパティ
    public int HpInCap
    {
        get => hpInCap;
        set
        {
            int newValue = Mathf.Clamp(value, 0, 10);
            if (newValue + fortune + stealth > GameManager.Instance.lv)
            {
                newValue = GameManager.Instance.lv - (fortune + stealth);
                newValue = Mathf.Clamp(newValue, 0, 10);
            }
            hpInCap = newValue;
        }
    }
    public int Fortune
    {
        get => fortune;
        set
        {
            int newValue = Mathf.Clamp(value, 0, 10);
            if (hpInCap + newValue + stealth > GameManager.Instance.lv)
            {
                newValue = GameManager.Instance.lv - (hpInCap + stealth);
                newValue = Mathf.Clamp(newValue, 0, 10);
            }
            fortune = newValue;
        }
    }
    public int Stealth
    {
        get => stealth;
        set
        {
            int newValue = Mathf.Clamp(value, 0, 10);
            if (hpInCap + fortune + newValue > GameManager.Instance.lv)
            {
                newValue = GameManager.Instance.lv - (hpInCap + fortune);
                newValue = Mathf.Clamp(newValue, 0, 10);
            }
            stealth = newValue;
        }
    }
    //ステージ情報用
    public string[] GetSkillSpritePath()
    {
        string[] s = new string[skills.Count];
        for (int i = 0; i < skills.Count; i++)
        {
            s[i] = skills[i].GetDescription();
        }
        return s;
    }
}