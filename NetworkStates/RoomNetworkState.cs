using Unity.Netcode;
using UnityEngine;

public class RoomNetworkState : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<int> numberOfLivePlayers = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<ulong> playerWhoReported = new NetworkVariable<ulong>();
    [SerializeField] private NetworkVariable<int> roomSeed = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<bool> isRoomStarted = new NetworkVariable<bool>();
    [SerializeField] private NetworkVariable<bool> isRoomLive = new NetworkVariable<bool>();

    public int RoomSeed => roomSeed.Value;
    public bool IsRoomLive => isRoomLive.Value;

    public override void OnNetworkSpawn()
    {
        if (!IsClient)
        {
            return;
        }

        playerWhoReported.OnValueChanged += OnNumberOfLivePlayersChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient)
        {
            return;
        }

        playerWhoReported.OnValueChanged -= OnNumberOfLivePlayersChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DecreaseNumberOfLivePlayersServerRpc(ulong playerId)
    {
        if (!NetworkManager.ConnectedClients.ContainsKey(playerId))
        {
            Debug.LogWarning(
                "Decrease number of players request dismissed, " +
                "since playerId doesn't belong to a connected player.");
            return;
        }

        if (numberOfLivePlayers.Value == 0)
        {
            Debug.LogWarning(
                "Decrease number of players request dismissed, " +
                "since number of live players is zero.");
            return;
        }

        numberOfLivePlayers.Value--;
        playerWhoReported.Value = playerId;
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncreaseNumberOfLivePlayersServerRpc(ulong playerId)
    {
        if (!NetworkManager.ConnectedClients.ContainsKey(playerId))
        {
            Debug.LogWarning(
                "Increase number of players request dismissed, " +
                "since playerId doesn't belong to a connected player.");
            return;
        }

        numberOfLivePlayers.Value++;
        playerWhoReported.Value = playerId;
    }

    [ServerRpc]
    public void SetRoomSeedServerRpc(int seed)
    {
        if (seed < 0 || seed > 100000)
        {
            Debug.LogWarning(
                "Seed setting request dismissed, " +
                "since seed is out of bounds. Seed: " + seed);
            return;
        }

        roomSeed.Value = seed;
    }
    
    [ServerRpc]
    public void SetIsRoomLiveServerRpc(bool isLive)
    {
        isRoomLive.Value = isLive;
    }

    private void OnNumberOfLivePlayersChanged(ulong oldPlayerWhoReport, ulong newPlayerWhoReport)
    {
        if (!IsClient)
        {
            return;
        }

        //if there were at least one old playerId -> not the first player joining
        //but the number of players alive is 1
        //and it is not us who reports (we cannot report that someone else died, so we won with our report)
        //we report on two occasions:
        // - when we enter the game (first condition!)
        // - when we die
        if (numberOfLivePlayers.Value == 1 && isRoomStarted.Value &&
            !newPlayerWhoReport.Equals(NetworkManager.Singleton.LocalClientId))
        {
            GameObjectManager.Instance.YouWonText.SetActive(true);
            GameObjectManager.Instance.DisableLocalPlayerMovement();
        }
        else if (IsServer)
        {
            isRoomStarted.Value = true;
            Debug.Log("First player joined!");
        }
        else
        {
            Debug.Log("Player reported.");
        }
    }
}