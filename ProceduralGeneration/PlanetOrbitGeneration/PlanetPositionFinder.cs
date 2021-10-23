using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetPositionFinder
{
    public static List<Vector3> FindPlanetPositions(LineRenderer functionView, float numberOfPointsInFunction, float planetDensity)
    {
        List<Vector3> planetPositions = new List<Vector3>();

        Vector3 currentPos = functionView.GetPosition(0);

        Vector3 lastPosition = Vector3.positiveInfinity;
        int indexOfClosestPoint = 0;

        planetPositions.Add(currentPos);
        for (int l = 0; l < numberOfPointsInFunction; l++)
        {
            Vector3 closestPoint = Vector3.positiveInfinity;
            float closestDistance = float.PositiveInfinity;

            for (int i = indexOfClosestPoint; i < numberOfPointsInFunction; i++)
            {
                Vector3 nextPoint = functionView.GetPosition(i);
                float distance = GetDistance(planetDensity, currentPos, nextPoint);
                if (distance > planetDensity * 3) break;
                if (distance < closestDistance)
                {
                    if (IsNewPoint(planetPositions, nextPoint, planetDensity, numberOfPointsInFunction))
                    {
                        closestDistance = distance;
                        closestPoint = nextPoint;
                        indexOfClosestPoint = i;
                    }
                }
            }

            if (!closestPoint.Equals(Vector3.positiveInfinity) && !closestPoint.Equals(lastPosition) && !currentPos.Equals(lastPosition))
            {
                planetPositions.Add(closestPoint);
                lastPosition = currentPos;
                currentPos = closestPoint;
            }
            else break;
        }

        return planetPositions;
    }

    private static bool IsNewPoint(List<Vector3> planetPositions, Vector3 nextPoint, float planetDensity, float numberOfPointsInFunction)
    {
        foreach (Vector3 vector in planetPositions)
        {
            //(circleRadius / (numberOfPointsInFunction / 4)) is about one point to point length in the line renderer
            //four time this length should be enought (+/-)
            if (Vector3.Distance(nextPoint, vector) < (float)(planetDensity - 4 * (planetDensity / (numberOfPointsInFunction / 4)))
                || Vector3.Distance(nextPoint, vector) < (float)(planetDensity + 4 * (planetDensity / (numberOfPointsInFunction / 4))))
            {
                return false;
            }
        }
        return true;
    }

    private static float GetDistance(float radius, Vector3 circleCenter, Vector3 point)
    {
        return Mathf.Abs(Mathf.Sqrt(((point.x - circleCenter.x) * (point.x - circleCenter.x)) + ((point.y - circleCenter.y) * (point.y - circleCenter.y))) - radius);
    }
}