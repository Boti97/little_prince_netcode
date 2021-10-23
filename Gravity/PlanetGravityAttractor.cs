using UnityEngine;

public class PlanetGravityAttractor : GravityAttractor
{
    [SerializeField]
    private LayerMask groundedMask;

    protected override float GetGravityPowerIndicator(GameObject body)
    {
        float distance = Vector3.Distance(transform.position, body.transform.position);

        return GravityPowerIndicator / distance;
    }

    protected override Vector3 GetGravityDirection(GameObject body)
    {
        Ray ray = new Ray(body.transform.position, -body.transform.up);

        if (Physics.Raycast(ray, out RaycastHit hit, 1 + .1f, groundedMask))
        {
            return hit.normal;
        }
        else
        {
            return (body.transform.position - transform.position).normalized;
        }
    }
}