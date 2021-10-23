using Unity.Netcode;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartSceneManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject backFromJoiningGameButton;

    [SerializeField]
    private GameObject mainMenuPanel;

    [SerializeField]
    private GameObject joinOnlineGamePanel;

    [SerializeField]
    private GameObject hostOnlineGamePanel;

    [SerializeField]
    private GameObject roomPrefab;

    [SerializeField]
    private GameObject roomListContent;

    private bool hostIsCreating = false;
    private List<GameObject> roomList = new List<GameObject>();

    public void Awake()
    {
        //Cursor.lockState = CursorLockMode.Confined;
        //Cursor.visible = true;

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
        //BoltLauncher.StartServer();
    }

    //public override void BoltStartDone()
    //{
    //    //if (BoltNetwork.IsServer)
    //    //{
    //    //    BoltNetwork.RegisterTokenClass<PhotonRoomProperties>();
    //    //    PhotonRoomProperties token = new PhotonRoomProperties();
    //    //    token.AddRoomProperty("roomName", GameObject.FindWithTag("NewRoomNameInputField").GetComponent<TMP_InputField>().text);
    //    //    BoltMatchmaking.CreateSession(sessionID: Guid.NewGuid().ToString(), sceneToLoad: "Game", token: token);
    //    //}
    //}

    //public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    //{
    //    ClearRooms();
    //    int i = 0;
    //    foreach (var session in sessionList)
    //    {
    //        PhotonSession photonSession = session.Value as PhotonSession;

    //        GameObject room = Instantiate(roomPrefab, roomListContent.transform);
    //        room.gameObject.SetActive(true);
    //        room.transform.position = new Vector3(room.transform.position.x, room.transform.position.y - i * 100, room.transform.position.z);
    //        room.GetComponentInChildren<TextMeshProUGUI>().text = photonSession.Properties["roomName"] as string;
    //        room.GetComponentInChildren<Button>().onClick.AddListener(() => OnClickJoinGame(photonSession));

    //        roomList.Add(room);
    //        i++;
    //    }
    //}

    public void OnClickExit()
    {
        Application.Quit();
    }

    public void OnClickExitJoiningGame()
    {
        backFromJoiningGameButton.SetActive(false);
        //BoltLauncher.Shutdown();
    }

    public void OnClickJoinOnlineGame()
    {
        joinOnlineGamePanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        //BoltLauncher.StartClient();
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
        //BoltLauncher.Shutdown();
    }

    public void OnClickBackFromHost()
    {
        GameObject.FindWithTag("NewRoomNameInputField").GetComponent<TMP_InputField>().text = "";
        hostOnlineGamePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnValueChangeForNewRoomNameInputFieldText()
    {
        string nameOfRoom = GameObject.FindWithTag("NewRoomNameInputField").GetComponent<TMP_InputField>().text;
        if (nameOfRoom != null && !nameOfRoom.Equals("") && !hostIsCreating)
        {
            GameObject.Find("HostButton").GetComponent<Button>().interactable = true;
        }
        else
        {
            GameObject.Find("HostButton").GetComponent<Button>().interactable = false;
        }
    }

    //private void OnClickJoinGame(PhotonSession photonSession)
    //{
    //    BoltMatchmaking.JoinSession(photonSession);
    //}

    private void ClearRooms()
    {
        foreach (GameObject room in roomList)
        {
            //Destroy(room);
        }

        roomList.Clear();
    }
}