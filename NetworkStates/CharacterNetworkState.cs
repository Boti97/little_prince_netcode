using Unity.Netcode;
using UnityEngine;

public class CharacterNetworkState : NetworkBehaviour
{
    private const string IsGroundedString = "isGrounded";
    private const string IsWalkingString = "isWalking";
    private const string IsJumpedString = "isJumped";
    private static readonly int IsGrounded = Animator.StringToHash(IsGroundedString);
    private static readonly int IsWalking = Animator.StringToHash(IsWalkingString);
    private static readonly int IsJumped = Animator.StringToHash(IsJumpedString);
    private readonly NetworkVariable<int> isGrounded = new NetworkVariable<int>();
    private readonly NetworkVariable<int> isJumped = new NetworkVariable<int>();
    private readonly NetworkVariable<int> isWalking = new NetworkVariable<int>();
    private readonly NetworkVariable<Quaternion> modelRotation = new NetworkVariable<Quaternion>();


    private Animator animator;

    private Transform model;

    public override void OnNetworkSpawn()
    {
        if (!IsClient)
        {
            return;
        }

        animator = GetComponentInChildren<Animator>();
        model = transform.Find("Model");

        modelRotation.OnValueChanged += OnModelRotationChanged;
        isGrounded.OnValueChanged += OnIsGroundedChanged;
        isWalking.OnValueChanged += OnIsWalkingChanged;
        isJumped.OnValueChanged += OnIsJumpedChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient)
        {
            return;
        }

        modelRotation.OnValueChanged -= OnModelRotationChanged;
        isGrounded.OnValueChanged -= OnIsGroundedChanged;
        isWalking.OnValueChanged -= OnIsWalkingChanged;
        isJumped.OnValueChanged -= OnIsJumpedChanged;
    }

    [ServerRpc]
    public void SetModelRotationServerRpc(Quaternion rotation)
    {
        var changeAngle = Quaternion.Angle(modelRotation.Value, rotation);
        if (changeAngle > 50 && !modelRotation.Value.eulerAngles.Equals(Quaternion.identity.eulerAngles))
        {
            Debug.LogWarning(
                "Rotation change dismissed, since change angle is too big: "
                + changeAngle
                + " degrees.");
            return;
        }

        modelRotation.Value = rotation;
    }

    [ServerRpc]
    public void SetAnimationServerRpc(string nameOfAnimation, int value)
    {
        if (value != 0 && value != 1)
        {
            Debug.LogWarning("Animation set request dismissed, since value is invalid. Value: " + value);
            return;
        }

        switch (nameOfAnimation)
        {
            case IsGroundedString:
                isGrounded.Value = value;
                break;
            case IsWalkingString:
                isWalking.Value = value;
                break;
            case IsJumpedString:
                isJumped.Value = value;
                break;
            default:
                Debug.LogWarning("Unknown animation set requested. Name: " + nameOfAnimation);
                break;
        }
    }

    private void OnModelRotationChanged(Quaternion oldModelRotation, Quaternion newModelRotation)
    {
        if (!IsClient)
        {
            return;
        }

        model.rotation = newModelRotation;
    }

    private void OnIsGroundedChanged(int oldValue, int newValue)
    {
        if (!IsClient)
        {
            return;
        }

        animator.SetInteger(IsGrounded, newValue);
    }

    private void OnIsWalkingChanged(int oldValue, int newValue)
    {
        if (!IsClient)
        {
            return;
        }

        animator.SetInteger(IsWalking, newValue);
    }

    private void OnIsJumpedChanged(int oldValue, int newValue)
    {
        if (!IsClient)
        {
            return;
        }

        animator.SetInteger(IsJumped, newValue);
    }
}