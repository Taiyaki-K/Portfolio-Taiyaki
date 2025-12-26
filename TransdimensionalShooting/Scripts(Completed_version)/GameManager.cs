using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Taiyaki
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public int dragonHp = 300;
        public int attackDuration = 2;
        public int attackInterval = 2;
        public string modeName = "Normal";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }




        public void GoToMainGame()
        {
            SEManager.Instance.Play(SEPath.T_FIRST_CLICK);
            SceneManager.LoadScene("MainGame");
        }
    }
}