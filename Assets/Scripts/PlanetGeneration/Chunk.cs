using System.Collections.Generic;
using UnityEngine;

namespace PlanetGeneration
{
    [RequireComponent(typeof(LODGroup))]
    public class Chunk : MonoBehaviour
    {
        [SerializeField] private List<MeshFilter> lodsMeshFilters;
        [SerializeField] private ComputeShader shader;
        
        private int _cubesNumber;
        private float _threshold;
        private IChunkGenerator _chunkGenerator;

        private Transform _transform;
        private LODGroup _lodGroup;
        
        public Vector4[,,] Cubes { get; private set; }
        private List<MeshGenerator> _lodsMeshGenerators;

        private void Init(int chunkSize, float threshold, IChunkGenerator chunkGenerator)
        {
            _cubesNumber = chunkSize + 1;
            _threshold = threshold;
            _chunkGenerator = chunkGenerator;
            
            _transform = transform;
            _lodGroup = GetComponent<LODGroup>();
            
            Cubes = new Vector4[_cubesNumber, _cubesNumber, _cubesNumber];
            _lodsMeshGenerators = new List<MeshGenerator>();

            for (var lod = 0; lod < lodsMeshFilters.Count; lod++)
            {
                var meshGenerator = new MeshGenerator(this, _cubesNumber, _threshold, lod, shader, lodsMeshFilters[lod]);
                _lodsMeshGenerators.Add(meshGenerator);
            }
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
                        Cubes[x, y, z] = new Vector3(x, y, z);
                        Cubes[x, y, z].w = _chunkGenerator.GetCubeValue(_transform.position + (Vector3)Cubes[x, y, z]);
                    }
                }
            }
            
            UpdateMeshes();
        }

        private void UpdateMeshes()
        {
            foreach (var meshGenerator in _lodsMeshGenerators)
                meshGenerator.UpdateMesh();

            _lodGroup.RecalculateBounds();
        }
    }
}
