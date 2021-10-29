using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject backFromJoiningGameButton;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject joinOnlineGamePanel;
    [SerializeField] private GameObject hostOnlineGamePanel;

    private bool hostIsCreating;

    public void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        joinOnlineGamePanel.SetActive(false);
        hostOnlineGamePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnClickHost()
    {
        hostIsCreating = true;
        GameObject.Find("HostButton").GetComponent<Button>().interactable = false;
        GameObject.Find("BackButton").GetComponent<Button>().interactable = false;
        GameObject.FindWithTag("NewRoomNameInputField").GetComponent<TMP_InputField>().interactable = false;
        SceneNetworkData.chosenJoinMode = SceneNetworkData.JoinMode.Host;
        SceneManager.LoadScene("Game");
    }

    public void OnClickJoin()
    {
        SceneNetworkData.chosenJoinMode = SceneNetworkData.JoinMode.Client;
        SceneManager.LoadScene("Game");
    }

    public void OnClickExit()
    {
        Application.Quit();
    }

    public void OnClickExitJoiningGame()
    {
        backFromJoiningGameButton.SetActive(false);
    }

    public void OnClickJoinOnlineGame()
    {
        joinOnlineGamePanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void OnClickHostOnlineGame()
    {
        hostOnlineGamePanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void OnClickBackFromJoin()
    {
        joinOnlineGamePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnClickBackFromHost()
    {
        GameObject.FindWithTag("NewRoomNameInputField").GetComponent<TMP_InputField>().text = "";
        hostOnlineGamePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnValueChangeForNewRoomNameInputFieldText()
    {
        var nameOfRoom = GameObject.FindWithTag("NewRoomNameInputField").GetComponent<TMP_InputField>().text;
        if (nameOfRoom != null && !nameOfRoom.Equals("") && !hostIsCreating)
        {
            GameObject.Find("HostButton").GetComponent<Button>().interactable = true;
        }
        else
        {
            GameObject.Find("HostButton").GetComponent<Button>().interactable = false;
        }
    }
}