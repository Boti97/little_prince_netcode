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

        GameObject.FindWithTag("PlayerNameText").GetComponent<TMP_Text>().text = SceneLoadData.Username;
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
        SceneLoadData.chosenJoinMode = SceneLoadData.JoinMode.Host;
        SceneLoadData.chosenGameMode = SceneLoadData.GameMode.Multi;

        SceneLoadData.IPAddress = GameObject.FindWithTag("IPAddressInputField").GetComponent<TMP_InputField>().text;
        SceneLoadData.Port = int.Parse(GameObject.FindWithTag("PortInputField").GetComponent<TMP_InputField>().text);
        SceneManager.LoadScene("MultiplayerGame");
    }

    public void OnClickJoin()
    {
        SceneLoadData.chosenJoinMode = SceneLoadData.JoinMode.Client;
        SceneLoadData.chosenGameMode = SceneLoadData.GameMode.Multi;
        SceneLoadData.IPAddress = GameObject.FindWithTag("IPAddressInputField").GetComponent<TMP_InputField>().text;
        SceneLoadData.Port = int.Parse(GameObject.FindWithTag("PortInputField").GetComponent<TMP_InputField>().text);
        SceneManager.LoadScene("MultiplayerGame");
    }

    public void OnClickSinglePlay()
    {
        SceneLoadData.chosenJoinMode = SceneLoadData.JoinMode.Host;
        SceneLoadData.chosenGameMode = SceneLoadData.GameMode.Single;

        SceneLoadData.IPAddress = "127.0.0.1";
        SceneLoadData.Port = 7777;
        SceneManager.LoadScene("SingleplayerGame");
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

        GameObject.FindWithTag("IPAddressInputField").GetComponent<TMP_InputField>().text = "127.0.0.1";
        GameObject.FindWithTag("PortInputField").GetComponent<TMP_InputField>().text = "9998";
    }

    public void OnClickHostOnlineGame()
    {
        hostOnlineGamePanel.SetActive(true);
        mainMenuPanel.SetActive(false);

        GameObject.FindWithTag("IPAddressInputField").GetComponent<TMP_InputField>().text = "127.0.0.1";
        GameObject.FindWithTag("PortInputField").GetComponent<TMP_InputField>().text = "9998";
    }

    public void OnClickBackFromJoin()
    {
        joinOnlineGamePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnClickBackFromHost()
    {
        hostOnlineGamePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnClickLogout()
    {
        SceneLoadData.ReasonForSceneLoad = "Logout";
        SceneManager.LoadScene("Login");
    }

    public void OnValueChangeForHostInputFields()
    {
        var ipAddress = GameObject.FindWithTag("IPAddressInputField").GetComponent<TMP_InputField>().text;
        var port = GameObject.FindWithTag("PortInputField").GetComponent<TMP_InputField>().text;
        if (ipAddress != null
            && !ipAddress.Equals("")
            && port != null
            && !port.Equals(""))
        {
            GameObject.Find("HostButton").GetComponent<Button>().interactable = true;
        }
        else
        {
            GameObject.Find("HostButton").GetComponent<Button>().interactable = false;
        }
    }

    public void OnValueChangeForJoinInputFields()
    {
        var ipAddress = GameObject.FindWithTag("IPAddressInputField").GetComponent<TMP_InputField>().text;
        var port = GameObject.FindWithTag("PortInputField").GetComponent<TMP_InputField>().text;
        if (ipAddress != null
            && !ipAddress.Equals("")
            && port != null
            && !port.Equals(""))
        {
            GameObject.Find("JoinButton").GetComponent<Button>().interactable = true;
        }
        else
        {
            GameObject.Find("JoinButton").GetComponent<Button>().interactable = false;
        }
    }
}