using UnityEngine;

namespace PlanetGeneration
{
    public interface ICubesGenerator
    {
        public Vector4[,,] GenerateCubes(int cubesNumber, Vector3 position);
    }
}
