// Author: dogfuntom, Kevin Reid, John Amanatides and Andrew Woo
// https://gamedev.stackexchange.com/questions/47362/cast-ray-to-select-block-in-voxel-game?rq=1
// https://gist.github.com/dogfuntom/cc881c8fc86ad43d55d8
using System;
using System.Runtime.InteropServices;
using UnityEngine.Assertions.Must;
using UnityEngine;
using Utilities;

namespace Experimental.Physics
{
    [StructLayout(LayoutKind.Auto)]
    internal struct PhysBoundsInt
    {
        private Vector3i _min;
        private Vector3i _max;

        public Vector3 Center
        {
            get
            {
                return (this.Max - this.Min) / 2f;
            }
        }

        public Vector3 Extents
        {
            get
            {
                return this.Max - this.Center;
            }
        }

        public Vector3i Max
        {
            get
            {
                return this._max;
            }
            set
            {
                this._max = value;
                this.Size.Any(c => c < 0).MustBeFalse();
            }
        }

        public Vector3i Min
        {
            get
            {
                return this._min;
            }
            set
            {
                this._min = value;
                this.Size.Any(c => c < 0).MustBeFalse();
            }
        }

        public Vector3i Size
        {
            get
            {
                return this.Max - this.Min;
            }
            set
            {
                this.Max = this.Min + value;
                this.Size.Any(c => c < 0).MustBeFalse();
            }
        }

        public PhysBoundsInt(Vector3i min, Vector3i max)
        {
            this._min = min;
            this._max = max;
        }

        public Vector3i ClosestPoint(Vector3i point)
        {
            throw new NotImplementedException();
        }

        public bool Contains(Vector3i point)
        {
            throw new NotImplementedException();
        }

        public void Encapsulate(Vector3i point)
        {
            throw new NotImplementedException();
        }

        public void Encapsulate(PhysBoundsInt bounds)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object other)
        {
            throw new NotImplementedException();
        }

        public void Expand(float amount)
        {
            throw new NotImplementedException();
        }

        public void Expand(Vector3i amount)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public bool IntersectRay(Ray ray)
        {
            throw new NotImplementedException();
        }

        public bool IntersectRay(Ray ray, out float distance)
        {
            throw new NotImplementedException();
        }

        public bool Intersects(PhysBoundsInt bounds)
        {
            return (this.Min.x > bounds.Max.x || this.Max.x < bounds.Min.x || this.Min.y > bounds.Max.y || this.Max.y < bounds.Min.y || this.Min.z > bounds.Max.z ? false : this.Max.z >= bounds.Min.z);
        }

        public static bool operator ==(PhysBoundsInt lhs, PhysBoundsInt rhs)
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(PhysBoundsInt lhs, PhysBoundsInt rhs)
        {
            return !(lhs == rhs);
        }

        public float SqrDistance(Vector3i point)
        {
            throw new NotImplementedException();
        }
    }
}