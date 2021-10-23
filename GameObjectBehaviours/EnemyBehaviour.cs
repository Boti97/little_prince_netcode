using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : CharacterBehaviour
{
    [SerializeField]
    private float pushPower;
    [SerializeField]
    private float pushPowerAmplifier;
    private int numberOfPushes = 0;
    private GameObject playerToFollow;

    protected override void CalculateMovingDirection()
    {
        //if we did not choose a player to follow yet, or the player we followed jumped to another planet as us, we choose one
        if ((playerToFollow == null && GameObjectManager.Instance.Players.Count > 0) || (!IsPlayerOnSamePlanet(playerToFollow) && GameObjectManager.Instance.Players.Count > 1))
        {
            //find the players whos are on the same planet
            List<GameObject> followablePlayers = new List<GameObject>();
            foreach (GameObject player in GameObjectManager.Instance.Players)
            {
                if (IsPlayerOnSamePlanet(player))
                {
                    followablePlayers.Add(player);
                }
            }

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
    }

    protected override void InitializeCharacterSpecificFields()
    {
        return;
    }

    protected override void HandleSprint()
    {
        //TODO: implement enemy sprint
        moveSpeed = walkSpeed;
        return;
    }

    protected override void HandleJump()
    {
        //TODO: implement jumping
        return;
    }

    protected override void HandleThrust()
    {
        //TODO: implement thrusting
        return;
    }

    protected override void HandleAttack()
    {
        //if we have a player to follow, who's on the same planet as us, follow it
        if (playerToFollow != null && IsPlayerOnSamePlanet(playerToFollow))
        {
            //if we close enough we push them into space
            if (Vector3.Distance(playerToFollow.transform.position, transform.position) < 1.5f)
            {
                //TODO: implement character pushed event
                Debug.LogError("SendCharacterPushedEvent is not implemented!");
                //EventManager.Instance.SendCharacterPushedEvent(
                //        transform.Find("Model").forward,
                //        playerToFollow.GetComponent<CharacterNetworkState>().NetworkObjectId,
                //        pushPower + (numberOfPushes * pushPowerAmplifier));

                numberOfPushes++;
                moveDir = Vector3.zero;
            }
            else
            {
                finalDir = Vector3.ProjectOnPlane((playerToFollow.transform.position - transform.position).normalized, transform.up).normalized;
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
        if (gravityBody.AttractorCount() == 0)
        {
            characterNetworkState.SetHealthServerRpc(characterNetworkState.Health - 0.002f);
            if (characterNetworkState.Health < 0f)
            {
                //TODO: implement death
                Debug.LogError("Enemy death is not implemented!");
            }
        }
    }
}