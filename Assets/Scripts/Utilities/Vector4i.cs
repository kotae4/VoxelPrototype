using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class Vector4i
    {
        public int x, y, z, w;

        public Vector4i(int _x, int _y, int _z, int _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }

        public Vector4i(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
            w = 0;
        }

        public override bool Equals(object obj)
        {
            Vector4i other = (Vector4i)obj;
            return (other.x == x && other.y == y && other.z == z);
        }

        public static implicit operator Vector3i(Vector4i v)
        {
            return new Vector3i(v.x, v.y, v.z);
        }

        public static bool operator ==(Vector4i one, Vector4i other)
        {
            return (one.x == other.x) && (one.y == other.y) && (one.z == other.z);
        }

        public static bool operator !=(Vector4i one, Vector4i other)
        {
            return !(one == other);
        }

        public override int GetHashCode()
        {
            //return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
            unchecked
            {
                var hash = (int)23988951768543;
                hash = hash * (int)8181723713453 + x.GetHashCode();
                hash = hash * (int)8181723713453 + y.GetHashCode();
                hash = hash * (int)8181723713453 + z.GetHashCode();
                return hash;
            }
        }
    }
}
