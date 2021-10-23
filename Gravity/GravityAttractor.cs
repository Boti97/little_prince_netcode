using UnityEngine;

public abstract class GravityAttractor : MonoBehaviour
{
    [SerializeField]
    protected float gravityPower = -3000f;
    [SerializeField]
    private float gravityPowerIndicator = 3;
    protected float attractTurnSpeed = 0.1f;

    public float GravityPowerIndicator
    {
        get { return gravityPowerIndicator; }
        set { gravityPowerIndicator = value; }
    }

    //base method, this needs to be overridden for special gravity behavior
    public void Attract(GameObject body)
    {
        Vector3 targetDir = GetGravityDirection(body);
        Quaternion targetRotation = Quaternion.FromToRotation(body.transform.up, targetDir) * body.transform.rotation;

        body.transform.rotation = Quaternion.Slerp(body.transform.rotation, targetRotation, attractTurnSpeed);

        body.GetComponent<Rigidbody>().AddForce(targetDir * gravityPower * GetGravityPowerIndicator(body));
    }

    //if only the power of the gravity needs to be modified, only this method needs to be overridden
    protected abstract float GetGravityPowerIndicator(GameObject body);

    protected abstract Vector3 GetGravityDirection(GameObject body);
}