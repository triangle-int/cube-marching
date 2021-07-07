using UnityEngine;

namespace PlanetGeneration
{
    public interface IChunkGenerator
    {
        float GetCubeValue(Vector3 position);
    }
}