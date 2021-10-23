using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetNetworkState : NetworkBehaviour
{
    private NetworkVariable<Quaternion> planetRotation = new NetworkVariable<Quaternion>();
    private NetworkVariable<Vector3> planetPosition = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        if (!IsClient) { return; }

        planetRotation.OnValueChanged += OnPlanetRotationChanged;
        planetPosition.OnValueChanged += OnPlanetPositionChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient) { return; }

        planetRotation.OnValueChanged -= OnPlanetRotationChanged;
        planetPosition.OnValueChanged -= OnPlanetPositionChanged;
    }

    [ServerRpc]
    public void SetPlanetRotationServerRpc(Quaternion planetRotation)
    {
        //TODO: implement checks

        this.planetRotation.Value = planetRotation;
    }

    [ServerRpc]
    public void SetPlanetPositionServerRpc(Vector3 planetPosition)
    {
        //TODO: implement checks

        this.planetPosition.Value = planetPosition;
    }

    private void OnPlanetRotationChanged(Quaternion oldPlanetRotation, Quaternion newPlanetRotation)
    {
        if (!IsClient) { return; }

        transform.rotation = newPlanetRotation;
    }

    private void OnPlanetPositionChanged(Vector3 oldPlanetPosition, Vector3 newPlanetPosition)
    {
        if (!IsClient) { return; }

        transform.position = newPlanetPosition;
    }
}
