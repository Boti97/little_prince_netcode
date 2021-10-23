using Unity.Netcode;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : NetworkBehaviour
{
    private Dictionary<ulong, KeyValuePair<GravityAttractor, float>> gravityAttractorDictionary;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        gravityAttractorDictionary = new Dictionary<ulong, KeyValuePair<GravityAttractor, float>>();
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void FixedUpdate()
    {
        if (!IsOwner) return;

        //CheckGravityAttractors();
        if (gravityAttractorDictionary.Count != 0)
        {
            gravityAttractorDictionary.Select(entry => entry.Value.Key).ToList().ForEach(gravityAttractor => gravityAttractor.Attract(transform.gameObject));
        }
        else
        {
            GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity / 1.02f;
            GetComponent<Rigidbody>().angularVelocity = GetComponent<Rigidbody>().angularVelocity / 1.02f;
        }
    }

    public int AttractorCount()
    {
        if (!IsOwner) return 0;
        return gravityAttractorDictionary.Count;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("GravityField")))
        {
            PlanetNetworkState planetNetworkState = other.gameObject.GetComponentInParent<PlanetNetworkState>();
            GravityAttractor gravityAttractor = other.gameObject.GetComponentInParent<GravityAttractor>();
            if (gravityAttractorDictionary.ContainsKey(planetNetworkState.NetworkObjectId))
            {
                //if it's already in the list we only need to set the time to 0
                gravityAttractorDictionary[planetNetworkState.NetworkObjectId] = new KeyValuePair<GravityAttractor, float>(gravityAttractor, 0f);
            }
            else
            {
                //if it's not already in the list we  need to add a new entry
                gravityAttractorDictionary.Add(planetNetworkState.NetworkObjectId, new KeyValuePair<GravityAttractor, float>(gravityAttractor, 0f));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;

        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("GravityField")))
        {
            PlanetNetworkState planetNetworkState = other.gameObject.GetComponentInParent<PlanetNetworkState>();
            if (other.gameObject.layer.Equals(LayerMask.NameToLayer("GravityField")))
            {
                gravityAttractorDictionary.Remove(planetNetworkState.NetworkObjectId);
            }
        }
    }

    //checks if one or more attractors are out of our object's scope (it left them a while ago) and removes them
    private void CheckGravityAttractors()
    {
        //List<Guid> attractorsToRemove = gravityAttractorDictionary
        //    .Where(gA => gA.Value.Value != 0f)
        //    .Where(gA => Time.realtimeSinceStartup - gA.Value.Value > 1)
        //    .Select(gA => gA.Key).ToList();
        //attractorsToRemove.ForEach(guid => gravityAttractorDictionary.Remove(guid));
    }
}