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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menu.activeSelf) menu.SetActive(false);
            else menu.SetActive(true);
            //Cursor.visible = true;
        }
    }

    public void OnExitClick()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            List<ulong> clientIds = NetworkManager.Singleton.ConnectedClients.Keys.ToList();
            foreach (var clientId in clientIds)
            {
                if (clientId != NetworkManager.Singleton.LocalClientId)
                {
                    NetworkManager.Singleton.DisconnectClient(clientId);
                }
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
    }
}