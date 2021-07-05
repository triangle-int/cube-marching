using UnityEngine;

public class PerlinNoiseGenerator : IChunkGenerator
{
    public float GetValue(Vector3 position)
    {
        var noise = Mathf.PerlinNoise(position.x / 5, position.z / 5) * 5;
        return noise - position.y;
    }
}