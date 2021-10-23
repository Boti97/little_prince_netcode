using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuManager
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
        Debug.LogError("Exit click is not implemented!");
        //if (NetworkManager. && !GameObjectManager.Instance.IsGameOver() && GameObjectManager.Instance.IsOwnedPlayerAlive())
        //{
        //    EventManager.Instance.SendPlayerDiedEvent(GameObjectManager.Instance.GetOwnedPlayerId());
        //}
        //SceneManager.LoadScene("Start");
        //BoltLauncher.Shutdown();
    }

    public void OnResumeClick()
    {
        menu.SetActive(false);
    }
}