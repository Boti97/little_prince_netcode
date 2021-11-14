using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlanetPositionFinder
{
    public static List<Vector3> FindPlanetPositions(LineRenderer functionView, float numberOfPointsInFunction,
        float planetDensity)
    {
        var planetPositions = new List<Vector3>();
        var currentPos = functionView.GetPosition(0);
        var lastPosition = Vector3.positiveInfinity;
        var indexOfClosestPoint = 0;

        planetPositions.Add(currentPos);
        for (var l = 0; l < numberOfPointsInFunction; l++)
        {
            var closestPoint = Vector3.positiveInfinity;
            var closestDistance = float.PositiveInfinity;

            for (var i = indexOfClosestPoint; i < numberOfPointsInFunction; i++)
            {
                var nextPoint = functionView.GetPosition(i);
                var distance = GetDistance(planetDensity, currentPos, nextPoint);
                if (distance > planetDensity * 3) break;

                if (!(distance < closestDistance) ||
                    !IsNewPoint(planetPositions, nextPoint, planetDensity, numberOfPointsInFunction)) continue;

                closestDistance = distance;
                closestPoint = nextPoint;
                indexOfClosestPoint = i;
            }

            if (!closestPoint.Equals(Vector3.positiveInfinity) && !closestPoint.Equals(lastPosition) &&
                !currentPos.Equals(lastPosition))
            {
                planetPositions.Add(closestPoint);
                lastPosition = currentPos;
                currentPos = closestPoint;
            }
            else break;
        }

        return planetPositions;
    }

    private static bool IsNewPoint(IEnumerable<Vector3> planetPositions, Vector3 nextPoint, float planetDensity,
        float numberOfPointsInFunction)
    {
        return planetPositions.All(vector =>
            !(Vector3.Distance(nextPoint, vector) <
              planetDensity - 4 * (planetDensity / (numberOfPointsInFunction / 4))) &&
            !(Vector3.Distance(nextPoint, vector) <
              planetDensity + 4 * (planetDensity / (numberOfPointsInFunction / 4))));
    }

    private static float GetDistance(float radius, Vector3 circleCenter, Vector3 point)
    {
        return Mathf.Abs(Mathf.Sqrt((point.x - circleCenter.x) * (point.x - circleCenter.x) +
                                    (point.z - circleCenter.z) * (point.z - circleCenter.z)) - radius);
    }
}