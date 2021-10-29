using UnityEngine;

public class PlanetGravityAttractor : GravityAttractor
{
    [SerializeField] private LayerMask groundedMask;

    protected override float GetGravityPowerIndicator(GameObject body)
    {
        var distance = Vector3.Distance(transform.position, body.transform.position);

        return GravityPowerIndicator / distance;
    }

    protected override Vector3 GetGravityDirection(GameObject body)
    {
        var position = body.transform.position;
        var ray = new Ray(position, -body.transform.up);

        return Physics.Raycast(ray, out var hit, 1 + .1f, groundedMask)
            ? hit.normal
            : (position - transform.position).normalized;
    }
}