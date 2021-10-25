using Unity.Netcode;

public class ServerNetworkState : NetworkBehaviour
{
    private NetworkVariable<byte> roomName = new NetworkVariable<byte>();
    private NetworkVariable<int> roomSeed = new NetworkVariable<int>();

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
        roomSeed.OnValueChanged += OnRoomSeedChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient)
        {
            return;
        }

        roomName.OnValueChanged -= OnRoomNameChanged;
        roomSeed.OnValueChanged -= OnRoomSeedChanged;
    }

    [ServerRpc]
    public void SetRoomNameServerRpc(byte roomName)
    {
        //TODO: implement checks

        this.roomName.Value = roomName;
    }

    [ServerRpc]
    public void SetRoomSeedServerRpc(int seed)
    {
        //TODO: implement checks

        roomSeed.Value = seed;
    }

    private void OnRoomNameChanged(byte oldPlanetRotation, byte newPlanetRotation)
    {
        if (!IsClient)
        {
            return;
        }

        //TODO: implement
    }

    private void OnRoomSeedChanged(int oldPlanetPosition, int newPlanetPosition)
    {
        if (!IsClient)
        {
            return;
        }

        //TODO: implement
    }
}