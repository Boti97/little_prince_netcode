using Unity.Netcode;
using UnityEngine;

public class PlanetBehaviour : NetworkBehaviour
{
    private const float RotationSpeed = 0f;
    [SerializeField] private Vector3 orbitCenter = Vector3.zero;
    private Vector3 selfRotationAxis = Vector3.up;
    private float selfRotationSpeed = 0f;

    private void Update()
    {
        if (!IsOwner) return;

        transform.RotateAround(orbitCenter, Vector3.up,
            RotationSpeed * NetworkManager.Singleton.ServerTime.FixedDeltaTime);
        transform.RotateAround(transform.position, selfRotationAxis,
            selfRotationSpeed * NetworkManager.Singleton.ServerTime.FixedDeltaTime);
    }


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        selfRotationSpeed = Random.Range(0.0f, selfRotationSpeed);
        selfRotationAxis = VectorExtensions.RandomAxis();
    }
}