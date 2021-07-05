using UnityEngine;

public class SphereGenerator : IChunkGenerator
{
    private readonly int _chunkSize, _chunksNumber;
    
    public SphereGenerator(int chunkSize, int chunksNumber)
    {
        _chunkSize = chunkSize;
        _chunksNumber = chunksNumber;
    }
    
    public float GetValue(Vector3 position)
    {
        var center = new Vector3(1, 1, 1) * ((_chunkSize - 1) * _chunksNumber / 2f);
        return -(Vector3.Distance(position, center) - _chunkSize);
    }
}