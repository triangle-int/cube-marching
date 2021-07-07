using System.Collections.Generic;
using UnityEngine;

namespace PlanetGeneration
{
    [RequireComponent(typeof(LODGroup))]
    public class Chunk : MonoBehaviour
    {
        [SerializeField] private List<MeshFilter> lodsMeshFilters;
        [SerializeField] private ComputeShader marchingCubesShader;
        [SerializeField] private ComputeShader cubesGeneratorShader;
        
        private int _cubesNumber;
        private float _threshold;

        private Transform _transform;
        private LODGroup _lodGroup;

        public Vector4[] Cubes { get; private set; }
        private List<MeshGenerator> _lodsMeshGenerators;
        
        public void Generate(int chunkSize, float threshold)
        {
            Init(chunkSize, threshold);
            GenerateCubes();
            UpdateMeshes();
        }

        private void Init(int chunkSize, float threshold)
        {
            _cubesNumber = chunkSize + 1;
            _threshold = threshold;
            
            _transform = transform;
            _lodGroup = GetComponent<LODGroup>();
            
            Cubes = new Vector4[_cubesNumber * _cubesNumber * _cubesNumber];
            _lodsMeshGenerators = new List<MeshGenerator>();

            for (var lod = 0; lod < lodsMeshFilters.Count; lod++)
            {
                var meshGenerator = new MeshGenerator(this, _cubesNumber, _threshold, lod, marchingCubesShader, lodsMeshFilters[lod]);
                _lodsMeshGenerators.Add(meshGenerator);
            }
        }
        
        private void GenerateCubes()
        {
            var kernelIndex = cubesGeneratorShader.FindKernel("Cubes");
            cubesGeneratorShader.GetKernelThreadGroupSizes(kernelIndex, out var x, out var y, out var z);
            
            var groupsCount = new Vector3Int(_cubesNumber /  (int)x, _cubesNumber / (int)y, _cubesNumber / (int)z);
            var cubesBuffer = new ComputeBuffer(Cubes.Length, sizeof(float) * 4);
            
            cubesGeneratorShader.SetInt("cubes_number", _cubesNumber);
            cubesGeneratorShader.SetVector("position", _transform.position);
            cubesGeneratorShader.SetBuffer(kernelIndex, "cubes", cubesBuffer);
            
            cubesGeneratorShader.Dispatch(kernelIndex, groupsCount.x, groupsCount.y, groupsCount.z);
            
            cubesBuffer.GetData(Cubes);
            cubesBuffer.Release();
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
