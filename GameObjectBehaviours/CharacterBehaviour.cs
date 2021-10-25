using Unity.Netcode;
using UnityEngine;

public abstract class CharacterBehaviour : NetworkBehaviour
{
    //basic objects
    [HideInInspector] public ulong planetId;
    //private Vector3 previousParentPos;
    //private Vector3 deltaParentPos;

    //variables for thrust
    [SerializeField] protected float thrustPower = 50f;

    //variables for attack
    [SerializeField] protected float attackPower = 1000f;

    //variables for movement
    [SerializeField] protected float sprintSpeed = 30f;
    [SerializeField] protected float walkSpeed = 0f;

    //variables for jump
    [SerializeField] protected float jumpForce = 2200f;
    [SerializeField] protected LayerMask groundedMask;
    private readonly float turnSmoothTime = 8f;
    protected Animator animator;

    //network state
    protected CharacterNetworkState characterNetworkState;
    protected Vector3 finalDir;
    protected GravityBody gravityBody;
    protected bool isGrounded;

    //state indicators
    protected bool isJumpEnabled;
    protected bool isJumping;
    protected bool isMoving;
    private Transform model;
    protected Vector3 moveDir;
    protected float moveSpeed = 8f;
    protected int numberOfJumps = 0;

    protected Transform parent;
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
            GetComponent<Rigidbody>()
                .MovePosition(GetComponent<Rigidbody>().position + (finalDir * moveSpeed) * Time.deltaTime);

            //characterNetworkState.SetCharacterPositionServerRpc(transform.position);
            characterNetworkState.SetModelRotationServerRpc(model.rotation);

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
            if (hit.collider.gameObject.GetComponentInParent<NetworkObject>().NetworkObjectId != planetId)
            {
                planetId = hit.collider.gameObject.GetComponentInParent<NetworkObject>().NetworkObjectId;
            }
        }
    }
}