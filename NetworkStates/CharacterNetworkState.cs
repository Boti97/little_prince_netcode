using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class CharacterNetworkState : NetworkBehaviour
{
    private NetworkVariable<Quaternion> modelRotation = new NetworkVariable<Quaternion>();
    private NetworkVariable<Vector3> characterPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<float> health = new NetworkVariable<float>(1f);
    private NetworkVariable<int> isGrounded = new NetworkVariable<int>();
    private NetworkVariable<int> isWalking = new NetworkVariable<int>();
    private NetworkVariable<int> isJumped = new NetworkVariable<int>();

    private Transform model;
    private Animator animator;

    private const string isGroundedString = "isGrounded";
    private const string isWalkingString = "isWalking";
    private const string isJumpedString = "isJumped";

    public float Health { get => health.Value; }

    public override void OnNetworkSpawn()
    {
        if (!IsClient) { return; }

        animator = GetComponentInChildren<Animator>();
        model = transform.Find("Model");

        modelRotation.OnValueChanged += OnModelRotationChanged;
        characterPosition.OnValueChanged += OnCharacterPositionChanged;
        health.OnValueChanged += OnHealthChanged;
        isGrounded.OnValueChanged += OnIsGroundedChanged;
        isWalking.OnValueChanged += OnIsWalkingChanged;
        isJumped.OnValueChanged += OnIsJumpedChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient) { return; }

        modelRotation.OnValueChanged -= OnModelRotationChanged;
        characterPosition.OnValueChanged -= OnCharacterPositionChanged;
        health.OnValueChanged -= OnHealthChanged;
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
    public void SetCharacterPositionServerRpc(Vector3 position)
    {
        //TODO: implement checks

        characterPosition.Value = position;
    }

    [ServerRpc]
    public void SetHealthServerRpc(float health)
    {
        //TODO: implement checks

        this.health.Value = health;
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
        };
    }

    private void OnCharacterPositionChanged(Vector3 oldCharacterPosition, Vector3 newCharacterPosition)
    {
        if (!IsClient) { return; }

        transform.position = newCharacterPosition;
    }

    private void OnModelRotationChanged(Quaternion oldModelRotation, Quaternion newModelRotation)
    {
        if (!IsClient) { return; }

        model.rotation = newModelRotation;
    }

    private void OnHealthChanged(float oldHeatlth, float newHealth)
    {
        if (!IsClient) { return; }

        GameObjectManager.Instance.HealthBar.value = newHealth;
    }

    private void OnIsGroundedChanged(int oldValue, int newValue)
    {
        if (!IsClient) { return; }

        animator.SetInteger(isGroundedString, newValue);
    }

    private void OnIsWalkingChanged(int oldValue, int newValue)
    {
        if (!IsClient) { return; }

        animator.SetInteger(isWalkingString, newValue);
    }

    private void OnIsJumpedChanged(int oldValue, int newValue)
    {
        if (!IsClient) { return; }

        animator.SetInteger(isJumpedString, newValue);
    }
}
