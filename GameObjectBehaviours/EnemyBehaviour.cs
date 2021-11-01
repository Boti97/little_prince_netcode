using System.Linq;
using UnityEngine;

public class EnemyBehaviour : CharacterBehaviour
{
    [SerializeField] private float pushPower;
    [SerializeField] private float pushPowerAmplifier;
    private bool alreadyPushedPlayer;
    private int numberOfPushes = 0;
    private GameObject playerToFollow;

    private void OnTriggerEnter(Collider player)
    {
        if (alreadyPushedPlayer || !player.gameObject.tag.Equals("Player")) return;

        player.GetComponent<PlayerBehaviour>().PushPlayerClientRpc(model.forward, pushPower * pushPowerAmplifier);
        pushPowerAmplifier += 0.2f;
        alreadyPushedPlayer = true;
    }

    private void OnTriggerExit(Collider player)
    {
        if (!player.gameObject.tag.Equals("Player")) return;

        alreadyPushedPlayer = false;
    }

    /// <summary>
    /// The enemy's moving direction is always towards a player so in this method we choose player.
    /// </summary>
    protected override void CalculateMovingDirection()
    {
        //if we did not choose a player to follow yet, or the player we followed jumped to another planet as us, we choose one
        if ((playerToFollow != null || GameObjectManager.Instance.Players.Count <= 0) &&
            (IsPlayerOnSamePlanet(playerToFollow) || GameObjectManager.Instance.Players.Count <= 1)) return;

        //find the players who are on the same planet
        var followablePlayers = GameObjectManager.Instance.Players.Where(IsPlayerOnSamePlanet).ToList();

        //choose a random player to follow
        if (followablePlayers.Count > 1)
        {
            playerToFollow = followablePlayers[Random.Range(0, followablePlayers.Count) - 1];
        }
        else if (followablePlayers.Count > 0)
        {
            playerToFollow = followablePlayers[0];
        }
    }

    protected override void InitializeCharacterSpecificFields()
    {
    }

    protected override void HandleSprint()
    {
        //TODO: implement enemy sprint
        moveSpeed = walkSpeed;
    }

    protected override void HandleJump()
    {
        //TODO: implement jumping
    }

    protected override void HandleThrust()
    {
        //TODO: implement thrusting
    }

    protected override void HandleAttack()
    {
        //if we have a player to follow, who's on the same planet as us, follow it
        if (playerToFollow != null && IsPlayerOnSamePlanet(playerToFollow))
        {
            //if we close enough we push them into space
            if (Vector3.Distance(playerToFollow.transform.position, transform.position) < 2f)
            {
                moveDir = Vector3.zero;
            }
            else
            {
                finalDir = Vector3.ProjectOnPlane(
                    (playerToFollow.transform.position - transform.position).normalized,
                    transform.up).normalized;
                moveDir = Vector3.forward;
            }
        }
        else
        {
            moveDir = Vector3.zero;
        }
    }


    private bool IsPlayerOnSamePlanet(GameObject player)
    {
        if (player != null)
        {
            return player.GetComponent<PlayerBehaviour>().planetId == planetId;
        }

        return false;
    }

    protected override void CheckHealth()
    {
        if (!IsOwner) return;

        if (gravityBody.AttractorCount() != 0) return;

        health -= 0.002f;
        if (health < 0f)
        {
            Debug.LogWarning("Enemy died!");
            GameObjectManager.Instance.CreateHeadstoneOnPositions(transform.position);
            Destroy(gameObject);
        }
    }
}