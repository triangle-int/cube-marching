using System.Collections.Generic;
using UnityEngine;

public class Triangle
{
    public Vector3 V1, V2, V3;

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        V1 = v1;
        V2 = v2;
        V3 = v3;
    }

    public static Mesh GenerateMesh(List<Triangle> triangles)
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();

        for (var i = 0; i < triangles.Count; i++)
        {
            verts.Add(triangles[i].V1);
            verts.Add(triangles[i].V2);
            verts.Add(triangles[i].V3);
            
            tris.Add(i * 3);
            tris.Add(i * 3 + 1);
            tris.Add(i * 3 + 2);
        }

        var mesh = new Mesh
        {
            vertices = verts.ToArray(),
            triangles = tris.ToArray()
        };

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }
}
