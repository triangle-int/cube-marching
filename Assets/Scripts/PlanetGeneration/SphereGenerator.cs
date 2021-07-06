using UnityEngine;

namespace PlanetGeneration
{
    public class SphereGenerator : IChunkGenerator
    {
        private readonly float _sphereRadius;
        private readonly Vector3 _sphereCenter;
        private readonly Color _topColor, _bottomColor;
    
        public SphereGenerator(Vector3 sphereCenter, float radius, Color topColor, Color bottomColor)
        {
            _sphereRadius = radius;
            _sphereCenter = sphereCenter;
            _topColor = topColor;
            _bottomColor = bottomColor;
        }

        public float GetVoxelValue(Vector3 position)
        {
            return -(Vector3.Distance(position, _sphereCenter) - _sphereRadius);
        }
    
        public Color GetColor(Vector3 position)
        {
            var spherePoint = position - _sphereCenter;
            var factor = Vector3.Dot(Vector3.up, spherePoint.normalized) * 0.5f + 0.5f;
            return Color.Lerp(_bottomColor, _topColor, factor);
        }
    }
}