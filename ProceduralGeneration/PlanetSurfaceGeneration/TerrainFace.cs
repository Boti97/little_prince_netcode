using UnityEngine;

public class TerrainFace
{
    private readonly Vector3 axisA;
    private readonly Vector3 axisB;
    private readonly Vector3 localUp;
    private readonly Mesh mesh;
    private readonly int resolution;
    private readonly ShapeGenerator shapeGenerator;

    public TerrainFace(Mesh mesh, Vector3 localUp, int resolution, ShapeGenerator shapeGenerator)
    {
        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
        this.localUp = localUp;
        this.mesh = mesh;
        this.shapeGenerator = shapeGenerator;
        this.resolution = resolution;
    }

    public Mesh ConstructMesh()
    {
        var vertices = new Vector3[resolution * resolution];
        var triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        var triIndex = 0;

        for (var y = 0; y < resolution; y++)
        {
            for (var x = 0; x < resolution; x++)
            {
                var i = x + y * resolution;
                var percent = new Vector2(x, y) / (resolution - 1);
                var point = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                point = point.normalized;
                point = shapeGenerator.GenerateShape(point);
                vertices[i] = point;

                if (x == resolution - 1 || y == resolution - 1) continue;

                triangles[triIndex] = i;
                triangles[triIndex + 1] = i + resolution + 1;
                triangles[triIndex + 2] = i + resolution;

                triangles[triIndex + 3] = i;
                triangles[triIndex + 4] = i + 1;
                triangles[triIndex + 5] = i + resolution + 1;
                triIndex += 6;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}