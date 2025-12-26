using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    //シングルトンインスタンス
    public static GameManager Instance { get; private set; }
    //ステージ情報
    public List<CharacterCapacity> reservedCharacterCapacities;    //プレイヤー0,NPC1~3(MenuScreenManagerのConfirmにて作成、BattleManagerのコンストラクタで使用)
    public int earnedExpInBattle;
    public int currentStageNum;//1から10
    public StageInfomation currentInfo;
    //SaveDataとして保存する情報群
    public CharacterCapacity playerCapacity;
    public int clearedStageNum;
    public int lv;
    [SerializeField] private int cumulativeExp;
    public bool development;
    //プロパティ
    public int CumulativeExp
    {
        get => cumulativeExp; 
        set
        {
            cumulativeExp = value;
            lv = ProcessLvAndExp.CalculateLv(value);
        }
    }

    void Awake()
    {
        // インスタンスがすでにある場合は自分を破壊
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // シーンを跨いでも消えないようにする
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        if (development) return;
        if (lv == 0)
        {
            SaveDataManager.Instance.ResetSaveData();
        }
        else
        {
            SaveDataManager.Instance.LoadGame();
        }
    }
}
