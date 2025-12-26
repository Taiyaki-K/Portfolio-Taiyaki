using UnityEngine;

[System.Serializable]
public class CharacterCurrentStatus
{
    public int characterId;
    public int maxHp;
    private int hp;
    public bool isAlive => Hp > 0;
    public int Hp 
    {
        get { return hp; }
        set 
        {
            if (value <= 0)
            {
                BattleSceneManager.Instance.charaIndicatorsManager.HateTextDeath(characterId);
                hp = 0;
                if (characterId == 0)
                {
                    BattleSceneManager.Instance.battlePostProcess.DisplayGameOverScreen();
                }
            }
            else
            {
                if (BattleSceneManager.Instance.endInitialize)
                {
                    CharacterHate ch = BattleSceneManager.Instance.currentBattleManager.characters[characterId].GetComponent<CharacterHate>();
                    if (hp > value) { ch.OnDamaged(); }
                    if (hp < value) { ch.OnHealing(); }
                }
                hp = value;
            }
            if(!(BattleSceneManager.Instance.charaIndicatorsManager.charaIndicatorsParent.transform.childCount == 0))
            BattleSceneManager.Instance.charaIndicatorsManager.AdjustHpBoxes(characterId, hp);
        }
    }

    //ƒXƒLƒ‹ŠÖ˜A
    public bool isShieldActive;
    public bool isCounterActive;
    public bool isLiveBulletTransferActive;
    public bool isCheckmateActive;
}