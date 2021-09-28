using System.Collections.Generic;
using UnityEngine;

namespace PlanetGeneration
{
    [RequireComponent(typeof(LODGroup))]
    [RequireComponent(typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        [SerializeField] private List<MeshFilter> lodsMeshFilters;

        private int _cubesNumber;
        private float _threshold;
        private CubesGenerator _generator;

        private Transform _transform;
        private LODGroup _lodGroup;
        private MeshCollider _meshCollider;

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
            _meshCollider = GetComponent<MeshCollider>();
            
            Cubes = new Vector4[_cubesNumber * _cubesNumber * _cubesNumber];
            _lodsMeshGenerators = new List<MeshGenerator>();

            for (var lod = 0; lod < lodsMeshFilters.Count; lod++)
            {
                var meshGenerator = new MeshGenerator(this, _cubesNumber, _threshold, lod);
                _lodsMeshGenerators.Add(meshGenerator);
            }
        }
        
        private void GenerateCubes()
        {
            Cubes = _generator.GenerateCubes(_cubesNumber, _transform.position);
        }

        private void UpdateMeshes()
        {
            for (var lod = 0; lod < lodsMeshFilters.Count; lod++)
            {
                var mesh = _lodsMeshGenerators[lod].GenerateMesh();
                lodsMeshFilters[lod].mesh = mesh;

                if (lod == 0 && mesh.triangles.Length > 0)
                    _meshCollider.sharedMesh = mesh;
            }

            _lodGroup.RecalculateBounds();
        }
    }
}
