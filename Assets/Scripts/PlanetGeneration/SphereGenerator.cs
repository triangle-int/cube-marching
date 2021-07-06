using UnityEngine;

namespace PlanetGeneration
{
    public class SphereGenerator : IChunkGenerator
    {
        private readonly float _sphereRadius;
        private readonly Vector3 _sphereCenter;
    
        public SphereGenerator(Vector3 sphereCenter, float radius)
        {
            _sphereRadius = radius;
            _sphereCenter = sphereCenter;
        }

        public float GetVoxelValue(Vector3 position)
        {
            return -(Vector3.Distance(position, _sphereCenter) - _sphereRadius);
        }
    }
}