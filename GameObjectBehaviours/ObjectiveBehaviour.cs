using UnityEngine;

public class ObjectiveBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider player)
    {
        if (player.gameObject.tag.Equals("Player"))
        {
            player.gameObject.GetComponent<PlayerBehaviour>().AddHealth(0.2f);
            GameObjectManager.Instance.IncreaseScore();
            gameObject.GetComponent<ObjectiveNetworkState>().ObjectiveAcquiredServerRpc();
        }
    }
}