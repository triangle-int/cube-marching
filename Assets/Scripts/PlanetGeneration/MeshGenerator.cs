using System.Collections.Generic;
using UnityEngine;

namespace PlanetGeneration
{
    public class MeshGenerator
    {
        private readonly Chunk _chunk;
        private readonly int _cubesNumber;
        private readonly float _threshold;
        private readonly int _lodDownscale;

        public MeshGenerator(Chunk chunk, int cubesNumber, float threshold, int lod)
        {
            _chunk = chunk;
            _cubesNumber = cubesNumber;
            _threshold = threshold;
            _lodDownscale = 1 << lod;
        }

        private Vector3 GetMiddlePoint(Vector4 v1, Vector4 v2)
        {
            return Vector3.Lerp(v1, v2, (_threshold - v1.w) / (v2.w - v1.w));
        }
        
        public Mesh GenerateMesh()
        {
            var triangles = new List<Triangle>();

            for (var x = 0; x < _cubesNumber - _lodDownscale; x += _lodDownscale)
            {
                for (var y = 0; y < _cubesNumber - _lodDownscale; y += _lodDownscale)
                {
                    for (var z = 0; z < _cubesNumber - _lodDownscale; z += _lodDownscale)
                    {
                        var corners = new [] {
                            _chunk.Cubes[x, y, z],
                            _chunk.Cubes[x + _lodDownscale, y, z],
                            _chunk.Cubes[x + _lodDownscale, y, z + _lodDownscale],
                            _chunk.Cubes[x, y, z + _lodDownscale],
                            _chunk.Cubes[x, y + _lodDownscale, z],
                            _chunk.Cubes[x + _lodDownscale, y + _lodDownscale, z],
                            _chunk.Cubes[x + _lodDownscale, y + _lodDownscale, z + _lodDownscale],
                            _chunk.Cubes[x, y + _lodDownscale, z + _lodDownscale]
                        };
                        var cubeIndex = 0;

                        for (var cornerIndex = 0; cornerIndex < 8; cornerIndex++)
                        {
                            if (corners[cornerIndex].w >= _threshold)
                                cubeIndex |= 1 << cornerIndex;
                        }

                        for (var triIndex = 0; triIndex < 16; triIndex += 3)
                        {
                            if (MarchingCubesTables.TriangulationTable[cubeIndex, triIndex] == -1)
                                break;

                            var triangle = new Triangle();

                            for (var vertIndex = 0; vertIndex < 3; vertIndex++)
                            {
                                var vert1 = corners[
                                    MarchingCubesTables.Corner1Index[
                                        MarchingCubesTables.TriangulationTable[cubeIndex, triIndex + vertIndex]]];
                                var vert2 = corners[
                                    MarchingCubesTables.Corner2Index[
                                        MarchingCubesTables.TriangulationTable[cubeIndex, triIndex + vertIndex]]];
                                triangle[vertIndex] = GetMiddlePoint(vert1, vert2);
                            }

                            triangles.Add(triangle);
                        }
                    }
                }
            }

            return TrianglesToMesh(triangles);
        }
        
        private Mesh TrianglesToMesh(List<Triangle> triangles)
        {
            var vertsDict = new Dictionary<Vector3, int>();
            var meshVerts = new List<Vector3>();
            var meshTris = new List<int>();

            foreach (var triangle in triangles)
            {
                for (var vertIndex = 0; vertIndex < 3; vertIndex++)
                {
                    if (!vertsDict.ContainsKey(triangle[vertIndex]))
                    {
                        vertsDict[triangle[vertIndex]] = meshVerts.Count;
                        meshVerts.Add(triangle[vertIndex]);
                    }
                    
                    meshTris.Add(vertsDict[triangle[vertIndex]]);
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
    }
}
