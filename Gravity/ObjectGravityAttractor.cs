using UnityEngine;

public class ObjectGravityAttractor : GravityAttractor
{
    protected override Vector3 GetGravityDirection(GameObject body)
    {
        throw new System.NotImplementedException();
    }

    protected override float GetGravityPowerIndicator(GameObject body)
    {
        throw new System.NotImplementedException();
    }
}