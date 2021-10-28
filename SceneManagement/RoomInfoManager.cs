using UnityEngine;

public sealed class RoomInfoManager : MonoBehaviour
{
    private static readonly object padlock = new object();
    private static RoomInfoManager instance = null;

    [SerializeField] private RoomNetworkState roomNetworkState;

    public static RoomInfoManager Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new RoomInfoManager();
                }

                return instance;
            }
        }
    }

    public RoomNetworkState RoomNetworkState
    {
        get => roomNetworkState;
        set => roomNetworkState = value;
    }

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    //----------------------------------- NETWORK METHODS -----------------------------------
    public void DecreaseNumberOfLivePlayers(ulong playerWhoReported)
    {
        RoomNetworkState.DecreaseNumberOfLivePlayersServerRpc(playerWhoReported);
    }

    public void IncreaseNumberOfLivePlayers(ulong playerWhoReported)
    {
        RoomNetworkState.IncreaseNumberOfLivePlayersServerRpc(playerWhoReported);
    }

    public void SetRoomSeed(int roomSeed)
    {
        RoomNetworkState.SetRoomSeedServerRpc(roomSeed);
    }
}