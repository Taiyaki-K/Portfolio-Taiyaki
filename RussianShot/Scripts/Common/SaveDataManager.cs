using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData
{
    //PlayerCapacity関連
    public int hpInCap;
    public int fortune;
    public int stealth;
    public List<SkillType> skills;
    //その他
    public int clearedStageNum;
    public int lv;
    public int cumulativeExp;
}

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.hpInCap = GameManager.Instance.playerCapacity.HpInCap;
        data.fortune = GameManager.Instance.playerCapacity.Fortune;
        data.stealth = GameManager.Instance.playerCapacity.Stealth;
        data.skills = GameManager.Instance.playerCapacity.skills;
        data.clearedStageNum = GameManager.Instance.clearedStageNum;
        data.lv = GameManager.Instance.lv;
        data.cumulativeExp = GameManager.Instance.CumulativeExp;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveKey", json);
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SaveKey"))
        {
            string json = PlayerPrefs.GetString("SaveKey");
            Debug.Log(json);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            GameManager.Instance.clearedStageNum = data.clearedStageNum;
            GameManager.Instance.lv = data.lv;
            GameManager.Instance.CumulativeExp = data.cumulativeExp;
            
            GameManager.Instance.playerCapacity.HpInCap = data.hpInCap;
            GameManager.Instance.playerCapacity.Fortune = data.fortune;
            GameManager.Instance.playerCapacity.Stealth = data.stealth;
            GameManager.Instance.playerCapacity.skills = data.skills != null ? new List<SkillType>(data.skills) : new List<SkillType>();
        }
        else
        {
            Debug.LogWarning("セーブデータがありません！");
        }
    }

    public void ResetSaveData()
    {
        SaveData data = new SaveData();
        data.hpInCap = 1;
        data.fortune = 0;
        data.stealth = 0;
        data.skills = new List<SkillType>();
        data.clearedStageNum = 0;
        data.lv = 1;
        data.cumulativeExp = 0;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveKey", json);
        PlayerPrefs.Save();
        Debug.Log(json);
        Destroy(GameManager.Instance);
        SceneManager.LoadScene("MenuScene");
    }

    public void SetCompleteSaveData()
    {
        SaveData data = new SaveData();
        data.hpInCap = 10;
        data.fortune = 10;
        data.stealth = 10;
        data.skills = new List<SkillType>();
        data.clearedStageNum = 10;
        data.lv = 30;
        data.cumulativeExp = 85550;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveKey", json);
        PlayerPrefs.Save();
        Debug.Log(json);
        Destroy(GameManager.Instance);
        SceneManager.LoadScene("MenuScene");
    }
}