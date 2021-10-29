using UnityEngine;

public static class BaseFunctionValueGenerator
{
    public static FloatPoint BaseFunction(float radius, float angle)
    {
        return new FloatPoint(radius * Mathf.Cos(angle * Mathf.Deg2Rad), radius * Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    public static float GetAngleByRadiusAndDensity(float radius, float density)
    {
        return Mathf.Asin(density / radius) * (180 / Mathf.PI);
    }
}