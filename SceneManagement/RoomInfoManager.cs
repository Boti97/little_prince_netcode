using UnityEngine;

public sealed class RoomInfoManager : MonoBehaviour
{
    private static readonly object padlock = new object();
    private static RoomInfoManager _instance;

    [SerializeField] private RoomNetworkState roomNetworkState;

    public RoomNetworkState RoomNetworkState => roomNetworkState;

    public static RoomInfoManager Instance
    {
        get
        {
            lock (padlock)
            {
                if (_instance == null)
                {
                    _instance = new RoomInfoManager();
                }

                return _instance;
            }
        }
    }

    public void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    //----------------------------------- NETWORK METHODS -----------------------------------
    public void ReportPlayerDeath(ulong playerWhoReported)
    {
        RoomNetworkState.ReportPlayerDeathServerRpc(playerWhoReported);
    }

    public void ReportJoinedPlayer(ulong playerWhoReported)
    {
        RoomNetworkState.ReportPlayerJoinedServerRpc(playerWhoReported);
    }

    public void SetRoomSeed(int roomSeed)
    {
        RoomNetworkState.SetRoomSeedServerRpc(roomSeed);
    }

    public void SetIsRoomLive(bool isLive)
    {
        RoomNetworkState.SetIsRoomLiveServerRpc(isLive);
    }

    public bool IsRoomLive()
    {
        return RoomNetworkState.IsRoomLive;
    }
}