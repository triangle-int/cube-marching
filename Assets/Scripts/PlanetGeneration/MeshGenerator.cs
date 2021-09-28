using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetGeneration
{
    public class MeshGenerator
    {
        private struct Triangle
        {
            private Vector3 _v1;
            private Vector3 _v2;
            private Vector3 _v3;

            public Vector3 this[int i]
            {
                get
                {
                    switch (i)
                    {
                        case 0: return _v1;
                        case 1: return _v2;
                        case 2: return _v3;
                        default: throw new IndexOutOfRangeException();
                    }
                }
                set
                {
                    switch (i)
                    {
                        case 0: _v1 = value; break;
                        case 1: _v2 = value; break;
                        case 2: _v3 = value; break;
                        default: throw new IndexOutOfRangeException();
                    }
                }
            }
        }

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

        private int CoordsToIndex(int x, int y, int z)
        {
            return x * _cubesNumber * _cubesNumber + y * _cubesNumber + z;
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
                            _chunk.Cubes[CoordsToIndex(x, y, z)],
                            _chunk.Cubes[CoordsToIndex(x + _lodDownscale, y, z)],
                            _chunk.Cubes[CoordsToIndex(x + _lodDownscale, y, z + _lodDownscale)],
                            _chunk.Cubes[CoordsToIndex(x, y, z + _lodDownscale)],
                            _chunk.Cubes[CoordsToIndex(x, y + _lodDownscale, z)],
                            _chunk.Cubes[CoordsToIndex(x + _lodDownscale, y + _lodDownscale, z)],
                            _chunk.Cubes[CoordsToIndex(x + _lodDownscale, y + _lodDownscale, z + _lodDownscale)],
                            _chunk.Cubes[CoordsToIndex(x, y + _lodDownscale, z + _lodDownscale)]
                        };
                        var index = 0;

                        for (var cube = 0; cube < 8; cube++)
                        {
                            if (corners[cube].w >= _threshold)
                                index |= 1 << cube;
                        }

                        for (var tri = 0; tri < 16; tri += 3)
                        {
                            if (MarchingCubesTables.TriangulationTable[index, tri] == -1)
                                break;

                            var triangle = new Triangle();

                            for (var vert = 0; vert < 3; vert++)
                            {
                                var vert1 = corners[
                                    MarchingCubesTables.Corner1Index[
                                        MarchingCubesTables.TriangulationTable[index, tri + vert]]];
                                var vert2 = corners[
                                    MarchingCubesTables.Corner2Index[
                                        MarchingCubesTables.TriangulationTable[index, tri + vert]]];
                                triangle[vert] = GetMiddlePoint(vert1, vert2);
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
    }
}
