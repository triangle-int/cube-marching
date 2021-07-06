using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetGeneration
{
    public class MeshGenerator
    {
        private struct Triangle
        {
            public Vector3 V1;
            public Vector3 V2;
            public Vector3 V3;

            public Vector3 this[int i]
            {
                get
                {
                    switch (i)
                    {
                        case 0: return V1;
                        case 1: return V2;
                        case 2: return V3;
                        default: throw new IndexOutOfRangeException();
                    }
                }
                set
                {
                    switch (i)
                    {
                        case 0: V1 = value; break;
                        case 1: V2 = value; break;
                        case 2: V3 = value; break;
                        default: throw new IndexOutOfRangeException();
                    }
                }
            }
        }

        private readonly int _cubesNumber;
        private readonly float _threshold;
        private readonly int _lod;
        private readonly int _lodDownscale;
        
        private readonly ComputeShader _shader;
        private readonly MeshFilter _meshFilter;

        public MeshGenerator(int cubesNumber, float threshold, int lod, ComputeShader shader, MeshFilter meshFilter)
        {
            _cubesNumber = cubesNumber;
            _threshold = threshold;
            _lod = lod;
            _lodDownscale = 1 << _lod;
            
            _shader = shader;
            _meshFilter = meshFilter;
        }

        public void UpdateMesh(Vector4[,,] cubes)
        {
            var kernelIndex = _shader.FindKernel("March");
            _shader.GetKernelThreadGroupSizes(kernelIndex, out var x, out var y, out var z);
            
            var totalCount = _cubesNumber * _cubesNumber * _cubesNumber;
            var groupsCount = new Vector3Int(_cubesNumber /  (int)x, _cubesNumber / (int)y, _cubesNumber / (int)z) / _lodDownscale;

            var cubesBuffer = CreateCubesBuffer(cubes);
            var trianglesBuffer = new ComputeBuffer(totalCount * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);
            trianglesBuffer.SetCounterValue(0);
            
            _shader.SetInt("lod_downscale", _lodDownscale);
            _shader.SetInt("cubes_number", _cubesNumber);
            _shader.SetFloat("threshold", _threshold);
            
            _shader.SetBuffer(kernelIndex, "cubes", cubesBuffer);
            _shader.SetBuffer(kernelIndex, "triangles", trianglesBuffer);
            
            _shader.Dispatch(kernelIndex, groupsCount.x, groupsCount.y, groupsCount.z);
            
            var trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
            ComputeBuffer.CopyCount(trianglesBuffer, trianglesCountBuffer, 0);
            
            var trianglesCount = new [] { 0 };
            trianglesCountBuffer.GetData(trianglesCount);

            var triangles = new Triangle[trianglesCount[0]];
            trianglesBuffer.GetData(triangles);
            
            trianglesCountBuffer.Release();
            cubesBuffer.Release();
            trianglesBuffer.Release();
            
            _meshFilter.mesh = TrianglesToMesh(triangles);
        }

        private ComputeBuffer CreateCubesBuffer(Vector4[,,] cubes)
        {
            var totalCount = _cubesNumber * _cubesNumber * _cubesNumber;
            var data = new Vector4[totalCount];

            for (var x = 0; x < _cubesNumber; x++)
            {
                for (var y = 0; y < _cubesNumber; y++)
                {
                    for (var z = 0; z < _cubesNumber; z++)
                        data[x * _cubesNumber * _cubesNumber + y * _cubesNumber + z] = cubes[x, y, z];
                }
            }

            var buffer = new ComputeBuffer(totalCount, sizeof(float) * 4);
            buffer.SetData(data);
            return buffer;
        }

        public void UpdateMeshCPU(Vector4[,,] cubes)
        {
            var skip = 1 << _lod;
            var triangles = new List<Triangle>();

            for (var x = 0; x < _cubesNumber - skip; x += skip)
            {
                for (var y = 0; y < _cubesNumber - skip; y += skip)
                {
                    for (var z = 0; z < _cubesNumber - skip; z += skip)
                        triangles.AddRange(ProcessCube(cubes, x, y, z));
                }
            }
            
            _meshFilter.mesh = TrianglesToMesh(triangles.ToArray());
        }

        private Mesh TrianglesToMesh(Triangle[] triangles)
        {
            var meshVerts = new List<Vector3>();
            var meshTris = new List<int>();

            for (var triIndex = 0; triIndex < triangles.Length; triIndex++)
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

        private List<Triangle> ProcessCube(Vector4[,,] cubes, int x, int y, int z)
        {
            var corners = new[]
            {
                cubes[x, y, z],
                cubes[x + _lodDownscale, y, z],
                cubes[x + _lodDownscale, y, z + _lodDownscale],
                cubes[x, y, z + _lodDownscale],
                cubes[x, y + _lodDownscale, z],
                cubes[x + _lodDownscale, y + _lodDownscale, z],
                cubes[x + _lodDownscale, y + _lodDownscale, z + _lodDownscale],
                cubes[x, y + _lodDownscale, z + _lodDownscale]
            };

            var triangles = new List<Triangle>();
            var cubeIndex = 0;

            for (var i = 0; i < corners.Length; i++)
            {
                if (corners[i].w >= _threshold)
                    cubeIndex |= 1 << i;
            }

            for (var triIndex = 0; triIndex < MarchingCubesTables.TriangulationTable.GetLength(1); triIndex += 3)
            {
                if (MarchingCubesTables.TriangulationTable[cubeIndex, triIndex] == -1)
                    break;

                var triangle = new Triangle();

                for (var vertIndex = 0; vertIndex < 3; vertIndex++)
                {
                    var v1 = corners[MarchingCubesTables.Corner1Index[MarchingCubesTables.TriangulationTable[cubeIndex, triIndex + vertIndex]]];
                    var v2 = corners[MarchingCubesTables.Corner2Index[MarchingCubesTables.TriangulationTable[cubeIndex, triIndex + vertIndex]]];
                    triangle[vertIndex] = GetMiddlePoint(v1, v2);
                }

                triangles.Add(triangle);
            }

            return triangles;
        }
        
        private Vector3 GetMiddlePoint(Vector4 v1, Vector4 v2)
        {
            return Vector3.Lerp(v1, v2, (_threshold - v1.w) / (v2.w - v1.w));
        }
    }
}
