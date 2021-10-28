using Unity.Netcode;
using UnityEngine;

public class RoomNetworkState : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<int> numberOfLivePlayers = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<ulong> playerWhoReported = new NetworkVariable<ulong>();
    [SerializeField] private NetworkVariable<byte> roomName = new NetworkVariable<byte>();
    [SerializeField] private NetworkVariable<int> roomSeed = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<bool> isRoomStarted = new NetworkVariable<bool>();

    public int RoomSeed
    {
        get => roomSeed.Value;
    }

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
    public void DecreaseNumberOfLivePlayersServerRpc(ulong playerWhoReported)
    {
        //TODO: implement checks

        numberOfLivePlayers.Value--;
        this.playerWhoReported.Value = playerWhoReported;
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncreaseNumberOfLivePlayersServerRpc(ulong playerWhoReported)
    {
        //TODO: implement checks

        numberOfLivePlayers.Value++;
        this.playerWhoReported.Value = playerWhoReported;
    }

    [ServerRpc]
    public void SetRoomNameServerRpc(byte roomName)
    {
        //TODO: implement checks

        this.roomName.Value = roomName;
    }

    [ServerRpc]
    public void SetRoomSeedServerRpc(int roomSeed)
    {
        //TODO: implement checks

        this.roomSeed.Value = roomSeed;
    }

    private void OnNumberOfLivePlayersChanged(ulong oldPlayerWhoReport, ulong newPlayerWhoReport)
    {
        if (!IsClient)
        {
            return;
        }

        //if there were at least one old playerid -> not the first player joining
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
            return;
        }

        //TODO: implement
    }
}