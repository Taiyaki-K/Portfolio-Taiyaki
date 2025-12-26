using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using System.Linq;
using KanKikuchi.AudioManager;

public class BattleUIManager : MonoBehaviour
{
    //その他
    [SerializeField] private GameObject shotGun;
    [SerializeField] private GameObject skillMenu;
    [SerializeField] private GameObject skillButtonPrefab;
     public List<GameObject> allSelectedObjects;//銃、アイテム、キャラクター
    public GameObject bulletImage;
    public GameObject shieldImage;
    public Button menuButton;
    //このクラスの処理で保持しておきたい変数
    private GameObject playerObject;
    private CharacterSkill playerSkillManager;
    private Dictionary<GameObject, System.Action> actionMap = new Dictionary<GameObject, System.Action>();
    private Dictionary<SkillType, GameObject> skillButtonDictionary = new Dictionary<SkillType, GameObject>();
    private CharacterChoice playerChoice;
    public List<GameObject> selectableObjects;
    public Dictionary<GameObject,Item> itemObjToItem = new Dictionary<GameObject,Item>();
    public GameObject cancelButton;
    //BulletAnalysis
    public GameObject bulletAnalysisField;
    public TextMeshProUGUI liveNumText;
    public TextMeshProUGUI empNumText;
    //tcs
    private UniTaskCompletionSource tcsSetFirstMenu;

    public void Initialize()
    {
        playerObject = BattleSceneManager.Instance.currentBattleManager.characters[0];
        playerSkillManager = playerObject.GetComponent<CharacterSkill>();
        allSelectedObjects = new List<GameObject>();
        allSelectedObjects.Add(shotGun);
        allSelectedObjects.AddRange(BattleSceneManager.Instance.currentBattleManager.characters);
        //actionMap
        actionMap[shotGun] = () =>
        {
            SEManager.Instance.Play(SEPath.SELET_GUN);
            playerObject.GetComponent<CharacterAnimation>().HaveShotGun();
            playerChoice.actionType = ActionType.Shot;
            SetSelectingTarget();
        };
        foreach (GameObject chara in BattleSceneManager.Instance.currentBattleManager.characters)
        {
            actionMap[chara] = async () =>
            {
                switch (playerChoice.actionType)
                {
                    case ActionType.Shot:
                        SEManager.Instance.Play(SEPath.DECIDE);
                        playerChoice.targetId = chara.GetComponent<Character>().characterCurrentStatus.characterId;
                        tcsSetFirstMenu.TrySetResult();
                        break;

                    //スキル
                    case ActionType.FakeShot:
                        SEManager.Instance.Play(SEPath.SKILL_SELECT);
                        playerChoice.targetId = chara.GetComponent<Character>().characterCurrentStatus.characterId;
                        tcsSetFirstMenu.TrySetResult();
                        break;

                    case ActionType.LiveBulletTransfer:
                        if (playerChoice.protectedId == null)
                        {
                            SEManager.Instance.Play(SEPath.DECIDE);
                            shieldImage.SetActive(false);
                            ResetAllMenu();
                            await UniTask.WaitForSeconds(0.5f);
                            bulletImage.SetActive(true);
                            playerChoice.protectedId = chara.GetComponent<Character>().characterCurrentStatus.characterId;
                            SetSelectingTarget(chara);
                        }else
                        {
                            SEManager.Instance.Play(SEPath.SKILL_SELECT);
                            bulletImage.SetActive(false);
                            playerChoice.attackedId = chara.GetComponent<Character>().characterCurrentStatus.characterId;
                            tcsSetFirstMenu.TrySetResult();
                        }
                        break;

                    //エラー
                    default:
                        Debug.LogWarning($"ActionTypeが処理できません：{playerChoice.actionType}");
                        break;
                }
            };
        }
        SetupSkillMenu();
        ResetAllMenu();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (selectableObjects == null) return;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 1f);

