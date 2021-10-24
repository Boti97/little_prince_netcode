using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBehaviour : CharacterBehaviour
{
    private Transform localCamera;
    private bool hitZeroStamina;

    public void AddHealth(float plusHealth)
    {
        if (characterNetworkState.Health + plusHealth < 1f)
        {
            characterNetworkState.SetHealthServerRpc(characterNetworkState.Health + plusHealth);
        }
        else
        {
            characterNetworkState.SetHealthServerRpc(1f);
        }
    }

    protected override void CalculateMovingDirection()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveDir = new Vector3(horizontal, 0f, vertical).normalized;

        Vector3 cameraRelFaceDir = Vector3.ProjectOnPlane(localCamera.forward, transform.up).normalized;
        float anglePlayerForwCameraForw = Vector3.SignedAngle(cameraRelFaceDir, transform.forward, transform.up);
        finalDir = (Quaternion.AngleAxis(-anglePlayerForwCameraForw, transform.up) * transform.TransformDirection(moveDir)).normalized;
    }

    protected override void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || isJumpEnabled))
        {
            Vector3 facingDir = new Vector3();
            //transform.parent = null;
            if (moveDir.Equals(Vector3.zero))
            {
                facingDir = transform.GetChild(0).transform.forward.normalized;
            }
            else
            {
                facingDir = finalDir;
            }
            GetComponent<Rigidbody>().AddForce(Vector3.RotateTowards(facingDir, transform.up, 30 * Mathf.Deg2Rad, 0) * jumpForce);

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
        localCamera = Camera.main.gameObject.transform;

        GameObjectManager.Instance.CinemachineVirtualCamera.LookAt = gameObject.transform;
        GameObjectManager.Instance.CinemachineVirtualCamera.Follow = gameObject.transform;

        //replaces PlayerJoinedEvent
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
        Debug.LogWarning("Player attack is not implemented!");
        //if (Input.GetMouseButtonDown(0))
        //{
        //    GameObjectManager.Instance.Players.ForEach(player =>
        //    {
        //        if (!player.GetComponent<PlayerNetworkState>().entity.IsOwner)
        //        {
        //            if (Vector3.Distance(player.transform.position, transform.position) < 1.5f)
        //            {
        //                EventManager.Instance.SendCharacterPushedEvent(
        //                    transform.Find("Model").forward,
        //                    player.GetComponent<PlayerNetworkState>().GetGuid(),
        //                    attackPower);
        //            }
        //        }
        //    });
        //}
    }

    protected override void CheckHealth()
    {
        if (gravityBody.AttractorCount() == 0)
        {
            characterNetworkState.SetHealthServerRpc(characterNetworkState.Health - 0.002f);
            GameObjectManager.Instance.HealthBar.value = characterNetworkState.Health;
            if (characterNetworkState.Health < 0f)
            {
                //TODO: implement death
                Debug.LogWarning("Player death is not implemented!");
                GameObjectManager.Instance.CinemachineVirtualCamera.gameObject.SetActive(false);
                //EventManager.Instance.SendPlayerDiedEvent(GetGuid());
            }
        }
    }
}