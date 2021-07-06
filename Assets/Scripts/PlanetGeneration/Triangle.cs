using System;
using UnityEngine;

namespace PlanetGeneration
{
    public class Triangle
    {
        private readonly Vector3 _v1, _v2, _v3;

        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            _v1 = v1;
            _v2 = v2;
            _v3 = v3;
        }

        public Vector3 this[int i]
        {
            get
            {
                return i switch
                {
                    0 => _v1,
                    1 => _v2,
                    2 => _v3,
                    _ => throw new IndexOutOfRangeException()
                };
            }
        }
    }
}
