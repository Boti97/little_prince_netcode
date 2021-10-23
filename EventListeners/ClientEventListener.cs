using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientEventListener
{
    //private float lastHitTime = 0f;

    //public override void OnEvent(GameOverEvent evnt)
    //{
    //    GameObjectManager.Instance.CinemachineVirtualCamera.gameObject.SetActive(false);
    //    GameObjectManager.Instance.GameOverText.SetActive(true);
    //}

    //public override void OnEvent(CharacterPushedEvent evnt)
    //{
    //    if (GameObjectManager.Instance.IsPlayerOwned(evnt.PushedCharacterId))
    //    {
    //        if (BoltNetwork.Time - lastHitTime > 1)
    //        {
    //            Debug.Log(evnt.PushedCharacterId);
    //            GameObject pushedPlayer = GameObjectManager.Instance.GetPlayerById(evnt.PushedCharacterId);
    //            pushedPlayer
    //                .GetComponent<Rigidbody>()
    //                .AddForce(evnt.PushDirection * evnt.PushPower);

    //            lastHitTime = BoltNetwork.Time;
    //        }
    //    }
    //}
}