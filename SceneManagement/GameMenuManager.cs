using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject menu;

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
        if (NetworkManager.Singleton.IsListening && !GameObjectManager.Instance.IsGameOver() && GameObjectManager.Instance.IsOwnedPlayerAlive())
        {
            Debug.LogError("Exit click event is not implemented!");
            //EventManager.Instance.SendPlayerDiedEvent(GameObjectManager.Instance.GetOwnedPlayerId());
        }
        SceneManager.LoadScene("Start");
        //BoltLauncher.Shutdown();
    }

    public void OnResumeClick()
    {
        menu.SetActive(false);
    }
}