            if (Physics.Raycast(ray, out var hit))
            {
                GameObject clickedObject = hit.collider.gameObject;
                //selectableObjectsに含まれるオブジェを子から親にかけて探す.なかったらnull.
                GameObject target = FindSelectableParent(clickedObject);
                if (target != null)
                {
                    if (actionMap.TryGetValue(target, out Action value))
                        value.Invoke();
                    else
                        Debug.LogWarning($"{target}はselectableObjectsに含まれていますが、actionMapに定義されていません");
                }
            }
        }
    }

    GameObject FindSelectableParent(GameObject obj)
    {
        while (obj != null)
        {
            if (selectableObjects.Contains(obj))
                return obj;
            obj = obj.transform.parent?.gameObject;
        }
        return null;
    }

    public async UniTask<CharacterChoice> RunPlayerTurnAsync()
    {
        while (true)
        {
            BattleSceneManager.Instance.animationManager.ProcessShotGun();
            playerChoice = new CharacterChoice();
            menuButton.interactable = true;
            await SetFirstMenu();
            menuButton.interactable = false;
            ResetAllMenu();
            if (playerChoice.actionType == ActionType.Cancel)
            {
                bulletImage.SetActive(false);
                shieldImage.SetActive(false);
                continue;
            }
            if (playerChoice.actionType == ActionType.Shot || playerChoice.actionType == ActionType.FakeShot) break;
            await ProcessPlayerChoice();
        }
        return playerChoice;
    }

    async UniTask SetFirstMenu()
    {
        tcsSetFirstMenu = new UniTaskCompletionSource();
        selectableObjects = new List<GameObject>();
        selectableObjects.Add(shotGun);
        selectableObjects.AddRange(playerObject.GetComponent<CharacterItem>().items);
        foreach(GameObject itemObj in playerObject.GetComponent<CharacterItem>().items)
        {
            //Muzzleつけてるとき、二連続で使えないように
            if(itemObjToItem[itemObj] == Item.Muzzle && BattleSceneManager.Instance.currentBattleManager.isValidMuzzle) 
                selectableObjects.Remove(itemObj); 
            //最大体力で回復できないように
            if (itemObjToItem[itemObj] == Item.Spray && playerObject.GetComponent<Character>().characterCurrentStatus.Hp == playerObject.GetComponent<Character>().characterCurrentStatus.maxHp)
                selectableObjects.Remove(itemObj);
        }
        DisplayOutline();
        EnableSkillMenu();
        await tcsSetFirstMenu.Task;
    }

    public void OnCancelButton()
    {
        SEManager.Instance.Play(SEPath.CANCEL);
        playerChoice.actionType = ActionType.Cancel;
        tcsSetFirstMenu.TrySetResult();
    }
    void SetSelectingTarget()
    {
        ResetAllMenu();
        cancelButton.SetActive(true);
        selectableObjects = new List<GameObject>();
        foreach (GameObject character in BattleSceneManager.Instance.currentBattleManager.characters)
        {
            if(!character.GetComponent<Character>().characterCurrentStatus.isAlive)
                continue;
            selectableObjects.Add(character);
        }
        DisplayOutline();
    }

    //実弾転移で二人目を前と同じキャラを選べないようにするため
    void SetSelectingTarget(GameObject formerSelectedChara)
    {
        ResetAllMenu();
        cancelButton.SetActive(true);
        selectableObjects = new List<GameObject>();
        foreach (GameObject character in BattleSceneManager.Instance.currentBattleManager.characters)
        {
            if (!character.GetComponent<Character>().characterCurrentStatus.isAlive)
                continue;
            selectableObjects.Add(character);
        }
        selectableObjects.Remove(formerSelectedChara);
        DisplayOutline();
    }


    public void ResetAllMenu()
    {
        cancelButton.SetActive(false);
        selectableObjects = null;
        foreach(GameObject item in allSelectedObjects)
        {
            if (item == null) continue; // Destroyされている
            var outline = item.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
        }
        foreach(Transform transform in skillMenu.transform)
        {
            var btn = transform.GetComponent<Button>();
            if (btn != null) btn.interactable = false;
        }
    }

    void SetupSkillMenu()
    {
        playerObject.GetComponent<Character>().characterCapacity.skills = Enum.GetValues(typeof(SkillType))
        .Cast<SkillType>()
        .Where(s => playerObject.GetComponent<Character>().characterCapacity.skills.Contains(s))
        .ToList();

        foreach (SkillType skillType in playerObject.GetComponent<Character>().characterCapacity.skills)
        {
            GameObject skillButton = Instantiate(skillButtonPrefab, skillMenu.transform);
            skillButtonDictionary.Add(skillType, skillButton);
            //後でスキルアイコンを張り替える処理を追加
            switch (skillType)
            {
                case SkillType.FakeShot:
                    skillButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("SkillIcons/FakeShot");
                    skillButton.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
                    skillButton.GetComponent<Button>().onClick.AddListener(() => SEManager.Instance.Play(SEPath.DECIDE));
                    skillButton.GetComponent<Button>().onClick.AddListener(() => playerChoice.actionType = ActionType.FakeShot);
                    skillButton.GetComponent<Button>().onClick.AddListener(() => SetSelectingTarget());
                    break;

                case SkillType.BulletAnalysis:
                    skillButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("SkillIcons/BulletAnalysis");
                    skillButton.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
                    skillButton.GetComponent<Button>().onClick.AddListener(() => SEManager.Instance.Play(SEPath.SKILL_SELECT));
                    skillButton.GetComponent<Button>().onClick.AddListener(() => playerChoice.actionType = ActionType.BulletAnalysis);
                    skillButton.GetComponent<Button>().onClick.AddListener(() => tcsSetFirstMenu.TrySetResult());
                    break;

                case SkillType.SkillSensing:
                    skillButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("SkillIcons/SkillSensing");
                    skillButton.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
                    skillButton.GetComponent<Button>().onClick.AddListener(() => SEManager.Instance.Play(SEPath.SKILL_SELECT));
                    skillButton.GetComponent<Button>().onClick.AddListener(() => playerChoice.actionType = ActionType.SkillSensing);
                    skillButton.GetComponent<Button>().onClick.AddListener(() => tcsSetFirstMenu.TrySetResult());
                    break;

                case SkillType.Shield:
                    skillButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("SkillIcons/Shield");
                    skillButton.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
                    skillButton.GetComponent<Button>().onClick.AddListener(() => SEManager.Instance.Play(SEPath.SKILL_SELECT));
                    skillButton.GetComponent<Button>().onClick.AddListener(() => playerChoice.actionType = ActionType.Shield);
                    skillButton.GetComponent<Button>().onClick.AddListener(() => tcsSetFirstMenu.TrySetResult());
                    break;

                case SkillType.Counter:
                    skillButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("SkillIcons/Counter");
                    skillButton.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
                    skillButton.GetComponent<Button>().onClick.AddListener(() => SEManager.Instance.Play(SEPath.SKILL_SELECT));
                    skillButton.GetComponent<Button>().onClick.AddListener(() => playerChoice.actionType = ActionType.Counter);
                    skillButton.GetComponent<Button>().onClick.AddListener(() => tcsSetFirstMenu.TrySetResult());
                    break;

                case SkillType.LiveBulletTransfer:
                    skillButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("SkillIcons/LiveBulletTransfer");
                    skillButton.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
                    skillButton.GetComponent<Button>().onClick.AddListener(() => SEManager.Instance.Play(SEPath.DECIDE));
                    skillButton.GetComponent<Button>().onClick.AddListener(() => shieldImage.SetActive(true));
                    skillButton.GetComponent<Button>().onClick.AddListener(() => playerChoice.actionType = ActionType.LiveBulletTransfer);
                    skillButton.GetComponent<Button>().onClick.AddListener(() => SetSelectingTarget());
                    break;

                case SkillType.Checkmate:
                    skillButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("SkillIcons/Checkmate");
                    skillButton.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
                    skillButton.GetComponent<Button>().onClick.AddListener(() => SEManager.Instance.Play(SEPath.SKILL_SELECT));
                    skillButton.GetComponent<Button>().onClick.AddListener(() => playerChoice.actionType = ActionType.Checkmate);
                    skillButton.GetComponent<Button>().onClick.AddListener(() => tcsSetFirstMenu.TrySetResult());
                    break;

                default:
                    Debug.LogWarning($"未定義のSkillTypeが指定されました:{skillType}");
                    break;
            }
        }
    }
    public void UpdateSkillMenu()
    {
        Debug.Log("Ok");
        foreach (KeyValuePair<SkillType, GameObject> pair in skillButtonDictionary)
        {
            switch(playerSkillManager.SkillUsedDictionary[pair.Key])
            {
                case SkillState.NotUsed:
                    pair.Value.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                    pair.Value.transform.GetChild(1).GetComponent<Image>().color = Color.white;
                    break;

                case SkillState.InUse:
                    pair.Value.transform.GetChild(0).GetComponent<Image>().color = Color.blue;
                    pair.Value.transform.GetChild(1).GetComponent<Image>().color = Color.blue;
                    break;

                case SkillState.Used:
                    pair.Value.transform.GetChild(0).GetComponent<Image>().color = Color.red;
                    pair.Value.transform.GetChild(1).GetComponent<Image>().color = Color.red;
                    break;
            }
        }
    }

    void EnableSkillMenu()
    {
        foreach (KeyValuePair<SkillType, GameObject> pair in skillButtonDictionary)
        {
            switch (playerSkillManager.SkillUsedDictionary[pair.Key])
            {
                case SkillState.NotUsed:
                    pair.Value.GetComponent<Button>().interactable = true;
                    break;

                case SkillState.InUse:
                    pair.Value.GetComponent<Button>().interactable = false;
                    break;

                case SkillState.Used:
                    pair.Value.GetComponent<Button>().interactable = false;
                    break;
            }
        }
    }

    public void DisplayBulletAnalysisField()
    {
        bulletAnalysisField.SetActive(true);
    }
    public void RemoveBulletAnalysisField()
    {
        bulletAnalysisField.SetActive(false);
    }

    public void AdjustBulletAnalysisField()
    {
        BulletManager bm = BattleSceneManager.Instance.currentBattleManager.bulletManager;
        int liveCount = bm.bullets.Count(b => b == BulletType.Live);
        int emptyCount = bm.bullets.Count(b => b == BulletType.Empty);
        liveNumText.text = "×" + liveCount.ToString();
        empNumText.text = "×" + emptyCount.ToString();
    }

    public void MapItemAction(GameObject obj, ActionType actionType,Item item)
    {
        actionMap[obj] = async () =>
        {
            if(actionType == ActionType.Spray && playerObject.GetComponent<Character>().characterCurrentStatus.Hp == playerObject.GetComponent<Character>().characterCurrentStatus.maxHp) return;
            if(item == Item.Spray)
            { playerObject.GetComponent<CharacterItem>().haveSpray = false; }
            playerChoice.actionType = actionType;
            allSelectedObjects.Remove(obj);
            playerObject.GetComponent<CharacterItem>()?.RemoveItem(obj);
            actionMap.Remove(obj);
            tcsSetFirstMenu.TrySetResult();
            await UniTask.Yield(); // 1フレーム後にDestroy
            Destroy(obj);
        };
    }




    async UniTask ProcessPlayerChoice()
    {
        if(playerChoice.actionType == ActionType.Shot || playerChoice.actionType == ActionType.FakeShot)
        {
            //tcsの処理をしてメインループに返す。
        }
        switch (playerChoice.actionType)
        {
            //アイテム
            case ActionType.Spray:
                await playerObject.GetComponent<CharacterItem>().Spray();
                break;
            case ActionType.Muzzle:
                await playerObject.GetComponent<CharacterItem>().Muzzle();
                break;
            case ActionType.Glass:
                await playerObject.GetComponent<CharacterItem>().Glass();
                break;

            //スキル
            case ActionType.BulletAnalysis:
                await playerSkillManager.BulletAnalysis();
                break;

            case ActionType.SkillSensing:
                playerSkillManager.SkillSensing();
                break;

            case ActionType.Shield:
                playerSkillManager.Shield();
                break;

            case ActionType.Counter:
                playerSkillManager.Counter();
                break;

            case ActionType.LiveBulletTransfer:
                if (playerChoice.protectedId != null && playerChoice.attackedId != null)
                    playerSkillManager.LiveBulletTransfer(playerChoice.protectedId.Value, playerChoice.attackedId.Value);
                else { Debug.LogWarning("実弾転移の処理でnullが発生しています"); }
                    break;

            case ActionType.Checkmate:
                playerSkillManager.Checkmate();
                break;

            default:
                Debug.LogWarning($"ProcessPlayerChoiceで処理できません。ActionType：{playerChoice.actionType}");
                break;
        }
    }

    void DisplayOutline()
    {
        foreach (GameObject item in selectableObjects)
        {
            item.GetComponent<Outline>().enabled = true;  // 輪郭の表示・非表示切り替え
        }
    }
}

