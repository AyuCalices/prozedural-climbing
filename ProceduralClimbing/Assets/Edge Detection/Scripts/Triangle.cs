using System;
using UnityEngine;

namespace Edge_Detection.Scripts
{
    public readonly struct Triangle : IEquatable<Triangle>
    {
        public readonly Vector3 v0;
        public readonly Vector3 v1;
        public readonly Vector3 v2;
        public readonly Vector3 normal;
        
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 normal)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.normal = normal;
        }
        
        public bool Equals(Triangle other)
        {
            return v0.Equals(other.v0) && v1.Equals(other.v1) && v2.Equals(other.v2);
        }

        public override bool Equals(object obj)
        {
            return obj is Triangle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (v0, v1, v2).GetHashCode();
        }
    }
}
