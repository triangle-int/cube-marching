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
        private CubesGenerator _generator;

        private Transform _transform;
        private LODGroup _lodGroup;

        public Vector4[] Cubes { get; private set; }
        private List<MeshGenerator> _lodsMeshGenerators;
        
        public void Generate(int chunkSize, float threshold, CubesGenerator generator)
        {
            Init(chunkSize, threshold, generator);
            GenerateCubes();
            UpdateMeshes();
        }

        private void Init(int chunkSize, float threshold, CubesGenerator generator)
        {
            _cubesNumber = chunkSize + 1;
            _threshold = threshold;
            _generator = generator;
            
            _transform = transform;
            _lodGroup = GetComponent<LODGroup>();
            
            Cubes = new Vector4[_cubesNumber * _cubesNumber * _cubesNumber];
            _lodsMeshGenerators = new List<MeshGenerator>();

            for (var lod = 0; lod < lodsMeshFilters.Count; lod++)
            {
                var meshGenerator = new MeshGenerator(this, _cubesNumber, _threshold, lod, shader, lodsMeshFilters[lod]);
                _lodsMeshGenerators.Add(meshGenerator);
            }
        }
        
        private void GenerateCubes()
        {
            Cubes = _generator.GenerateCubes(_cubesNumber, _transform.position);
        }

        private void UpdateMeshes()
        {
            foreach (var meshGenerator in _lodsMeshGenerators)
                meshGenerator.UpdateMesh();

            _lodGroup.RecalculateBounds();
        }

        public int CoordsToIndex(int x, int y, int z)
        {
            return x * _cubesNumber * _cubesNumber + y * _cubesNumber + z;
        }
    }
}
