using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class Vec3s
    {
        private short m_X;
        private short m_Y;
        private short m_Z;

        public Vec3s(short x, short y, short z)
        {
            m_X = x;
            m_Y = y;
            m_Z = z;
        }

        public short X
        {
            get { return m_X; }
            set { m_X = value; }
        }

        public short Y
        {
            get { return m_Y; }
            set { m_Y = value; }
        }

        public short Z
        {
            get { return m_Z; }
            set { m_Z = value; }
        }

        public override bool Equals(object obj)
        {
            Vec3s other = (Vec3s)obj;
            return (other.X == m_X && other.Y == m_Y && other.Z == m_Z);
        }

        public static bool operator ==(Vec3s one, Vec3s other)
        {
            return (one.X == other.X) && (one.Y == other.Y) && (one.Z == other.Z);
        }

        public static bool operator !=(Vec3s one, Vec3s other)
        {
            return !(one == other);
        }
    }
}
