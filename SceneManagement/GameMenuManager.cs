using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuManager : NetworkBehaviour
{
    [SerializeField] private GameObject menu;

    private void Update()
    {
        if (GameObjectManager.Instance.LoadingBar.gameObject.activeSelf) return;

        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (menu.activeSelf)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
            menu.SetActive(false);
            //let the player move again
            GameObjectManager.Instance.EnableLocalPlayerMovement();
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            menu.SetActive(true);
            //do not let the player move
            GameObjectManager.Instance.DisableLocalPlayerMovement();
        }
    }

    public void OnExitClick()
    {
        try
        {
            if (NetworkManager.Singleton.IsHost)
            {
                var clientIds = NetworkManager.Singleton.ConnectedClients.Keys.ToList();
                foreach (var clientId in
                    clientIds.Where(clientId => clientId != NetworkManager.Singleton.LocalClientId))
                {
                    NetworkManager.Singleton.DisconnectClient(clientId);
                }

                Destroy(GameObject.FindWithTag("NetworkManager"));
                //NetworkManager.Singleton.Shutdown();
            }
            else
            {
                RoomInfoManager.Instance.ReportPlayerDeath(GameObjectManager.Instance.GetLocalPlayerId());
                Destroy(GameObject.FindWithTag("NetworkManager"));
            }
        }
        catch (NullReferenceException exception)
        {
            Debug.Log("NetworkManager doesn't exist. Stack trace: " + exception);
        }

        SceneManager.LoadScene("Start");
    }

    public void OnResumeClick()
    {
        menu.SetActive(false);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        try
        {
            if (NetworkManager.Singleton != null)
            {
                GameObjectManager.Instance.EnableLocalPlayerMovement();
            }
        }
        catch (NullReferenceException exception)
        {
            Debug.Log("NetworkManager doesn't exist. Stack trace: " + exception);
        }
    }
}