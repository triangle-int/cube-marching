using UnityEngine;

public interface IChunkGenerator
{
    float GetVoxelValue(Vector3 position);
    Color GetColor(Vector3 position);
}