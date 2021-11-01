using Unity.Netcode;
using UnityEngine;

public class ObjectiveNetworkState : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<bool> isAcquired = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        if (!IsClient)
        {
            return;
        }

        isAcquired.OnValueChanged += ObjectiveAcquired;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ObjectiveAcquiredServerRpc()
    {
        isAcquired.Value = true;
    }

    private void ObjectiveAcquired(bool oldIsAcquired, bool newIsAcquired)
    {
        if (!newIsAcquired)
        {
            Debug.LogWarning("New value of IsAcquired should not be false.");
            return;
        }

        gameObject.SetActive(false);
    }
}