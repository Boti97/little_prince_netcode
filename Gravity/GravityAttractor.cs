using UnityEngine;

public abstract class GravityAttractor : MonoBehaviour
{
    [SerializeField] protected float gravityPower = -3000f;
    [SerializeField] private float gravityPowerIndicator = 3;

    private const float AttractTurnSpeed = 0.1f;

    protected float GravityPowerIndicator => gravityPowerIndicator;

    //base method, this needs to be overridden for special gravity behavior
    public void Attract(GameObject body)
    {
        var targetDir = GetGravityDirection(body);
        var rotation = body.transform.rotation;
        var targetRotation = Quaternion.FromToRotation(body.transform.up, targetDir) * rotation;

        rotation = Quaternion.Slerp(rotation, targetRotation, AttractTurnSpeed);
        body.transform.rotation = rotation;

        body.GetComponent<Rigidbody>().AddForce(targetDir * gravityPower * GetGravityPowerIndicator(body));
    }

    //if only the power of the gravity needs to be modified, only this method needs to be overridden
    protected abstract float GetGravityPowerIndicator(GameObject body);

    protected abstract Vector3 GetGravityDirection(GameObject body);
}