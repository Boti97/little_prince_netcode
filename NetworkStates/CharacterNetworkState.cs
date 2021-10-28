using Unity.Netcode;
using UnityEngine;

public class CharacterNetworkState : NetworkBehaviour
{
    private const string isGroundedString = "isGrounded";
    private const string isWalkingString = "isWalking";
    private const string isJumpedString = "isJumped";

    private Animator animator;
    private NetworkVariable<int> isGrounded = new NetworkVariable<int>();
    private NetworkVariable<int> isJumped = new NetworkVariable<int>();
    private NetworkVariable<int> isWalking = new NetworkVariable<int>();

    private Transform model;
    private NetworkVariable<Quaternion> modelRotation = new NetworkVariable<Quaternion>();

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

    //data format is questionable, maybe byte
    [ServerRpc]
    public void SetModelRotationServerRpc(Quaternion rotation)
    {
        //TODO: implement checks

        modelRotation.Value = rotation;
    }

    [ServerRpc]
    public void SetAnimationServerRpc(string nameOfAnimation, int value)
    {
        //TODO: implement checks

        switch (nameOfAnimation)
        {
            case isGroundedString:
                isGrounded.Value = value;
                break;
            case isWalkingString:
                isWalking.Value = value;
                break;
            case isJumpedString:
                isJumped.Value = value;
                break;
        }

        ;
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

        animator.SetInteger(isGroundedString, newValue);
    }

    private void OnIsWalkingChanged(int oldValue, int newValue)
    {
        if (!IsClient)
        {
            return;
        }

        animator.SetInteger(isWalkingString, newValue);
    }

    private void OnIsJumpedChanged(int oldValue, int newValue)
    {
        if (!IsClient)
        {
            return;
        }

        animator.SetInteger(isJumpedString, newValue);
    }
}