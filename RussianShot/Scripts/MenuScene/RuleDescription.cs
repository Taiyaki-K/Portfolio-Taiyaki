using UnityEngine;
using UnityEngine.UI;

public class RuleDescription : MonoBehaviour
{
    public GameObject basicScreen;
    public GameObject screenScreen;
    public GameObject itemScreen;
    public GameObject skillScreen;
    public GameObject capacityScreen;
    public GameObject hateScreen;
    public GameObject othersScreen;

    public GameObject basicButton;
    public GameObject screenButton;
    public GameObject itemButton;
    public GameObject skillButton;
    public GameObject capacityButton;
    public GameObject hateButton;
    public GameObject othersButton;

    private GameObject lastPressedButton;


    public void RemoveAllScreen()
    {
        basicScreen.SetActive(false);
        screenScreen.SetActive(false);
        itemScreen.SetActive(false);
        skillScreen.SetActive(false);
        capacityScreen.SetActive(false);
        hateScreen.SetActive(false);
        othersScreen.SetActive(false);
    }
    public void OnButtonPushed(GameObject pressedButton)
    {
        lastPressedButton = pressedButton;
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        // 対象のボタンを全部リスト化
        GameObject[] buttons = { basicButton,screenButton, itemButton, skillButton, capacityButton, hateButton ,othersButton};

        foreach (var btn in buttons)
        {
            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                if (btn == lastPressedButton)
                {
                    img.color = new Color(38f / 255f, 74f / 255f, 115f / 255f, 186f / 255f); // 不透明（元の色）
                }
                else
                {
                    img.color = new Color(38f / 255f, 74f / 255f, 115f / 255f, 40f / 255f); // 半透明
                }
            }
        }
    }
public void SetScreen(GameObject screen)
    { 
        RemoveAllScreen();
        screen.SetActive(true); 
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnButtonPushed(basicButton);
        SetScreen(basicScreen);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
