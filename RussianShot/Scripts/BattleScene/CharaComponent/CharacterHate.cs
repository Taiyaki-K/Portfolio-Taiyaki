using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterHate : MonoBehaviour
{
    private int hate;
    private int stealthLv;

    public int Hate
    {
        get => hate;
        set
        {
            hate = Mathf.Clamp(value, 0, 40);
            if (!(BattleSceneManager.Instance.charaIndicatorsManager.charaIndicatorsParent.transform.childCount == 0))
                BattleSceneManager.Instance.charaIndicatorsManager.AdjustHateText(gameObject.GetComponent<Character>().characterCurrentStatus.characterId, hate);
        }
    }

    public void Initialize()
    {
        int s = gameObject.GetComponent<Character>().characterCapacity.Stealth;
        if(s <=3 )
        {
            stealthLv = 0;
        } 
        else if (s <= 7)
        {
            stealthLv= 1;
        } 
        else if (s <= 10)
        {
            stealthLv = 2;
        }
        Hate = 20 - (2 * s);
    }

    public void OnHealing()
    {
        Hate = Hate + (5 - stealthLv);
    }
    public void OnDamaged()
    {
        Hate = Hate - (3 + stealthLv);
    }

    public void OnHpFirstPlaceInRound()
    {
        Hate = Hate + (2 - stealthLv);
    }
    public void OnHpLastPlaceInRound()
    {
        Hate = Hate - (2 + stealthLv);
    }

    public void OnShotOther()
    {
        Hate = Hate + (5 - stealthLv);
    }
    public void OnShotSelf()
    {
        Hate = Hate - (3 + stealthLv);
    }
}
