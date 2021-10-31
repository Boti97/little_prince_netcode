using Unity.Netcode;
using UnityEngine;

public class PlayerBehaviour : CharacterBehaviour
{
    private bool hitZeroStamina;
    private Transform localCamera;

    public void AddHealth(float plusHealth)
    {
        if (health + plusHealth < 1f)
        {
            health += plusHealth;
        }
        else
        {
            health = 1f;
        }
    }

    protected override void CalculateMovingDirection()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");
        moveDir = new Vector3(horizontal, 0f, vertical).normalized;

        var up = transform.up;
        var cameraRelativeFacingDir = Vector3.ProjectOnPlane(localCamera.forward, up).normalized;
        var anglePlayerForwardCameraForward = Vector3.SignedAngle(cameraRelativeFacingDir, transform.forward, up);
        finalDir = (Quaternion.AngleAxis(-anglePlayerForwardCameraForward, up) *
                    transform.TransformDirection(moveDir)).normalized;
    }

    protected override void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || isJumpEnabled))
        {
            var facingDir = moveDir.Equals(Vector3.zero)
                ? transform.GetChild(0).transform.forward.normalized
                : finalDir;

            GetComponent<Rigidbody>()
                .AddForce(Vector3.RotateTowards(
                              facingDir,
                              transform.up,
                              30 * Mathf.Deg2Rad,
                              0)
                          * jumpForce);

            SetAnimation("isJumped", 1);
            SetAnimation("isGrounded", 0);

            numberOfJumps++;
            isGrounded = false;
            isJumping = true;
        }
        else
        {
            isJumping = false;
        }

        if (numberOfJumps > 1)
        {
            isJumpEnabled = false;
            numberOfJumps = 0;
        }
    }

    protected override void InitializeCharacterSpecificFields()
    {
        if (Camera.main is { }) localCamera = Camera.main.gameObject.transform;

        GameObjectManager.Instance.CinemachineVirtualCamera.LookAt = transform;
        GameObjectManager.Instance.CinemachineVirtualCamera.Follow = transform;

        GameObjectManager.Instance.RefreshPlayers();
    }

    protected override void HandleSprint()
    {
        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0f && !hitZeroStamina)
        {
            moveSpeed = sprintSpeed;
            stamina -= 0.003f;
            GameObjectManager.Instance.StaminaBar.value = stamina;

            if (stamina < 0.005)
            {
                hitZeroStamina = true;
            }
        }
        else
        {
            if (stamina < 1f)
            {
                stamina += 0.0005f;
                GameObjectManager.Instance.StaminaBar.value = stamina;
            }

            if (stamina > 0.3f)
            {
                hitZeroStamina = false;
            }

            moveSpeed = walkSpeed;
        }
    }

    protected override void HandleThrust()
    {
        if (Input.GetMouseButtonDown(1) && thrust > 0.33f)
        {
            thrust -= 0.33f;
            GameObjectManager.Instance.ThrustBar.value = thrust;
            GetComponent<Rigidbody>().AddForce(localCamera.forward * thrustPower);
        }
        else if (thrust < 1f)
        {
            thrust += 0.001f;
            GameObjectManager.Instance.ThrustBar.value = thrust;
        }
    }

    protected override void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObjectManager.Instance.Players.ForEach(player =>
            {
                //we don't want to attack ourselves
                if (player.GetComponent<NetworkObject>().NetworkObjectId == NetworkObjectId) return;

                if (Vector3.Distance(player.transform.position, transform.position) < 1.5f)
                {
                    Debug.LogWarning("Player attack is not implemented!");
                }
            });
        }
    }

    protected override void CheckHealth()
    {
        if (gravityBody.AttractorCount() != 0 || !NetworkObject.IsSpawned ||
            !(NetworkManager.ServerTime.TimeAsFloat > 5f)) return;

        health -= 0.002f;
        GameObjectManager.Instance.HealthBar.value = health;

        if (health < 0f)
        {
            GameObjectManager.Instance.CinemachineVirtualCamera.gameObject.SetActive(false);
            //GameObjectManager.Instance.SetObjectsForPlayerDeath(gameObject.GetComponent<NetworkObject>().NetworkObjectId);
            GameObjectManager.Instance.GameOverText.SetActive(true);

            RoomInfoManager.Instance.ReportPlayerDeath(GameObjectManager.Instance.GetLocalPlayerId());
        }
    }
}