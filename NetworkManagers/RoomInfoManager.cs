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
}