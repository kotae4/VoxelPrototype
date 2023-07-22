// CREDITS:
// This entire system (including response handler in ExpVoxelCollider.cs) is sourced from: https://paginas.fe.up.pt/~ei12085/blog/aabb_collision_handling.php
// Author: Luis Eduardo Reis, BrendanL.K, Kenton Hamaluik et al.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Experimental.Physics
{
    public static class CollisionExp
    {
        static float lineToPlane(Vector3 pointInPlane, Vector3 lineDirection, Vector3 linePosition, Vector3 planeNormal)
        {
            float NdotU = planeNormal.x * lineDirection.x + planeNormal.y * lineDirection.y + planeNormal.z * lineDirection.z;
            if (NdotU == 0f) return Mathf.Infinity;
            return (planeNormal.x * (linePosition.x - pointInPlane.x) + 
                planeNormal.y * (linePosition.y - pointInPlane.y) + 
                planeNormal.z * (linePosition.z - pointInPlane.z)) / NdotU;
        }

        static bool between(float x, float a, float b)
        {
            return x >= a && x <= b;
        }

        public static float sweepAABB(Vector3 aPos, Vector3 aSize, Vector3 bPos, Vector3 bSize, Vector3 aVelocity, out Vector3 hitNormal)
        {
            // NOTE: 'h' is always 1 in our voxel world.
            // NOTE: this function assumes box 'b' to be stationary, so only box 'a''s velocity is passed.
            Vector3 m, mSize;

            m.x = bPos.x - (aPos.x + aSize.x);
            m.y = bPos.y - (aPos.y + aSize.y);
            m.z = bPos.z - (aPos.z + aSize.z);
            mSize.x = aSize.x + bSize.x;
            mSize.y = aSize.y + bSize.y;
            mSize.z = aSize.z + bSize.z;

            float h = 1, s, nx = 0, ny = 0, nz = 0;
            // X min
            s = lineToPlane(Vector3.zero, aVelocity, m, Vector3.left);
            if (s >= 0 && aVelocity.x > 0 && s < h && between(s * aVelocity.y, m.y, m.y + mSize.y) && between(s * aVelocity.z, m.z, m.z + mSize.z))
                { h = s; nx = -1; ny = 0; nz = 0; }

            // X max
            s = lineToPlane(Vector3.zero, aVelocity, new Vector3(m.x + mSize.x, m.y, m.z), Vector3.right);
            if (s >= 0 && aVelocity.x < 0 && s < h && between(s * aVelocity.y, m.y, m.y + mSize.y) && between(s * aVelocity.z, m.z, m.z + mSize.z))
                { h = s; nx = 1; ny = 0; nz = 0; }

            // Y min
            s = lineToPlane(Vector3.zero, aVelocity, m, Vector3.down);
            if (s >= 0 && aVelocity.y > 0 && s < h && between(s * aVelocity.x, m.x, m.x + mSize.x) && between(s * aVelocity.z, m.z, m.z + mSize.z))
                { h = s; nx = 0; ny = -1; nz = 0; }

            // Y max
            s = lineToPlane(Vector3.zero, aVelocity, new Vector3(m.x, m.y + mSize.y, m.z), Vector3.up);
            if (s >= 0 && aVelocity.y < 0 && s < h && between(s * aVelocity.x, m.x, m.x + mSize.x) && between(s * aVelocity.z, m.z, m.z + mSize.z))
                { h = s; nx = 0; ny = 1; nz = 0; }

            // Z min
            s = lineToPlane(Vector3.zero, aVelocity, m, Vector3.back);
            if (s >= 0 && aVelocity.z > 0 && s < h && between(s * aVelocity.x, m.x, m.x + mSize.x) && between(s * aVelocity.y, m.y, m.y + mSize.y))
                { h = s; nx = 0; ny = 0; nz = -1; }

            // Z max
            s = lineToPlane(Vector3.zero, aVelocity, new Vector3(m.x, m.y, m.z + mSize.z), Vector3.forward);
            if (s >= 0 && aVelocity.z < 0 && s < h && between(s * aVelocity.x, m.x, m.x + mSize.x) && between(s * aVelocity.y, m.y, m.y + mSize.y))
                { h = s; nx = 0; ny = 0; nz = 1; }

            hitNormal = new Vector3(nx, ny, nz);
            return h;
        }
    }
}
