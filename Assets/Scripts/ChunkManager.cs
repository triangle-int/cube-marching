using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [SerializeField] private int chunksNumber;
    [SerializeField] private int chunkSize;
    [SerializeField] private float threshold;
    [SerializeField] private GameObject chunkPrefab;

    private static Chunk[,,] _chunks;

    private void Start()
    {
        _chunks = new Chunk[chunksNumber, chunksNumber, chunksNumber];
        GenerateChunks();
    }

    private void GenerateChunks()
    {
        var chunkGenerator = new SphereGenerator(chunkSize, chunksNumber);

        for (var x = 0; x < chunksNumber; x++)
        {
            for (var y = 0; y < chunksNumber; y++)
            {
                for (var z = 0; z < chunksNumber; z++)
                {
                    var position = new Vector3(x, y, z) * (chunkSize - 1);
                    var chunk = Instantiate(chunkPrefab, position, Quaternion.identity).GetComponent<Chunk>();
                    chunk.GenerateCubes(chunkSize, threshold, chunkGenerator);
                    _chunks[x, y, z] = chunk;
                }
            }
        }
    }

    public void AddValue(Vector3 position, float radius, float value)
    {
        var x = Mathf.FloorToInt(position.x / chunkSize);
        var y = Mathf.FloorToInt(position.y / chunkSize);
        var z = Mathf.FloorToInt(position.z / chunkSize);

        for (var dx = -1; dx <= 1; dx++)
        {
            for (var dy = -1; dy <= 1; dy++)
            {
                for (var dz = -1; dz <= 1; dz++)
                {
                    var chunkX = x + dx;
                    var chunkY = y + dy;
                    var chunkZ = z + dz;

                    if (
                        chunkX >= 0 && chunkX < chunksNumber &&
                        chunkY >= 0 && chunkY < chunksNumber &&
                        chunkZ >= 0 && chunkZ < chunksNumber
                    )
                        _chunks[chunkX, chunkY, chunkZ].AddValue(position, radius, value);
                }
            }
        }
    }
}