using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : NetworkBehaviour
{
    private Dictionary<ulong, KeyValuePair<GravityAttractor, float>> gravityAttractorDictionary;

    private void Start()
    {
        gravityAttractorDictionary = new Dictionary<ulong, KeyValuePair<GravityAttractor, float>>();
    }

    public void FixedUpdate()
    {
        if (!IsOwner) return;

        //CheckGravityAttractors();
        if (gravityAttractorDictionary.Count != 0)
        {
            gravityAttractorDictionary.Select(entry => entry.Value.Key).ToList()
                .ForEach(gravityAttractor => gravityAttractor.Attract(transform.gameObject));
        }
        else
        {
            GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity / 1.02f;
            GetComponent<Rigidbody>().angularVelocity = GetComponent<Rigidbody>().angularVelocity / 1.02f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (!other.gameObject.layer.Equals(LayerMask.NameToLayer("GravityField"))) return;

        var networkObject = other.gameObject.GetComponentInParent<NetworkObject>();
        var gravityAttractor = other.gameObject.GetComponentInParent<GravityAttractor>();
        if (gravityAttractorDictionary.ContainsKey(networkObject.NetworkObjectId))
        {
            //if it's already in the list we only need to set the time to 0
            gravityAttractorDictionary[networkObject.NetworkObjectId] =
                new KeyValuePair<GravityAttractor, float>(gravityAttractor, 0f);
        }
        else
        {
            //if it's not already in the list we  need to add a new entry
            gravityAttractorDictionary.Add(networkObject.NetworkObjectId,
                new KeyValuePair<GravityAttractor, float>(gravityAttractor, 0f));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;

        if (!other.gameObject.layer.Equals(LayerMask.NameToLayer("GravityField"))) return;

        var networkObject = other.gameObject.GetComponentInParent<NetworkObject>();
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("GravityField")))
        {
            gravityAttractorDictionary.Remove(networkObject.NetworkObjectId);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        gravityAttractorDictionary = new Dictionary<ulong, KeyValuePair<GravityAttractor, float>>();
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
    }

    public int AttractorCount()
    {
        return !IsOwner
            ? 0
            : gravityAttractorDictionary.Count;
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