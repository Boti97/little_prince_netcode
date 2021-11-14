using Unity.Netcode;
using UnityEngine;

public class SkyboxBehaviour : NetworkBehaviour
{
    [SerializeField] private float skyboxSpeed = 0;
    private float skyboxRotation = 1;

    private void Update()
    {
        if (!RoomInfoManager.Instance.RoomNetworkState.IsRoomStarted) return;

        RenderSettings.skybox.SetFloat("_Rotation",
            NetworkManager.Singleton.ServerTime.FixedDeltaTime * skyboxRotation);
        skyboxRotation += skyboxSpeed;
    }
}