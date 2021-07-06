using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetGeneration
{
    [RequireComponent(typeof(LODGroup))]
    [RequireComponent(typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        [SerializeField] private List<MeshFilter> lods;
    
        private int _cubesNumber;
        private float _threshold;
        private IChunkGenerator _chunkGenerator;

        private Transform _transform;
        private LODGroup _lodGroup;
        private MeshCollider _meshCollider;
        private Vector4[,,] _cubes;

        private void Init(int chunkSize, float threshold, IChunkGenerator chunkGenerator)
        {
            _cubesNumber = chunkSize + 1;
            _threshold = threshold;
            _chunkGenerator = chunkGenerator;
        
            _cubes = new Vector4[_cubesNumber, _cubesNumber, _cubesNumber];
            _transform = transform;
            _lodGroup = GetComponent<LODGroup>();
            _meshCollider = GetComponent<MeshCollider>();
        }

        private void EnumerateAllCubes(Action<int, int, int> action, int step = 1)
        {
            for (var x = 0; x < _cubesNumber; x += step)
            {
                for (var y = 0; y < _cubesNumber; y += step)
                {
                    for (var z = 0; z < _cubesNumber; z += step)
                        action(x, y, z);
                }
            }
        }

        public void GenerateCubes(int chunkSize, float threshold, IChunkGenerator chunkGenerator)
        {
            Init(chunkSize, threshold, chunkGenerator);
        
            EnumerateAllCubes((x, y, z) =>
            {
                _cubes[x, y, z] = new Vector3(x, y, z);
                _cubes[x, y, z].w = _chunkGenerator.GetVoxelValue(_transform.position + (Vector3)_cubes[x, y, z]);
            });
        
            UpdateMeshes();
        }

        private void UpdateMeshes()
        {
            for (var lod = 0; lod < lods.Count; lod++)
            {
                var cubesSkip = 1 << lod;
                var maxCubeIndex = _cubesNumber - cubesSkip;
                var triangles = new List<Triangle>();
            
                EnumerateAllCubes((x, y, z) =>
                {
                    if (x < maxCubeIndex && y < maxCubeIndex && z < maxCubeIndex)
                        triangles.AddRange(ProcessCube(x, y, z, cubesSkip));
                }, cubesSkip);
            
                var mesh = GenerateMesh(triangles);
                lods[lod].mesh = mesh;
            
                if (lod == 0 && triangles.Count > 0)
                    _meshCollider.sharedMesh = mesh;
            }
        
            _lodGroup.RecalculateBounds();
        }

        private List<Triangle> ProcessCube(int x, int y, int z, int skip)
        {
            var corners = new []
            {
                _cubes[x, y, z],
                _cubes[x + skip, y, z],
                _cubes[x + skip, y, z + skip],
                _cubes[x, y, z + skip],
                _cubes[x, y + skip, z],
                _cubes[x + skip, y + skip, z],
                _cubes[x + skip, y + skip, z + skip],
                _cubes[x, y + skip, z + skip]
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

                var verts = new List<Vector3>();

                for (var vertIndex = 0; vertIndex < 3; vertIndex++)
                {
                    var vert1 = corners[MarchingCubesTables.Corner1Index[MarchingCubesTables.TriangulationTable[cubeIndex, triIndex + vertIndex]]];
                    var vert2 = corners[MarchingCubesTables.Corner2Index[MarchingCubesTables.TriangulationTable[cubeIndex, triIndex + vertIndex]]];
                    verts.Add(GetMiddlePoint(vert1, vert2));
                }

                triangles.Add(new Triangle(verts.ToArray()));
            }

            return triangles;
        }
    
        private Vector3 GetMiddlePoint(Vector4 v1, Vector4 v2)
        {
            return Vector3.Lerp(v1, v2, (_threshold - v1.w) / (v2.w - v1.w));
        }

        private Mesh GenerateMesh(List<Triangle> triangles)
        {
            var meshVerts = new List<Vector3>();
            var meshTris = new List<int>();
            var meshColors = new List<Color>();

            for (var triIndex = 0; triIndex < triangles.Count; triIndex++)
            {
                var verts = triangles[triIndex].Verts;
            
                for (var vertIndex = 0; vertIndex < verts.Length; vertIndex++)
                {
                    meshVerts.Add(verts[vertIndex]); 
                    meshTris.Add(triIndex * verts.Length + vertIndex);
                    meshColors.Add(_chunkGenerator.GetColor(verts[vertIndex] + _transform.position));
                }
            }

            var mesh = new Mesh
            {
                vertices = meshVerts.ToArray(),
                triangles = meshTris.ToArray(),
                colors = meshColors.ToArray(),
            };

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
}
