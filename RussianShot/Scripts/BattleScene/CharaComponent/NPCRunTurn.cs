using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NPCRunTurn : MonoBehaviour
{
    private CharacterItem itemClass;
    private CharacterSkill skillClass;
    private Dictionary<GameObject, System.Action> actionMap = new Dictionary<GameObject, System.Action>();
    private Dictionary<GameObject, Item> itemObjToItem = new Dictionary<GameObject, Item>();
    private Dictionary<SkillType,ActionType> skillToAction = new Dictionary<SkillType, ActionType>()
    { 
        { SkillType.FakeShot, ActionType.FakeShot },
        { SkillType.BulletAnalysis, ActionType.BulletAnalysis },
        { SkillType.SkillSensing, ActionType.SkillSensing },
        { SkillType.Shield, ActionType.Shield },
        { SkillType.Counter, ActionType.Counter },
        { SkillType.LiveBulletTransfer, ActionType.LiveBulletTransfer },
        { SkillType.Checkmate, ActionType.Checkmate }
    };
    private Dictionary<Item, ActionType> itemToActionType = new Dictionary<Item, ActionType>()
    {
        { Item.Spray, ActionType.Spray },
        { Item.Muzzle, ActionType.Muzzle},
        { Item.Glass, ActionType.Glass}
    };
    CharacterChoice choice;
    List<SkillType> usableSkill;
    bool glass;

    public void Initialize()
    {
        itemClass = gameObject.GetComponent<CharacterItem>();
        skillClass = gameObject.GetComponent<CharacterSkill>();
    }

    public async UniTask<CharacterChoice> RunNPCTurnAsync()
    {
        //アイテム
        while (true)
        {
            GameObject spray = IsHavinSpray();
            if (spray != null && gameObject.GetComponent<Character>().characterCurrentStatus.Hp < gameObject.GetComponent<Character>().characterCurrentStatus.maxHp)
            {
                if (actionMap.TryGetValue(spray, out System.Action value))
                {
                    value.Invoke();
                    choice = new CharacterChoice { actionType = itemToActionType[Item.Spray] };
                    await ProcessChoice();
                    continue;
                }
                else
                {
                    Debug.LogWarning($"{spray}はselectableObjectsに含まれていますが、actionMapに定義されていません");
                }
            }
            if(glass && (BattleSceneManager.Instance.currentBattleManager.bulletManager.PeekNextBullet() == BulletType.Empty)) break;
            if( glass && (BattleSceneManager.Instance.currentBattleManager.bulletManager.PeekNextBullet() == BulletType.Live)&& !BattleSceneManager.Instance.currentBattleManager.isValidMuzzle)
            {
                GameObject obj = GetMuzzleOrNull();
                if (obj != null)
                {
                    if (actionMap.TryGetValue(obj, out System.Action value))
                    {
                        value.Invoke();
                        choice = new CharacterChoice { actionType = itemToActionType[Item.Muzzle] };
                        await ProcessChoice();
                        continue;
                    }
                    else
                    {
                        Debug.LogWarning($"{obj}はselectableObjectsに含まれていますが、actionMapに定義されていません");
                    }
                }
            }
            if (itemClass.items.Count != 0 && Random.value < 0.8f)
            {
                Item item = SelectItem();
                if (!(item == Item.None))
                { choice = new CharacterChoice { actionType = itemToActionType[item] }; }
                else
                { break; }
                await ProcessChoice();
                continue;
            }
            break;
        }
        //スキル
        bool turn2 = false;
        while (true)
        {
            if (turn2)
            {
                choice.actionType = ActionType.Shot;
                break;
            }
            if (glass && (BattleSceneManager.Instance.currentBattleManager.bulletManager.PeekNextBullet() == BulletType.Empty))
            {
                choice.actionType = ActionType.Shot;
                break;
            }
            if(glass && (BattleSceneManager.Instance.currentBattleManager.bulletManager.PeekNextBullet() == BulletType.Live))
            {
                choice.actionType = ActionType.Shot;
                break;
            }
            usableSkill = GetAvailableSkillsExceptCheckmate();
            choice = DecideChoice();
            if (choice.actionType == ActionType.Shot || choice.actionType == ActionType.FakeShot) break;
            await ProcessChoice();
            turn2 = true;
        }
        choice.targetId = DecideTargetId();
        return choice;
    }
    private int DecideTargetId()
    {
        BulletManager bm = BattleSceneManager.Instance.currentBattleManager.bulletManager;
        if (glass)
        {
            glass = false;
            if (bm.PeekNextBullet() == BulletType.Live)
            {
                return GetOtherTargetNumber();
            }
            else
            {
                return gameObject.GetComponent<Character>().characterCurrentStatus.characterId;
            }
        }

        if(BattleSceneManager.Instance.currentBattleManager.isValidMuzzle)
        {
            return GetOtherTargetNumber();
        }

        if(gameObject.GetComponent<Character>().characterCurrentStatus.Hp == 1)
        {
            return GetOtherTargetNumber();
        }

        int liveCount = bm.bullets.Count(b => b == BulletType.Live);
        int emptyCount = bm.bullets.Count(b => b == BulletType.Empty);
        if (liveCount < emptyCount)
        {
            if (Random.value < 0.6f)
            {
                return gameObject.GetComponent<Character>().characterCurrentStatus.characterId;
            }
        }

        return GetOtherTargetNumber() ;
    }
    GameObject GetMuzzleOrNull()
    {
        foreach (GameObject obj in itemClass.items)
        {
            if(itemObjToItem[obj] == Item.Muzzle)
                return obj;
        }
        return null;
    }

    private int GetOtherTargetNumber()
    {
        List<Character> characters = BattleSceneManager.Instance.currentBattleManager.characters
                        .Select(obj => obj.GetComponent<Character>())
                        .Where(c => c != gameObject.GetComponent<Character>())
                        .Where(c => c.characterCurrentStatus.isAlive)
                        .ToList();
        Dictionary<Character, float> charaToHate = new Dictionary<Character, float>();
        foreach (Character c in characters)
        { charaToHate.Add(c, c.GetComponent<CharacterHate>().Hate); }

        return Choose(charaToHate).characterCurrentStatus.characterId;
    }

    public T Choose<T>(Dictionary<T, float> weights)
    {
        float total = weights.Values.Sum();
        float rand = Random.value * total;

        if (total <= 0)
        {
            return weights.Keys.ElementAt(Random.Range(0, weights.Count));
        }

        foreach (var kvp in weights)
        {
            rand -= kvp.Value;
            if (rand <= 0)
                return kvp.Key;
        }
        return weights.Keys.First(); // 保険
    }


    private CharacterChoice DecideChoice()
    {
        //スキル
        switch (gameObject.GetComponent<Character>().characterCurrentStatus.Hp)
        {
            case 1:
                //チェックメイト
                if (skillClass.SkillUsedDictionary.TryGetValue(SkillType.Checkmate, out var state) && state == SkillState.NotUsed)
                {
                    if (Random.value < 0.3f)
                    { return new CharacterChoice { actionType = ActionType.Checkmate }; }
                }
                //その他スキル
                if (usableSkill.Any())
                {
                    if (Random.value < 0.9f)
                    {
                        int index = Random.Range(0, usableSkill.Count);
                        ActionType at = skillToAction[usableSkill[index]];
                        { return new CharacterChoice { actionType = at }; }
                    }
                }
                break;
            case 2:
                //その他スキル
                if (usableSkill.Any())
                {
                    if (Random.value < 0.6f)
                    {
                        int index = Random.Range(0, usableSkill.Count);
                        ActionType at = skillToAction[usableSkill[index]];
                        { return new CharacterChoice { actionType = at }; }
                    }
                }
                break;
            case 3:
                //その他スキル
                if (usableSkill.Any())
                {
                    if (Random.value < 0.4f)
                    {
                        int index = Random.Range(0, usableSkill.Count);
                        ActionType at = skillToAction[usableSkill[index]];
                        { return new CharacterChoice { actionType = at }; }
                    }
                }
                break;
            default:
                if (usableSkill.Any())
                {
                    if (Random.value < 0.1f)
                    {
                        int index = Random.Range(0, usableSkill.Count);
                        ActionType at = skillToAction[usableSkill[index]];
                        { return new CharacterChoice { actionType = at }; }
                    }
                }
                break;
        }
        return new CharacterChoice { actionType = ActionType.Shot };
    }
    private Item SelectItem()
    {
        GameObject glassObj = IsHavingMuzzleAndGlass();
        if (glassObj != null)
        {
            if (actionMap.TryGetValue(glassObj, out System.Action action))
            {
                action.Invoke();
                return Item.Glass;
            }
            else
            {
                Debug.LogWarning($"{glassObj}はselectableObjectsに含まれていますが、actionMapに定義されていません");
            }
        }
        int index = Random.Range(0,itemClass.items.Count);
        GameObject itemObj = itemClass.items[index];
        if (itemObjToItem[itemObj] == Item.Spray && gameObject.GetComponent<Character>().characterCurrentStatus.Hp == gameObject.GetComponent<Character>().characterCurrentStatus.maxHp)
        { return Item.None; }
        if (itemObjToItem[itemObj] == Item.Glass && glass)
        { return Item.None; }
        if (itemObjToItem[itemObj] == Item.Muzzle && BattleSceneManager.Instance.currentBattleManager.isValidMuzzle)
            { return Item.None; }
        if (actionMap.TryGetValue(itemClass.items[index], out System.Action value))
        { 
            value.Invoke(); 
            return itemObjToItem[itemObj];
        }
        else
        { 
            Debug.LogWarning($"{itemClass.items[index]}はselectableObjectsに含まれていますが、actionMapに定義されていません"); 
            return Item.None;
        }
    }
    public void MapItemAction(GameObject obj, ActionType actionType, Item item)
    {
        itemObjToItem.Add(obj,item);
        actionMap[obj] = async () =>
        {
            if(item == Item.Spray) 
            { itemClass.haveSpray = false; }
            gameObject.GetComponent<CharacterItem>()?.RemoveItem(obj);
            actionMap.Remove(obj);
            await UniTask.Yield();
            Destroy(obj);
        };
    }

    GameObject IsHavingMuzzleAndGlass()
    {
        if (glass) return null;
        if(BattleSceneManager.Instance.currentBattleManager.isValidMuzzle) return null;
        bool haveMuzzle = false;
        bool haveGlass = false;
        GameObject glassobj = null;
        foreach (GameObject obj in itemClass.items)
        {
            if (itemObjToItem[obj] == Item.Muzzle)
                haveMuzzle = true;
            if (itemObjToItem[obj] == Item.Glass)
            {
                haveGlass = true;
                glassobj = obj;
            }
        }
        if (haveMuzzle && haveGlass)
        { return glassobj; }
        else { return null; }
    }

    GameObject IsHavinSpray()
    {
        foreach (GameObject obj in itemClass.items)
        {
            if (itemObjToItem[obj] == Item.Spray)
            {
                return obj;
            }
        }
        return null;
    }


    async UniTask ProcessChoice()
    {
        if (choice.actionType == ActionType.Shot || choice.actionType == ActionType.FakeShot)
        {
            //ここに来る前に処理
        }
        switch (choice.actionType)
        {
            //アイテム
            case ActionType.Spray:
                await gameObject.GetComponent<CharacterItem>().Spray();
                break;
            case ActionType.Muzzle:
                await gameObject.GetComponent<CharacterItem>().Muzzle();
                break;
            case ActionType.Glass:
                glass = true;
                await gameObject.GetComponent<CharacterItem>().Glass();
                break;

            //スキル
            case ActionType.Shield:
                skillClass.Shield();
                break;
            case ActionType.Counter:
                skillClass.Counter();
                break;
            case ActionType.LiveBulletTransfer:
                int selfId = gameObject.GetComponent<Character>().characterCurrentStatus.characterId;
                int protect;
                protect = selfId;
                int attack;
                do { attack = GetOtherTargetNumber(); } while (attack == protect);
                choice.protectedId = protect;
                choice.attackedId = attack;
                skillClass.LiveBulletTransfer(choice.protectedId.Value, choice.attackedId.Value);
                break;
            case ActionType.Checkmate:
                skillClass.Checkmate();
                break;
            default:
                Debug.LogWarning($"ProcessChoiceで処理できません。ActionType：{choice.actionType}");
                break;
        }
    }

    public List<SkillType> GetAvailableSkillsExceptCheckmate()
    {
        return skillClass.SkillUsedDictionary
            .Where(kvp => kvp.Key != SkillType.Checkmate && kvp.Value == SkillState.NotUsed)
            .Select(kvp => kvp.Key).ToList();
    }
}
