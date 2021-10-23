﻿using Unity.Netcode;
using System;
using System.Collections;
using UnityEngine;

public abstract class CharacterBehaviour : NetworkBehaviour
{
    //basic objects
    [HideInInspector]
    public ulong planetId;
    protected Animator animator;
    private Transform model;
    protected GravityBody gravityBody;
    protected Transform parent;
    //private Vector3 previousParentPos;
    //private Vector3 deltaParentPos;

    //variables for thrust
    [SerializeField]
    protected float thrustPower = 50f;
    protected float thrust = 1f;

    //variables for attack
    [SerializeField]
    protected float attackPower = 1000f;

    //variables for movement
    [SerializeField]
    protected float sprintSpeed = 30f;
    [SerializeField]
    protected float walkSpeed = 0f;
    protected float stamina = 1f;
    protected float moveSpeed = 8f;
    protected Vector3 moveDir;
    protected Vector3 finalDir;
    private readonly float turnSmoothTime = 8f;
    protected bool isMoving;

    //variables for jump
    [SerializeField]
    protected float jumpForce = 2200f;
    [SerializeField]
    protected LayerMask groundedMask;
    protected int numberOfJumps = 0;

    //state indicators
    protected bool isJumpEnabled;
    protected bool isGrounded;
    protected bool isJumping;

    //network state
    protected CharacterNetworkState characterNetworkState;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        animator = GetComponentInChildren<Animator>();
        gravityBody = GetComponent<GravityBody>();
        model = transform.Find("Model");

        characterNetworkState = transform.GetComponent<CharacterNetworkState>();

        InitializeCharacterSpecificFields();

        SetAnimation("isWalking", 0);
    }

    protected void Update()
    {
        CheckPlanet();

        if (!IsOwner) return;

        CalculateMovingDirection();
        CheckOnGround();
        CheckHealth();

        HandleJump();
        HandleSprint();
        HandleThrust();
        HandleAttack();
    }

    protected void LateUpdate()
    {
        if (!IsOwner) return;

        /* move only when:
         * - character is grounded and
         * - user said so and
         * - there are at least one attractor
         */
        if (isGrounded && moveDir.magnitude >= .1f && gravityBody.AttractorCount() > 0)
        {
            if (isGrounded)
            {
                SetAnimation("isWalking", 1);
            }

            //deltaParentPos = parent.position - previousParentPos;
            //previousParentPos = parent.position;
            //transform.parent = null;


            //TODO: might be needed a network delta time
            Quaternion targetRotation = Quaternion.LookRotation(finalDir, transform.up);
            model.rotation = Quaternion.Slerp(model.rotation, targetRotation, turnSmoothTime * Time.deltaTime);
            GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + (finalDir * moveSpeed) * Time.deltaTime);

            characterNetworkState.SetCharacterPositionServerRpc(transform.position);
            characterNetworkState.SetModelRotationServerRpc(model.rotation);

            isMoving = true;
        }
        else
        {
            SetAnimation("isWalking", 0);

            isMoving = false;
        }
    }

    protected abstract void CalculateMovingDirection();

    protected abstract void HandleJump();

    protected abstract void HandleSprint();

    protected abstract void HandleThrust();

    protected abstract void HandleAttack();

    protected abstract void CheckHealth();

    protected abstract void InitializeCharacterSpecificFields();

    protected void SetAnimation(string nameOfAnimation, int value)
    {
        if (!IsOwner) return;

        animator.SetInteger(nameOfAnimation, value);
        characterNetworkState.SetAnimationServerRpc(nameOfAnimation, value);
    }

    private void CheckOnGround()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, 2 + .1f, groundedMask))
        {
            if (!isJumping && !isMoving)
            {
                isGrounded = true;
                isJumpEnabled = true;

                SetAnimation("isGrounded", 1);
                SetAnimation("isJumped", 0);

                //parent = hit.transform;
                //transform.parent = parent;
            }
        }
    }

    private void CheckPlanet()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, 2 + .1f, groundedMask))
        {
            if (hit.collider.gameObject.GetComponentInParent<PlanetNetworkState>().NetworkObjectId != planetId)
            {
                planetId = hit.collider.gameObject.GetComponentInParent<PlanetNetworkState>().NetworkObjectId;
            }
        }
    }
}