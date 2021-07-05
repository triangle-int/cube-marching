using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    private int _chunkSize;
    private float _threshold;

    private Transform _transform;
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    private Vector4[,,] _cubes;

    private void Init(int chunkSize, float threshold)
    {
        _chunkSize = chunkSize;
        _threshold = threshold;
        
        _cubes = new Vector4[_chunkSize, _chunkSize, _chunkSize];
        _transform = transform;
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
    }

    private void EnumerateAllCubes(Action<int, int, int> action)
    {
        for (var x = 0; x < _chunkSize; x++)
        {
            for (var y = 0; y < _chunkSize; y++)
            {
                for (var z = 0; z < _chunkSize; z++)
                    action(x, y, z);
            }
        }
    }

    public void GenerateCubes(int chunkSize, float threshold, IChunkGenerator chunkGenerator)
    {
        Init(chunkSize, threshold);
        
        EnumerateAllCubes((x, y, z) =>
        {
            _cubes[x, y, z] = new Vector3(x, y, z);
            _cubes[x, y, z].w = chunkGenerator.GetValue(_transform.position + (Vector3)_cubes[x, y, z]);
        });
        
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        var triangles = new List<Triangle>();
        
        EnumerateAllCubes((x, y, z) =>
        {
            if (x < _chunkSize - 1 && y < _chunkSize - 1 && z < _chunkSize - 1)
                triangles.AddRange(ProcessCube(x, y, z));
        });

        var mesh = GenerateMesh(triangles);
        _meshFilter.mesh = mesh;
        _meshCollider.sharedMesh = mesh;
    }

    private List<Triangle> ProcessCube(int x, int y, int z)
    {
        var corners = new []
        {
            _cubes[x, y, z],
            _cubes[x + 1, y, z],
            _cubes[x + 1, y, z + 1],
            _cubes[x, y, z + 1],
            _cubes[x, y + 1, z],
            _cubes[x + 1, y + 1, z],
            _cubes[x + 1, y + 1, z + 1],
            _cubes[x, y + 1, z + 1]
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

                var factor = verts[vertIndex].y % _chunkSize / _chunkSize;
                meshColors.Add(Color.Lerp(Color.yellow, Color.red, factor));
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

    public void AddValue(Vector3 point, float radius, float value)
    {
        var needsUpdate = false;
        
        EnumerateAllCubes((x, y, z) =>
        {
            if (Vector3.Distance((Vector3)_cubes[x, y, z] + _transform.position, point) <= radius)
            {
                _cubes[x, y, z].w -= value;
                needsUpdate = true;
            }
        });
        
        if (needsUpdate)
            UpdateMesh();
    }
}
