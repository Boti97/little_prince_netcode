using Unity.Netcode;
using UnityEngine;

public class PlanetBehaviour : NetworkBehaviour
{
    [SerializeField] private Vector3 orbitCenter = Vector3.zero;
    private Vector3 selfRotationAxis = Vector3.up;
    private float selfRotationSpeed;

    private void Update()
    {
        if (!IsOwner) return;

        transform.RotateAround(orbitCenter, Vector3.up,
            GameObjectManager.Instance.rotationSpeed * NetworkManager.Singleton.ServerTime.FixedDeltaTime);
        transform.RotateAround(transform.position, selfRotationAxis,
            selfRotationSpeed * NetworkManager.Singleton.ServerTime.FixedDeltaTime);
    }


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        selfRotationSpeed = Random.Range(0.0f, GameObjectManager.Instance.selfRotationSpeed);
        selfRotationAxis = VectorExtensions.RandomAxis();
    }
}