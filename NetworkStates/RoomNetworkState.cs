using Unity.Netcode;
using UnityEngine;

public class RoomNetworkState : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<int> numberOfLivePlayers = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<ulong> playerWhoReported = new NetworkVariable<ulong>();
    [SerializeField] private NetworkVariable<byte> roomName = new NetworkVariable<byte>();
    [SerializeField] private NetworkVariable<int> roomSeed = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<bool> isRoomStarted = new NetworkVariable<bool>();

    public int RoomSeed => roomSeed.Value;

    public override void OnNetworkSpawn()
    {
        if (!IsClient)
        {
            return;
        }

        roomName.OnValueChanged += OnRoomNameChanged;
        playerWhoReported.OnValueChanged += OnNumberOfLivePlayersChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient)
        {
            return;
        }

        roomName.OnValueChanged -= OnRoomNameChanged;
        playerWhoReported.OnValueChanged -= OnNumberOfLivePlayersChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DecreaseNumberOfLivePlayersServerRpc(ulong playerId)
    {
        //TODO: implement checks

        numberOfLivePlayers.Value--;
        playerWhoReported.Value = playerId;
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncreaseNumberOfLivePlayersServerRpc(ulong playerId)
    {
        //TODO: implement checks

        numberOfLivePlayers.Value++;
        playerWhoReported.Value = playerId;
    }

    [ServerRpc]
    public void SetRoomNameServerRpc(byte nameOfRoom)
    {
        //TODO: implement checks

        roomName.Value = nameOfRoom;
    }

    [ServerRpc]
    public void SetRoomSeedServerRpc(int seed)
    {
        //TODO: implement checks

        roomSeed.Value = seed;
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
            GameObjectManager.Instance.CinemachineVirtualCamera.gameObject.SetActive(false);
            GameObjectManager.Instance.GetOwnedPlayerById(NetworkManager.Singleton.LocalClientId)
                .GetComponent<PlayerBehaviour>().enabled = false;
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

    private void OnRoomNameChanged(byte oldPlanetRotation, byte newPlanetRotation)
    {
        if (!IsClient)
        {
        }

        //TODO: implement
    }
}