using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using KanKikuchi.AudioManager;
using UnityEngine;
using UnityEngine.UI;


public class BattleSceneManager : MonoBehaviour
{
    public Image fadeImage;

    public GameObject mainCamera;
    //ステージ10で消すもの
    public GameObject darkRoom;
    public GameObject saku;
    public static BattleSceneManager Instance { get; private set; }

    void Awake()
    {
        // インスタンスがすでにある場合は自分を破壊
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // 自分をインスタンスとして登録
        Instance = this;
    }
    //すべてのStartを集約
    async void Start()
    {
        //キャラにつくコンポーネントのInitializeはコンストラクタ内で済ます
        BGMManager.Instance.Play(
                                   audioPath: BGMPath.BATTLE_BGM, //再生したいオーディオのパス
                                   volumeRate: 1,                //音量の倍率
                                   delay: 0,                //再生されるまでの遅延時間
                                   pitch: 1,                //ピッチ
                                   isLoop: true,             //ループ再生するか
                                   allowsDuplicate: false             //他のBGMと重複して再生させるか
                                 );
        fadeImage.color = Color.black;
        fadeImage.DOFade(0f, 1f);
        currentBattleManager = new BattleManager();
        battleUIManager.Initialize();
        charaIndicatorsManager.Initialize();
        animationManager.Initialize();
        endInitialize = true;
        await UniTask.WaitForSeconds(1f);
        if (GameManager.Instance.currentStageNum == 10)
        {
            Destroy(darkRoom);
            Destroy(saku);
            mainCamera.transform.DORotate(new Vector3(-10f, -114.58f, 0f), 3f);
            await UniTask.WaitForSeconds(3f);
            mainCamera.transform.DORotate(new Vector3(37.88f, 0f, 0f), 3f);
            await UniTask.WaitForSeconds(3f);
        }
        await currentBattleManager.RunBattleAsync();
    }

    public bool endInitialize = false;

    //GameObject参照
    public BattleUIManager battleUIManager;
    public CharaIndicatorsManager charaIndicatorsManager;
    public AnimationManager animationManager;
    public VolumeController volumeController;
    public BattlePostProcess battlePostProcess;

    //new演算子参照
    [System.NonSerialized] public BattleManager currentBattleManager;
}
