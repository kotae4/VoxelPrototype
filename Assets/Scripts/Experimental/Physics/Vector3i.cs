// Author: dogfuntom, Kevin Reid, John Amanatides and Andrew Woo
// https://gamedev.stackexchange.com/questions/47362/cast-ray-to-select-block-in-voxel-game?rq=1
// https://gist.github.com/dogfuntom/cc881c8fc86ad43d55d8
using System;
using System.Diagnostics;
using UnityEngine;

namespace Utilities
{

    [Serializable]
    public struct Vector3i : IEquatable<Vector3i>
    {
        public static readonly Vector3i zero = new Vector3i(0, 0, 0);
        public static readonly Vector3i one = new Vector3i(1, 1, 1);

        public static readonly Vector3i forward = new Vector3i(0, 0, 1);
        public static readonly Vector3i back = new Vector3i(0, 0, -1);
        public static readonly Vector3i up = new Vector3i(0, 1, 0);
        public static readonly Vector3i down = new Vector3i(0, -1, 0);
        public static readonly Vector3i left = new Vector3i(-1, 0, 0);
        public static readonly Vector3i right = new Vector3i(1, 0, 0);

        public static readonly Vector3i[] directions = new Vector3i[] {
        left, right,
        back, forward,
        down, up,
    };
        // do not rename, would cause deserialization problems
        public int x, y, z;

        [DebuggerStepThrough]
        public Vector3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3i(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.z = 0;
        }

        public Vector3i(Vector3 original, System.Func<float, int> convert)
        {
            this.x = convert(original.x);
            this.y = convert(original.y);
            this.z = convert(original.z);
        }

        public Vector3i(int value) : this()
        {
            this.x = this.y = this.z = value;
        }

        public int Volume
        {
            get
            {
                return this.x * this.y * this.z;
            }
        }

        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.x;
                    case 1:
                        return this.y;
                    case 2:
                        return this.z;
                    default:
                        throw new ArgumentOutOfRangeException("index");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    case 2:
                        this.z = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("index");
                }
            }
        }

        public static Vector3i operator -(Vector3i a)
        {
            return new Vector3i(-a.x, -a.y, -a.z);
        }

        [DebuggerStepThrough]
        public static Vector3i operator -(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3i operator +(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3i operator *(Vector3i v, int factor)
        {
            return new Vector3i(v.x * factor, v.y * factor, v.z * factor);
        }

        public static Vector3 operator *(Vector3i v, float factor)
        {
            return new Vector3(v.x * factor, v.y * factor, v.z * factor);
        }

        public static Vector3i operator /(Vector3i v, int factor)
        {
            return new Vector3i(v.x / factor, v.y / factor, v.z / factor);
        }

        public static Vector3 operator /(Vector3i v, float factor)
        {
            return new Vector3(v.x / factor, v.y / factor, v.z / factor);
        }

        //public static Vector3i operator %(Vector3i v, int factor)
        //{
        //    return new Vector3i(v.x % factor, v.y % factor, v.z % factor);
        //}

        public static implicit operator Vector3(Vector3i v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator Vector4i(Vector3i v)
        {
            return new Vector4i(v.x, v.y, v.z, 0);
        }

        public static explicit operator Vector3i(Vector3 v)
        {
            return Floor(v);
        }

        public static bool operator ==(Vector3i a, Vector3i b)
        {
            return a.x == b.x &&
                    a.y == b.y &&
                    a.z == b.z;
        }

        public static bool operator !=(Vector3i a, Vector3i b)
        {
            return a.x != b.x ||
                    a.y != b.y ||
                    a.z != b.z;
        }

        public static Vector3i Min(Vector3i a, Vector3i b)
        {
            return new Vector3i(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));
        }

        public static Vector3i Max(Vector3i a, Vector3i b)
        {
            return new Vector3i(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));
        }

        public static Vector3i Floor(Vector3 v)
        {
            return new Vector3i(
                Mathf.FloorToInt(v.x),
                Mathf.FloorToInt(v.y),
                Mathf.FloorToInt(v.z)
                );
        }

        public static Vector3i Ceil(Vector3 v)
        {
            return new Vector3i(
                Mathf.CeilToInt(v.x),
                Mathf.CeilToInt(v.y),
                Mathf.CeilToInt(v.z)
                );
        }

        public static Vector3i Round(Vector3 v)
        {
            return new Vector3i(
                Mathf.RoundToInt(v.x),
                Mathf.RoundToInt(v.y),
                Mathf.RoundToInt(v.z)
                );
        }

        public Vector3i Mul(Vector3i factor)
        {
            return new Vector3i(x * factor.x, y * factor.y, z * factor.z);
        }

        public Vector3i Div(Vector3i factor)
        {
            return new Vector3i(x / factor.x, y / factor.y, z / factor.z);
        }

        public Vector3i Sign()
        {
            return new Vector3i(
                System.Math.Sign(this.x),
                System.Math.Sign(this.y),
                System.Math.Sign(this.z));
        }

        public bool Any(Predicate<int> p)
        {
            if (p(x))
                return true;
            if (p(y))
                return true;
            if (p(z))
                return true;

            return false;
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3i))
                return false;

            return this.Equals((Vector3i)other);
        }

        public override int GetHashCode()
        {
            //return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
            unchecked
            {
                var hash = (int)23988961768543;
                hash = hash * (int)8187723713453 + x.GetHashCode();
                hash = hash * (int)8187723713453 + y.GetHashCode();
                hash = hash * (int)8187723713453 + z.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("Vector3i({0} {1} {2})", x, y, z);
        }

        public bool Equals(Vector3i other)
        {
            return x == other.x &&
                   y == other.y &&
                   z == other.z;
        }
    }
}