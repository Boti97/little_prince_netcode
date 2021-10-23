using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EventManager
{
    private static readonly object padlock = new object();
    private static EventManager instance = null;

    public static EventManager Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new EventManager();
                }
                return instance;
            }
        }
    }

    public void SendCharacterPushedEvent(Vector3 pushDirection, Guid characterId, float pushPower)
    {
        //CharacterPushedEvent characterPushedEvent = CharacterPushedEvent.Create();
        //characterPushedEvent.PushDirection = pushDirection;
        //characterPushedEvent.PushedCharacterId = characterId;
        //characterPushedEvent.PushPower = pushPower;
        //characterPushedEvent.Send();
    }

    public void SendPlayerDiedEvent(Guid deadPlayerId)
    {
        //PlayerDiedEvent playerDiedEvent = PlayerDiedEvent.Create();
        //playerDiedEvent.DeadPlayerId = deadPlayerId;
        //playerDiedEvent.Send();
    }

    public void SendGameOverEvent()
    {
        //GameOverEvent.Create().Send();
    }
}