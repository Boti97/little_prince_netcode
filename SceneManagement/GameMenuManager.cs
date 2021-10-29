using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuManager : NetworkBehaviour
{
    [SerializeField] private GameObject menu;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (menu.activeSelf)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
            menu.SetActive(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            menu.SetActive(true);
        }
    }

    public void OnExitClick()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            var clientIds = NetworkManager.Singleton.ConnectedClients.Keys.ToList();
            foreach (var clientId in clientIds.Where(clientId => clientId != NetworkManager.Singleton.LocalClientId))
            {
                NetworkManager.Singleton.DisconnectClient(clientId);
            }

            NetworkManager.Singleton.Shutdown();
        }
        else
        {
            RoomInfoManager.Instance.DecreaseNumberOfLivePlayers(NetworkManager.Singleton.LocalClientId);
        }

        SceneManager.LoadScene("Start");
    }

    public void OnResumeClick()
    {
        menu.SetActive(false);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }
}