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
        Debug.Log("ShowGameplay called - this might be firing unexpectedly");

        menuPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        endPanel.SetActive(false);
    }

    public void ShowEndScreen(bool localPlayerWon)
    {
        Debug.Log("ShowEndScreen called - this might be firing unexpectedly");

        menuPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        endPanel.SetActive(true);
        endText.text = localPlayerWon ? "You Win!" : "You Lose!";
    }

    public void ShowMenu()
    {
        Debug.Log("ShowMenu running - menuPanel: " + (menuPanel != null) + ", current active state: " + menuPanel.activeSelf);

        menuPanel.SetActive(true);
        gameplayPanel.SetActive(false);
        endPanel.SetActive(false);

        Debug.Log("ShowMenu finished - menuPanel now active: " + menuPanel.activeSelf + ", endPanel now active: " + endPanel.activeSelf);
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