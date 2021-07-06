﻿using UnityEngine;

namespace PlanetGeneration
{
    public interface IChunkGenerator
    {
        float GetVoxelValue(Vector3 position);
    }
}