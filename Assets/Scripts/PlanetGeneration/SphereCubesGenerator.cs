using UnityEngine;

namespace PlanetGeneration
{
    public class SphereCubesGenerator : MonoBehaviour, ICubesGenerator
    {
        [SerializeField] private float radius;
        [SerializeField] private Vector3 sphereCenter;

        public Vector4[,,] GenerateCubes(int cubesNumber, Vector3 position)
        {
            var result = new Vector4[cubesNumber, cubesNumber, cubesNumber];

            for (var x = 0; x < cubesNumber; x++)
            {
                for (var y = 0; y < cubesNumber; y++)
                {
                    for (var z = 0; z < cubesNumber; z++)
                    {
                        result[x, y, z] = new Vector3(x, y, z);
                        var globalPos = position + (Vector3)result[x, y, z] - sphereCenter;
                        result[x, y, z].w = -(globalPos.magnitude - radius) / radius;
                    }
                }
            }

            return result;
        }
    }
}
