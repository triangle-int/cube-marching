using UnityEngine;

namespace PlanetGeneration
{
    public class CubesGenerator : MonoBehaviour
    {
        [SerializeField] private float radius;
        [SerializeField] private float noiseScale;
        [SerializeField] private float noiseAmplitude;
        [SerializeField] private Vector3 sphereCenter;
        [SerializeField] private ComputeShader shader;

        public Vector4[] GenerateCubes(int cubesNumber, Vector3 position)
        {
            var kernelIndex = shader.FindKernel("Cubes");
            shader.GetKernelThreadGroupSizes(kernelIndex, out var x, out var y, out var z);

            var cubes = new Vector4[cubesNumber * cubesNumber * cubesNumber];
            var groupsCount = new Vector3Int(cubesNumber /  (int)x, cubesNumber / (int)y, cubesNumber / (int)z);
            var cubesBuffer = new ComputeBuffer(cubes.Length, sizeof(float) * 4);
            
            shader.SetInt("cubes_number", cubesNumber);
            shader.SetFloat("radius", radius);
            shader.SetFloat("noise_scale", noiseScale);
            shader.SetFloat("noise_amplitude", noiseAmplitude);
            shader.SetVector("sphere_center", sphereCenter);
            shader.SetVector("position", position);
            shader.SetBuffer(kernelIndex, "cubes", cubesBuffer);
            
            shader.Dispatch(kernelIndex, groupsCount.x, groupsCount.y, groupsCount.z);
            
            cubesBuffer.GetData(cubes);
            cubesBuffer.Release();
            return cubes;
        }
    }
}
