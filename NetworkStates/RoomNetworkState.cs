using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoomNetworkState : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<int> numberOfLivePlayers = new NetworkVariable<int>(0);

    //holds the last PLAYERID (NetworkObjectId of the little prince gameobject) who died
    [SerializeField] private NetworkVariable<ulong> diedPlayerId = new NetworkVariable<ulong>();

    //holds the last PLAYERID (NetworkObjectId of the little prince gameobject) who joined
    [SerializeField] private NetworkVariable<ulong> joinedPlayerId = new NetworkVariable<ulong>();
    [SerializeField] private NetworkVariable<int> roomSeed = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<bool> isRoomStarted = new NetworkVariable<bool>();
    [SerializeField] private NetworkVariable<bool> isRoomLive = new NetworkVariable<bool>();

    private List<ulong> playerIds = new List<ulong>();
    public int RoomSeed => roomSeed.Value;
    public bool IsRoomLive => isRoomLive.Value;

    public override void OnNetworkSpawn()
    {
        if (!IsClient)
        {
            return;
        }

        if (IsHost)
        {
            diedPlayerId.Value = ulong.MaxValue;
            joinedPlayerId.Value = ulong.MaxValue;
        }

        numberOfLivePlayers.OnValueChanged += OnNumberOfLivePlayersChanged;
        diedPlayerId.OnValueChanged += OnDiedPlayerIdChanged;
        joinedPlayerId.OnValueChanged += OnJoinedPlayerIdChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient)
        {
            return;
        }

        numberOfLivePlayers.OnValueChanged -= OnNumberOfLivePlayersChanged;
        diedPlayerId.OnValueChanged -= OnDiedPlayerIdChanged;
        joinedPlayerId.OnValueChanged -= OnJoinedPlayerIdChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReportPlayerDeathServerRpc(ulong playerId)
    {
        if (!playerIds.Contains(playerId))
        {
            Debug.LogWarning(
                "Player death report request dismissed, " +
                "since playerId doesn't belong to a living player.");
            return;
        }

        if (numberOfLivePlayers.Value == 0)
        {
            Debug.LogWarning(
                "Player death report request dismissed, " +
                "since number of live players is zero.");
            return;
        }

        if (diedPlayerId.Value.Equals(playerId))
        {
            Debug.LogWarning(
                "Player death report request dismissed, " +
                "since it is equals with the last died player id.");
            return;
        }

        GameObjectManager.Instance.CreateHeadstoneForPlayer(playerId);
        numberOfLivePlayers.Value--;
        diedPlayerId.Value = playerId;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReportPlayerJoinedServerRpc(ulong playerId)
    {
        if (playerIds.Contains(playerId))
        {
            Debug.LogWarning(
                "Player death report request dismissed, " +
                "since playerId already belongs to a joined player.");
            return;
        }

        if (isRoomStarted.Value && joinedPlayerId.Value.Equals(playerId))
        {
            Debug.LogWarning(
                "Player join report request dismissed, " +
                "since it is equals with the last died player id.");
            return;
        }

        numberOfLivePlayers.Value++;
        joinedPlayerId.Value = playerId;
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

    private void OnDiedPlayerIdChanged(ulong oldDiedPlayerId, ulong newDiedPlayerId)
    {
        if (!IsClient)
        {
            return;
        }

        if (numberOfLivePlayers.Value == 1
            && isRoomStarted.Value
            && !newDiedPlayerId.Equals(GameObjectManager.Instance.GetLocalPlayerId())
            && !GameObjectManager.Instance.IsGameOver())
        {
            Debug.Log("Player died, and only one player is alive!");
            GameObjectManager.Instance.YouWonText.SetActive(true);
            GameObjectManager.Instance.DisableLocalPlayerMovement();
        }
        else
        {
            Debug.Log("Player died!");
        }

        //TODO: add popup 
        GameObjectManager.Instance.DisablePlayerById(newDiedPlayerId);
        playerIds.Remove(newDiedPlayerId);
    }

    private void OnNumberOfLivePlayersChanged(int oldNumberOfLivePlayers, int newNumberOfLivePlayers)
    {
        if (!IsClient)
        {
            return;
        }

        if (oldNumberOfLivePlayers > newNumberOfLivePlayers
            && newNumberOfLivePlayers == 1
            && isRoomStarted.Value
            && !diedPlayerId.Value.Equals(GameObjectManager.Instance.GetLocalPlayerId())
            && !GameObjectManager.Instance.IsGameOver())
        {
            Debug.Log("Player died, and only one player is alive!");
            //TODO: add popup 
            GameObjectManager.Instance.YouWonText.SetActive(true);
            GameObjectManager.Instance.DisableLocalPlayerMovement();
        }
    }

    private void OnJoinedPlayerIdChanged(ulong oldJoinedPlayerId, ulong newJoinedPlayerId)
    {
        if (!IsClient)
        {
            return;
        }

        //if it is false, it means we are the first player 
        if (!isRoomStarted.Value)
        {
            isRoomStarted.Value = true;
            Debug.Log("First player joined!");
        }
        else
        {
            Debug.Log("Player joined!");
        }

        //TODO: add popup
        playerIds.Add(newJoinedPlayerId);
        GameObjectManager.Instance.RefreshPlayers();
    }
}