using System;
using UnityEngine;

namespace ScratchFramework
{
    [Serializable]
    public struct BVector2 : IEquatable<BVector2>
    {
        public static readonly BVector2 zero = new BVector2(Vector2.zero);
        public float x;
        public float y;

        public bool Equals(BVector2 other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj)
        {
            return obj is BVector2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public static bool operator ==(BVector2 left, BVector2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BVector2 left, BVector2 right)
        {
            return !left.Equals(right);
        }

        public BVector2(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
        }

        public BVector2(Vector3 d)
        {
            x = d.x;
            y = d.y;
        }

        public static implicit operator Vector2(BVector2 v)
        {
            return new Vector3(v.x, v.y);
        }

        //  User-defined conversion from double to Digit
        public static implicit operator BVector2(Vector2 v)
        {
            return new BVector2 { x = v.x, y = v.y };
        }

        public static implicit operator Vector3(BVector2 v)
        {
            return new Vector3(v.x, v.y);
        }

        //  User-defined conversion from double to Digit
        public static implicit operator BVector2(Vector3 v)
        {
            return new BVector2 { x = v.x, y = v.y };
        }

        public override string ToString()
        {
            return $"({x},{y})";
        }
    }
}

