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

        public void GenerateCubes(int chunkSize, float threshold, IChunkGenerator chunkGenerator)
        {
            Init(chunkSize, threshold, chunkGenerator);
            
            for (var x = 0; x < _cubesNumber; x++)
            {
                for (var y = 0; y < _cubesNumber; y++)
                {
                    for (var z = 0; z < _cubesNumber; z++)
                    {
                        _cubes[x, y, z] = new Vector3(x, y, z);
                        _cubes[x, y, z].w = _chunkGenerator.GetVoxelValue(_transform.position + (Vector3)_cubes[x, y, z]);
                    }
                }
            }

            UpdateMeshes();
        }

        private void UpdateMeshes()
        {
            for (var lod = 0; lod < lods.Count; lod++)
            {
                var mesh = MeshGenerator.GenerateMesh(_cubesNumber, _threshold, _cubes, lod);
                lods[lod].mesh = mesh;

                if (lod == 0)
                    _meshCollider.sharedMesh = mesh;
            }

            _lodGroup.RecalculateBounds();
        }
    }
}
