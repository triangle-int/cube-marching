using System.Collections;
using UnityEngine;

namespace PlanetGeneration
{
    [RequireComponent(typeof(CubesGenerator))]
    public class ChunksGenerator : MonoBehaviour
    {
        [SerializeField] private int chunksNumber;
        [SerializeField] private int chunkSize;
        [SerializeField] private float threshold;
        [SerializeField] private GameObject chunkPrefab;

        private CubesGenerator _generator;
        private Chunk[,,] _chunks;

        private void Start()
        {
            _generator = GetComponent<CubesGenerator>();
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
                        chunk.Generate(chunkSize, threshold, _generator);
                        _chunks[x, y, z] = chunk;
                        yield return null;
                    }
                }
            }
        }
    }
}