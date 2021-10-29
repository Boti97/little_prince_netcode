using Unity.Netcode;
using UnityEngine;

public class PlanetBehaviour : NetworkBehaviour
{
    private const float RotationSpeed = 0f;
    [SerializeField] private Transform orbitCenter;
    private Vector3 selfRotationAxis = Vector3.up;
    private float selfRotationSpeed;

    private void Update()
    {
        if (!IsOwner) return;

        if (orbitCenter != null)
        {
            transform.RotateAround(orbitCenter.position, Vector3.up, RotationSpeed * Time.deltaTime);
        }

        transform.RotateAround(transform.position, selfRotationAxis, selfRotationSpeed * Time.deltaTime);
    }


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        selfRotationSpeed = Random.Range(0.0f, selfRotationSpeed);
    }
}