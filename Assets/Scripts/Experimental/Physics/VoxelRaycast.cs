﻿// Author: dogfuntom, Kevin Reid, John Amanatides and Andrew Woo
// https://gamedev.stackexchange.com/questions/47362/cast-ray-to-select-block-in-voxel-game?rq=1
// https://gist.github.com/dogfuntom/cc881c8fc86ad43d55d8
// Heavily based on:
// http://gamedev.stackexchange.com/a/49423/8806
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Experimental.Physics;
using Utilities;

internal class VoxelRaycast
{
    private readonly PhysBoundsInt? worldBounds;

    public delegate TResult CallbackDel<T1, T2, T3, TResult>(T1 arg1, T2 arg2, out T3 arg3);

    public VoxelRaycast(PhysBoundsInt? worldBounds)
    {
        this.worldBounds = worldBounds;
    }

    /**
     * Call the callback with (position, face) of all blocks along the line
     * segment from point 'origin' in vector direction 'direction' of length
     * 'radius'. 'radius' may be infinite, but beware infinite loop in this case.
     *
     * 'face' is the normal vector of the face of that block that was entered.
     *
     * If the callback returns a true value, the traversal will be stopped.
     */
    public bool Raycast(
        Vector3 origin,
        Vector3 direction,
        float radius,
        CallbackDel<Vector3i, Vector3i, Block, bool> callback,
        out Vector3i hitPos,
        out Vector3i hitFace,
        out Block hitBlock)
    {
        hitPos = new Vector3i();
        // From "A Fast Voxel Traversal Algorithm for Ray Tracing"
        // by John Amanatides and Andrew Woo, 1987
        // <http://www.cse.yorku.ca/~amana/research/grid.pdf>
        // <http://citeseer.ist.psu.edu/viewdoc/summary?doi=10.1.1.42.3443>
        // Extensions to the described algorithm:
        //   • Imposed a distance limit.
        //   • The face passed through to reach the current cube is provided to
        //     the callback.

        // The foundation of this algorithm is a parameterized representation of
        // the provided ray,
        //                    origin + t * direction,
        // except that t is not actually stored; rather, at any given point in the
        // traversal, we keep track of the *greater* t values which we would have
        // if we took a step sufficient to cross a cube boundary along that axis
        // (i.e. change the integer part of the coordinate) in the variables
        // tMaxX, tMaxY, and tMaxZ.

        // Cube containing origin point.
        int x = Mathf.FloorToInt(origin[0]);
        int y = Mathf.FloorToInt(origin[1]);
        int z = Mathf.FloorToInt(origin[2]);

        if (VoxelPrototype.MeshGenerators.NaiveMeshGenerator.bCenterMesh)
        {
            float fracX = origin[0] - (float)Math.Truncate(origin[0]);
            float fracY = origin[1] - (float)Math.Truncate(origin[1]);
            float fracZ = origin[2] - (float)Math.Truncate(origin[2]);

            if (fracX > 0.5f)
                x = Mathf.CeilToInt(origin[0]);
            if (fracY > 0.5f)
                y = Mathf.CeilToInt(origin[1]);
            if (fracZ > 0.5f)
                z = Mathf.CeilToInt(origin[2]);
        }

        // Break out direction vector.
        float dx = direction[0];
        float dy = direction[1];
        float dz = direction[2];

        // Direction to increment x,y,z when stepping.
        int stepX = signum(dx);
        int stepY = signum(dy);
        int stepZ = signum(dz);

        // See description above. The initial values depend on the fractional
        // part of the origin.
        float tMaxX = intbound(origin[0], dx);
        float tMaxY = intbound(origin[1], dy);
        float tMaxZ = intbound(origin[2], dz);

        // The change in t when taking a step (always positive).
        float tDeltaX = (stepX / dx);
        float tDeltaY = (stepY / dy);
        float tDeltaZ = (stepZ / dz);

        // Buffer for reporting faces to the callback.
        hitFace = new Vector3i();

        // Avoids an infinite loop.
        if (dx == 0 && dy == 0 && dz == 0)
            throw new Exception("Ray-cast in zero direction!");

        // Rescale from units of 1 cube-edge to units of 'direction' so we can
        // compare with 't'.
        radius /= Mathf.Sqrt(dx * dx + dy * dy + dz * dz);

        // Deal with world bounds or their absence.
        Vector3i min, max;
        min = max = default(Vector3i);
        bool worldIsUnlimited = !this.worldBounds.HasValue;
        if (!worldIsUnlimited)
        {
            min = this.worldBounds.Value.Min;
            max = this.worldBounds.Value.Max;
        }

        while (worldIsUnlimited || (
            /* ray has not gone past bounds of world */
            (stepX > 0 ? x < max.x : x >= min.x) &&
            (stepY > 0 ? y < max.y : y >= min.y) &&
            (stepZ > 0 ? z < max.z : z >= min.z)))
        {
            // Invoke the callback, unless we are not *yet* within the bounds of the
            // world.
            if (worldIsUnlimited ||
                !(x < min.x || y < min.y || z < min.z || x >= max.x || y >= max.y || z >= max.z))
            {
                hitPos.x = x;
                hitPos.y = y;
                hitPos.z = z;
                if (callback(hitPos, hitFace, out hitBlock))
                    return true;
            }

            // tMaxX stores the t-value at which we cross a cube boundary along the
            // X axis, and similarly for Y and Z. Therefore, choosing the least tMax
            // chooses the closest cube boundary. Only the first case of the four
            // has been commented in detail.
            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ)
                {
                    if (tMaxX > radius)
                        break;
                    // Update which cube we are now in.
                    x += stepX;
                    // Adjust tMaxX to the next X-oriented boundary crossing.
                    tMaxX += tDeltaX;
                    // Record the normal vector of the cube face we entered.
                    hitFace[0] = -stepX;
                    hitFace[1] = 0;
                    hitFace[2] = 0;
                }
                else
                {
                    if (tMaxZ > radius)
                        break;
                    z += stepZ;
                    tMaxZ += tDeltaZ;
                    hitFace[0] = 0;
                    hitFace[1] = 0;
                    hitFace[2] = -stepZ;
                }
            }
            else
            {
                if (tMaxY < tMaxZ)
                {
                    if (tMaxY > radius)
                        break;
                    y += stepY;
                    tMaxY += tDeltaY;
                    hitFace[0] = 0;
                    hitFace[1] = -stepY;
                    hitFace[2] = 0;
                }
                else
                {
                    // Identical to the second case, repeated for simplicity in
                    // the conditionals.
                    if (tMaxZ > radius)
                        break;
                    z += stepZ;
                    tMaxZ += tDeltaZ;
                    hitFace[0] = 0;
                    hitFace[1] = 0;
                    hitFace[2] = -stepZ;
                }
            }
        }
        hitBlock = default(Block);
        return false;
    }

    // fix proposed by KillaMaaki for an edge case in intbound
    private static float ceil(float s)
    {
        if (s == 0f) return 1f;
        else return Mathf.Ceil(s);
    }

    private float intbound(float s, float ds)
    {
        if (VoxelPrototype.MeshGenerators.NaiveMeshGenerator.bCenterMesh)
        {
            float retVal = 0f;
            float fracS = s - (float)Math.Truncate(s);
            if (fracS > 0.5f)
            {
                retVal = (ds > 0f ? (1f - fracS) + 0.5f : fracS - 0.5f) / Mathf.Abs(ds);
            }
            else if (fracS < 0.5f)
            {
                retVal = (ds > 0f ? 0.5f - fracS : fracS + 0.5f) / Mathf.Abs(ds);
            }
            return retVal;
        }
        else
        {
            // Some kind of edge case, see:
            // http://gamedev.stackexchange.com/questions/47362/cast-ray-to-select-block-in-voxel-game#comment160436_4942
            bool sIsInteger = Mathf.Round(s) == s;
            if (ds < 0 && sIsInteger)
                return 0;

            return (ds > 0f ? ceil(s) - s : s - Mathf.Floor(s)) / Mathf.Abs(ds);
        }
    }

    private int signum(float x)
    {
        return x > 0 ? 1 : x < 0 ? -1 : 0;
    }
}
 