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
        private readonly ComputeShader _shader;

        public MeshGenerator(Chunk chunk, int cubesNumber, float threshold, int lod, ComputeShader shader)
        {
            _chunk = chunk;
            _cubesNumber = cubesNumber;
            _threshold = threshold;
            _lodDownscale = 1 << lod;
            _shader = shader;
        }

        public Mesh GenerateMesh()
        {
            var kernelIndex = _shader.FindKernel("March");
            _shader.GetKernelThreadGroupSizes(kernelIndex, out var x, out var y, out var z);
            
            var totalCount = _cubesNumber * _cubesNumber * _cubesNumber;
            var groupsCount = new Vector3Int(_cubesNumber /  (int)x, _cubesNumber / (int)y, _cubesNumber / (int)z) / _lodDownscale;

            var cubesBuffer = new ComputeBuffer(totalCount, sizeof(float) * 4);
            cubesBuffer.SetData(_chunk.Cubes);
            
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
            
            return TrianglesToMesh(triangles);
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
    }
}
