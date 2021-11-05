using System.Collections;
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
    private GameObject EventPopUp { get; set; }
    private TMP_Text EventPopUpText { get; set; }

    public void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        joinOnlineGamePanel.SetActive(false);
        hostOnlineGamePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private void Start()
    {
        EventPopUp = GameObject.FindWithTag("EventPopUp");
        EventPopUpText = GameObject.FindWithTag("EventPopUpText").GetComponent<TMP_Text>();

        EventPopUp.SetActive(false);

        //if this is not empty it means there's a reason why this is scene is running now
        if (!string.IsNullOrEmpty(SceneLoadData.ReasonForSceneLoad))
        {
            StartCoroutine(PopUpEvent(SceneLoadData.ReasonForSceneLoad));
        }
        
        Destroy(GameObject.FindWithTag("NetworkManager"));
    }

    private IEnumerator PopUpEvent(string message)
    {
        EventPopUp.SetActive(true);
        EventPopUpText.text = message;
        SceneLoadData.ReasonForSceneLoad = "";
        yield return new WaitForSeconds(5);
        EventPopUp.SetActive(false);
    }

    public void OnClickHost()
    {
        hostIsCreating = true;
        GameObject.Find("HostButton").GetComponent<Button>().interactable = false;
        GameObject.Find("BackButton").GetComponent<Button>().interactable = false;
        GameObject.FindWithTag("NewRoomNameInputField").GetComponent<TMP_InputField>().interactable = false;
        SceneLoadData.chosenJoinMode = SceneLoadData.JoinMode.Host;
        SceneManager.LoadScene("Game");
    }

    public void OnClickJoin()
    {
        SceneLoadData.chosenJoinMode = SceneLoadData.JoinMode.Client;
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