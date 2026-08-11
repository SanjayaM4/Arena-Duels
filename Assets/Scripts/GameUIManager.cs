using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    public GameObject gameplayPanel;
    public GameObject endPanel;
    public TextMeshProUGUI endText;
    public Button rematchButton;
    public Button leaveRoomButton;
    public GameObject menuPanel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        menuPanel.SetActive(true);
        gameplayPanel.SetActive(false);
        endPanel.SetActive(false);

        rematchButton.onClick.AddListener(OnRematchClicked);
        leaveRoomButton.onClick.AddListener(OnLeaveRoomClicked);
    }

    public void ShowGameplay()
    {
        menuPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        endPanel.SetActive(false);
    }

    public void ShowEndScreen(bool localPlayerWon)
    {
        menuPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        endPanel.SetActive(true);
        endText.text = localPlayerWon ? "You Win!" : "You Lose!";
    }

    public void ShowMenu()
    {
        menuPanel.SetActive(true);
        gameplayPanel.SetActive(false);
        endPanel.SetActive(false);
    }

    void OnRematchClicked()
    {
        GameManager.Instance.RequestRematchServerRpc();
    }

    void OnLeaveRoomClicked()
    {
        GameManager.Instance.LeaveRoom();
    }
}