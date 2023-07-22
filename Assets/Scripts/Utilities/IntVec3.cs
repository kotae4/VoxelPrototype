
namespace Utilities
{
    public struct IntVec3
    {
        private int m_X;
        private int m_Y;
        private int m_Z;
        public IntVec3(int x, int y, int z)
        {
            m_X = x;
            m_Y = y;
            m_Z = z;
        }

        public int X
        {
            get { return m_X; }
            set { m_X = value; }
        }

        public int Y
        {
            get { return m_Y; }
            set { m_Y = value; }
        }

        public int Z
        {
            get { return m_Z; }
            set { m_Z = value; }
        }

        public override bool Equals(object obj)
        {
            IntVec3 other = (IntVec3)obj;
            return (other.X == m_X && other.Y == m_Y && other.Z == m_Z);
        }

        public static bool operator ==(IntVec3 one, IntVec3 other)
        {
            return (one.X == other.X) && (one.Y == other.Y) && (one.Z == other.Z);
        }

        public static bool operator !=(IntVec3 one, IntVec3 other)
        {
            return !(one == other);
        }
    }
}