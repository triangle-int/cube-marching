using System.Collections;
using UnityEngine;

public class ChunksGenerator : MonoBehaviour
{
    [SerializeField] private int chunksNumber;
    [SerializeField] private int chunkSize;
    [SerializeField] private float threshold;
    [SerializeField] private GameObject chunkPrefab;
    
    [SerializeField] private float radius;
    [SerializeField] private Color topColor;
    [SerializeField] private Color bottomColor;
    
    private IChunkGenerator _chunkGenerator;
    private Chunk[,,] _chunks;

    private void Start()
    {
        var sphereCenter = new Vector3(1, 1, 1) * (chunkSize * chunksNumber / 2f);
        _chunkGenerator = new SphereGenerator(sphereCenter, radius, topColor, bottomColor);
        
        _chunks = new Chunk[chunksNumber, chunksNumber, chunksNumber];
        StartCoroutine(GenerateChunks());
    }

    private IEnumerator GenerateChunks()
    {
        for (var x = 0; x < chunksNumber; x++)
        {
            for (var y = 0; y < chunksNumber; y++)
            {
                for (var z = 0; z < chunksNumber; z++)
                {
                    var position = new Vector3(x, y, z) * chunkSize;
                    var chunk = Instantiate(chunkPrefab, position, Quaternion.identity).GetComponent<Chunk>();
                    chunk.GenerateCubes(chunkSize, threshold, _chunkGenerator);
                    _chunks[x, y, z] = chunk;
                }
                
                yield return null;
            }
        }
    }
}