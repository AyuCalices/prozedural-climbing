using System;
using UnityEngine;

namespace Edge_Detection.Scripts
{
    public readonly struct Edge : IEquatable<Edge>
    {
        public readonly Vector3 v0;
        public readonly Vector3 v1;

        private readonly bool _sameHash;

        public Edge(Vector3 v0, Vector3 v1)
        {
            // ensure the same order to guarantee equality
            if (v0.GetHashCode() > v1.GetHashCode())
            {
                this.v0 = v0;
                this.v1 = v1;
                _sameHash = false;
            }
            //There might be a hash collision! When it happens, it is necessary to compare them independently of their order.
            else if (v0.GetHashCode() == v1.GetHashCode())
            {
                this.v0 = v0;
                this.v1 = v1;
                _sameHash = true;
            }
            else
            {
                this.v0 = v1;
                this.v1 = v0;
                _sameHash = false;
            }
        }
        
        public bool Equals(Edge other)
        {
            return _sameHash
                ? v0.Equals(other.v0) && v1.Equals(other.v1) || v0.Equals(other.v1) && v1.Equals(other.v0)
                : v0.Equals(other.v0) && v1.Equals(other.v1);
        }

        public override bool Equals(object obj)
        {
            return obj is Edge other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (v0, v1).GetHashCode();
        }
    }
}
