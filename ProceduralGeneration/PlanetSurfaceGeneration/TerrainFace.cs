
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace
{
    //other variables
    private Vector3 axisA;
    private Vector3 axisB;
    private Vector3 localUp;
    private Mesh mesh;
    private ShapeGenerator shapeGenerator;
    private int resolution;

    public TerrainFace(Mesh mesh, Vector3 localUp, int resolution, ShapeGenerator shapeGenerator)
    {

        this.axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        this.axisB = Vector3.Cross(localUp, axisA);
        this.localUp = localUp;
        this.mesh = mesh;
        this.shapeGenerator = shapeGenerator;
        this.resolution = resolution;
    }


    public Mesh ConstructMesh()
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 point = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                point = point.normalized;
                point = shapeGenerator.GenerateShape(point);
                vertices[i] = point;

                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}