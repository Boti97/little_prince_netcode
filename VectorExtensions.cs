using UnityEngine;

public static class VectorExtensions
{
    public static Vector3 RandomVector(Vector3 min, Vector3 max)
    {
        return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y),
            Random.Range(min.z, max.z));
    }

    public static Vector3 RandomAxis()
    {
        var random = Random.Range(0f, 3f);
        if (random < 1)
        {
            return Vector3.up;
        }
        else if (random > 1 && random < 3)
        {
            return Vector3.right;
        }
        else
        {
            return Vector3.forward;
        }
    }
}