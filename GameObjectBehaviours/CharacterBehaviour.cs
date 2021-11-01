using Unity.Netcode;
using UnityEngine;

public abstract class CharacterBehaviour : NetworkBehaviour
{
    [HideInInspector] public ulong planetId;

    [SerializeField] protected float thrustPower = 50f;
    [SerializeField] protected float sprintSpeed = 30f;
    [SerializeField] protected float walkSpeed = 10f;
    [SerializeField] protected float jumpForce = 2200f;
    [SerializeField] protected LayerMask groundedMask;

    private const float TurnSmoothTime = 8f;
    private Animator animator;
    private CharacterNetworkState characterNetworkState;
    protected Vector3 finalDir;
    protected GravityBody gravityBody;
    [Range(0f, 1f)] protected float health = 1f;
    protected bool isGrounded;
    protected bool isJumpEnabled;
    protected bool isJumping;
    private bool isMoving;

    protected Transform model;
    protected Vector3 moveDir;
    protected float moveSpeed = 8f;
    protected int numberOfJumps = 0;
    protected float stamina = 1f;
    protected float thrust = 1f;

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

    protected void FixedUpdate()
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

            var targetRotation = Quaternion.LookRotation(finalDir, transform.up);
            var rotation = model.rotation;
            rotation = Quaternion.Slerp(rotation, targetRotation,
                TurnSmoothTime * NetworkManager.Singleton.ServerTime.FixedDeltaTime);
            model.rotation = rotation;
            GetComponent<Rigidbody>()
                .MovePosition(GetComponent<Rigidbody>().position +
                              (finalDir * moveSpeed) * NetworkManager.Singleton.ServerTime.FixedDeltaTime);

            characterNetworkState.SetModelRotationServerRpc(rotation);

            isMoving = true;
        }
        else
        {
            SetAnimation("isWalking", 0);

            isMoving = false;
        }
    }

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
        if (Physics.Raycast(ray, out _, 2 + .1f, groundedMask))
        {
            if (!isJumping && !isMoving)
            {
                isGrounded = true;
                isJumpEnabled = true;

                SetAnimation("isGrounded", 1);
                SetAnimation("isJumped", 0);
            }
        }
    }

    private void CheckPlanet()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, 2 + .1f, groundedMask))
        {
            if (hit.collider.gameObject.GetComponentInParent<NetworkObject>().NetworkObjectId != planetId)
            {
                planetId = hit.collider.gameObject.GetComponentInParent<NetworkObject>().NetworkObjectId;
            }
        }
    }
}