using UnityEngine;

namespace PlanetGeneration
{
    public class ChunksGenerator : MonoBehaviour
    {
        [SerializeField] private int chunksNumber;
        [SerializeField] private int chunkSize;
        [SerializeField] private float threshold;
        [SerializeField] private GameObject chunkPrefab;
    
        private Chunk[,,] _chunks;

        private void Start()
        {
            _chunks = new Chunk[chunksNumber, chunksNumber, chunksNumber];
            GenerateChunks();
        }

        private void GenerateChunks()
        {
            var time = Time.realtimeSinceStartup;
            
            for (var x = 0; x < chunksNumber; x++)
            {
                for (var y = 0; y < chunksNumber; y++)
                {
                    for (var z = 0; z < chunksNumber; z++)
                    {
                        var position = new Vector3(x, y, z) * chunkSize;
                        var chunk = Instantiate(chunkPrefab, position, Quaternion.identity).GetComponent<Chunk>();
                        chunk.Generate(chunkSize, threshold);
                        _chunks[x, y, z] = chunk;
                    }
                }
            }
            
            Debug.Log(Time.realtimeSinceStartup - time);
        }
    }
}