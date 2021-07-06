using System.Collections.Generic;
using UnityEngine;

namespace PlanetGeneration
{
    public static class MeshGenerator
    {
        public static Mesh GenerateMesh(int cubesNumber, float threshold, Vector4[,,] cubes, int lod)
        {
            var cubeSkip = 1 << lod;
            var triangles = new List<Triangle>();

            for (var x = 0; x < cubesNumber - cubeSkip; x += cubeSkip)
            {
                for (var y = 0; y < cubesNumber - cubeSkip; y += cubeSkip)
                {
                    for (var z = 0; z < cubesNumber - cubeSkip; z += cubeSkip)
                        triangles.AddRange(ProcessCube(cubes, threshold, cubeSkip, x, y, z));
                }
            }

            var meshVerts = new List<Vector3>();
            var meshTris = new List<int>();

            for (var triIndex = 0; triIndex < triangles.Count; triIndex++)
            {
                var triangle = triangles[triIndex];

                for (var vertIndex = 0; vertIndex < 3; vertIndex++)
                {
                    meshVerts.Add(triangle[vertIndex]);
                    meshTris.Add(triIndex * 3 + vertIndex);
                }
            }

            var mesh = new Mesh
            {
                vertices = meshVerts.ToArray(),
                triangles = meshTris.ToArray()
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mesh;
        }
        
        private static List<Triangle> ProcessCube(Vector4[,,] cubes, float threshold, int skip, int x, int y, int z)
        {
            var corners = new[]
            {
                cubes[x, y, z],
                cubes[x + skip, y, z],
                cubes[x + skip, y, z + skip],
                cubes[x, y, z + skip],
                cubes[x, y + skip, z],
                cubes[x + skip, y + skip, z],
                cubes[x + skip, y + skip, z + skip],
                cubes[x, y + skip, z + skip]
            };

            var triangles = new List<Triangle>();
            var cubeIndex = 0;

            for (var i = 0; i < corners.Length; i++)
            {
                if (corners[i].w >= threshold)
                    cubeIndex |= 1 << i;
            }

            for (var triIndex = 0; triIndex < MarchingCubesTables.TriangulationTable.GetLength(1); triIndex += 3)
            {
                if (MarchingCubesTables.TriangulationTable[cubeIndex, triIndex] == -1)
                    break;

                var verts = new List<Vector3>();

                for (var vertIndex = 0; vertIndex < 3; vertIndex++)
                {
                    var v1 = corners[MarchingCubesTables.Corner1Index[MarchingCubesTables.TriangulationTable[cubeIndex, triIndex + vertIndex]]];
                    var v2 = corners[MarchingCubesTables.Corner2Index[MarchingCubesTables.TriangulationTable[cubeIndex, triIndex + vertIndex]]];
                    verts.Add(GetMiddlePoint(v1, v2, threshold));
                }

                triangles.Add(new Triangle(verts[0], verts[1], verts[2]));
            }

            return triangles;
        }
        
        private static Vector3 GetMiddlePoint(Vector4 v1, Vector4 v2, float threshold)
        {
            return Vector3.Lerp(v1, v2, (threshold - v1.w) / (v2.w - v1.w));
        }
    }
}
