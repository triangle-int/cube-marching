using System;
using UnityEngine;

namespace PlanetGeneration
{
    public struct Triangle
    {
        private Vector3 _v1;
        private Vector3 _v2;
        private Vector3 _v3;

        public Vector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return _v1;
                    case 1: return _v2;
                    case 2: return _v3;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (i)
                {
                    case 0: _v1 = value; break;
                    case 1: _v2 = value; break;
                    case 2: _v3 = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }
    }
}
