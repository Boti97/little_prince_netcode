using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;

public class ServerEventListener : NetworkBehaviour
{
    ////if a player dies and no more than one player remains, the game is over
    //public override void OnEvent(PlayerDiedEvent evnt)
    //{
    //    GameObjectManager.Instance.RemovePlayer(evnt.DeadPlayerId);

    //    if (BoltNetwork.IsServer)
    //    {
    //        if (GameObjectManager.Instance.Players.Count <= 1)
    //            EventManager.Instance.SendGameOverEvent();
    //    }
    //}
}