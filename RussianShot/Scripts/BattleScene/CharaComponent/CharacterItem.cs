using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterItem : MonoBehaviour
{
    [SerializeField] GameObject spray;
    [SerializeField] GameObject muzzle;
    [SerializeField] GameObject glass;
    [SerializeField] GameObject rug;

    [SerializeField] GameObject liveBullet;
    [SerializeField] GameObject emptyBullet;

    private GameObject shotGun;

    [System.NonSerialized] public List<GameObject> items = new List<GameObject>();
    [System.NonSerialized] public bool[] slotUsed = new bool[5];
    private Dictionary<GameObject, int> itemObjToSlotNum = new Dictionary<GameObject, int>();
    private Dictionary<Item, GameObject> itemToPrefab = new Dictionary<Item, GameObject>();
    private Dictionary<Item, Vector3> itemToPos = new Dictionary<Item, Vector3>();
    private Dictionary<Item, ActionType> itemToActionType = new Dictionary<Item, ActionType>();
    private Vector3[] rugPositions = new Vector3[4]
    {
        new Vector3(0.23f,2.1f,-2.8f),
        new Vector3(-2.8f,2.1f,-0.23f),
        new Vector3(-0.23f,2.1f,2.8f),
        new Vector3(2.8f,2.1f,0.23f),
    };
    public bool haveSpray;

    public void Initialize(int charaId)//タイミング的にほかのキャラの変数が必要になる操作はできない、BattleManager.Instanceの準備も整ってない
    {
        GameObject temp = Instantiate(rug, rugPositions[charaId],Quaternion.Euler(0,90*(1+charaId),0));
        rug = temp;
        itemToActionType.Add(Item.Spray, ActionType.Spray);
        itemToActionType.Add(Item.Muzzle,ActionType.Muzzle);
        itemToActionType.Add(Item.Glass,ActionType.Glass);

        itemToPrefab.Add(Item.Spray, spray);
        itemToPrefab.Add(Item.Muzzle, muzzle);
        itemToPrefab.Add(Item.Glass, glass);

        itemToPos.Add(Item.Spray, new Vector3(-0.29f, 0.06f,0));
        itemToPos.Add(Item.Muzzle, new Vector3(0.352f, 0.088f, 0));
        itemToPos.Add(Item.Glass, new Vector3(0.245f, 0.085f, 0));

        shotGun = shotGun = GameObject.Find("ShotGun_E");
    }


    public void AddItems(int addItemCount)
    {
        for (int i = 0; i < addItemCount; i++)
        {
            // 空きスロットを探す
            int slotIndex = -1;
            for (int j = 0; j < slotUsed.Length; j++)
            {
                if (!slotUsed[j])
                {
                    slotIndex = j;
                    break;
                }
            }
            if (slotIndex == -1) break; // 空きがなければ終了

            Item randomItem;
            do { randomItem = (Item)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Item)).Length); } while (randomItem == Item.None || (haveSpray && randomItem == Item.Spray));
            if(randomItem == Item.Spray)
            { haveSpray = true; }

            Vector3 position = new Vector3(itemToPos[randomItem].x, itemToPos[randomItem].y, 0.7f - 0.5f * slotIndex);
            GameObject obj = Instantiate(itemToPrefab[randomItem], rug.transform);
            obj.transform.localPosition = position;
            obj.transform.localEulerAngles = new Vector3(0, -90, 0);
            obj.GetComponent<Outline>().enabled = false;

            items.Add(obj);
            slotUsed[slotIndex] = true;
            itemObjToSlotNum.Add(obj, slotIndex);

            if (gameObject.GetComponent<Character>().characterCurrentStatus.characterId == 0)
            {
                BattleSceneManager.Instance.battleUIManager.allSelectedObjects.Add(obj);
                BattleSceneManager.Instance.battleUIManager.MapItemAction(obj, itemToActionType[randomItem],randomItem);
                BattleSceneManager.Instance.battleUIManager.itemObjToItem.Add(obj, randomItem);
            }
            else
            {
                gameObject.GetComponent<NPCRunTurn>().MapItemAction(obj, itemToActionType[randomItem],randomItem);
            }
        }
    }

    public void RemoveItem(GameObject itemObj)
    {
        items.Remove(itemObj);
        slotUsed[itemObjToSlotNum[itemObj]] = false;
        itemObjToSlotNum.Remove(itemObj);
    }

    public async UniTask Spray()
    {
        gameObject.GetComponent<Character>().characterCurrentStatus.Hp++;
        await gameObject.GetComponent<CharacterAnimation>().PlayHealingEffectAsync();
    }
    public async UniTask Muzzle()
    {
        GameObject obj = Instantiate(muzzle, shotGun.transform);
        obj.GetComponent<Outline>().enabled = false;
        obj.transform.localPosition = new Vector3(0, 0.0288f, 0.477f);
        obj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        BattleSceneManager.Instance.currentBattleManager.shotGunMuzzle = obj;
        BattleSceneManager.Instance.currentBattleManager.isValidMuzzle = true;
        SEManager.Instance.Play(SEPath.MUZZLE);
        await UniTask.WaitForSeconds(0.5f);

    }
    public async UniTask Glass()
    {
        if (gameObject.GetComponent<Character>().characterCurrentStatus.characterId == 0)
        { 
            GameObject obj = null;
            BulletType bullet = BattleSceneManager.Instance.currentBattleManager.bulletManager.PeekNextBullet();
            SEManager.Instance.Play(SEPath.GLASS);
            await BattleSceneManager.Instance.volumeController.FadeInVignette(1f, 2f);
            switch (bullet)
            {
                case BulletType.Live:
                    obj = SpawnObjectInCenter(liveBullet, 4f);
                    break;
                case BulletType.Empty:
                    obj = SpawnObjectInCenter(emptyBullet, 4f);
                    break;
                case BulletType.None:
                    Debug.LogWarning("弾丸がNoneです");
                    return;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            Destroy(obj);
            await BattleSceneManager.Instance.volumeController.FadeOutVignette(1f);
            SEManager.Instance.Stop(SEPath.GLASS);
        }
        else
        {
            SEManager.Instance.Play(SEPath.GLASS);
            await BattleSceneManager.Instance.volumeController.FadeInVignette(1f, 1f);
            await BattleSceneManager.Instance.volumeController.FadeOutVignette(1f);
            SEManager.Instance.Stop(SEPath.GLASS);
        }
    }

    GameObject SpawnObjectInCenter(GameObject prefab, float distanceFromCamera)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("MainCameraが見つかりません");
            return null;
        }

        // カメラの位置＋前方向ベクトル×距離で配置場所を計算
        Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera;

        Quaternion spawnRotation = Quaternion.LookRotation(-mainCamera.transform.forward)
                                 * Quaternion.Euler(-90f, 0f, 0f);

        GameObject obj = Instantiate(prefab, spawnPosition, spawnRotation);
        return obj;
    }
}
