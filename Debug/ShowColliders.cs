using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowColliders : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 60);
    }
}