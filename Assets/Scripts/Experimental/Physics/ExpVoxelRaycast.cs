// Author: dogfuntom, Kevin Reid, John Amanatides and Andrew Woo
// https://gamedev.stackexchange.com/questions/47362/cast-ray-to-select-block-in-voxel-game?rq=1
// https://gist.github.com/dogfuntom/cc881c8fc86ad43d55d8
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utilities;

namespace Experimental.Physics
{
    internal class ExpVoxelRaycast
    {
        public delegate TResult CallbackDel<T1, T2, T3, TResult>(T1 arg1, T2 arg2, out T3 arg3);

        public static bool Raycast(Vector3 origin, Vector3 dir, CallbackDel<Vector3i, Vector3i, Block, bool> callback, out Vector3i hitPos, out Vector3i hitFace, out Block hitBlock)
        {
            Vector3i startVoxel = new Vector3i(Mathf.FloorToInt(origin.x), Mathf.FloorToInt(origin.y), Mathf.FloorToInt(origin.z));
            Vector3i endVoxel = new Vector3i((startVoxel + dir), f => Mathf.FloorToInt(f));
            return RaycastWithEndpoints(startVoxel, endVoxel, callback, out hitPos, out hitFace, out hitBlock);
        }

        static bool RaycastWithEndpoints(Vector3 vStart, Vector3 vEnd, CallbackDel<Vector3i, Vector3i, Block, bool> callback, out Vector3i hitPos, out Vector3i hitFace, out Block hitBlock)
        {
            float xStart = vStart.x + 0.5f;
            float yStart = vStart.y + 0.5f;
            float zStart = vStart.z + 0.5f;
            float xEnd = vEnd.x + 0.5f;
            float yEnd = vEnd.y + 0.5f;
            float zEnd = vEnd.z + 0.5f;

            int i = Mathf.FloorToInt(xStart);
            int j = Mathf.FloorToInt(yStart);
            int k = Mathf.FloorToInt(zStart);

            int iEnd = Mathf.FloorToInt(xEnd);
            int jEnd = Mathf.FloorToInt(yEnd);
            int kEnd = Mathf.FloorToInt(zEnd);

            // di, dj, dk is stepX, stepY, stepZ
            int di = ((xStart < xEnd) ? 1 : ((xStart > xEnd) ? -1 : 0));
            int dj = ((yStart < yEnd) ? 1 : ((yStart > yEnd) ? -1 : 0));
            int dk = ((zStart < zEnd) ? 1 : ((zStart > zEnd) ? -1 : 0));

            // deltax is tDeltaX
            float deltaX = 1.0f / Mathf.Abs(xEnd - xStart);
            float deltaY = 1.0f / Mathf.Abs(yEnd - yStart);
            float deltaZ = 1.0f / Mathf.Abs(zEnd - zStart);

            // tx is tMaxX
            float minX = Mathf.Floor(xStart), maxX = minX + 1.0f;
            float tX = ((xStart > xEnd) ? (xStart - minX) : (maxX - xStart)) * deltaX;
            float minY = Mathf.Floor(yStart), maxY = minY + 1.0f;
            float tY = ((yStart > yEnd) ? (yStart - minY) : (maxY - yStart)) * deltaY;
            float minZ = Mathf.Floor(zStart), maxZ = minZ + 1.0f;
            float tZ = ((zStart > zEnd) ? (zStart - minZ) : (maxZ - zStart)) * deltaZ;

            hitPos = new Utilities.Vector3i();
            hitFace = new Utilities.Vector3i();

            for (;;)
            {
                hitPos.x = i;
                hitPos.y = j;
                hitPos.z = k;
                if (callback(hitPos, hitFace, out hitBlock))
                {
                    return true;
                }

                if ((tX <= tY) && (tX <= tZ))
                {
                    if (i == iEnd) break;
                    tX += deltaX;
                    i += di;
                    hitFace.x = -di;
                    hitFace.y = 0;
                    hitFace.z = 0;
                }
                else if (tY <= tZ)
                {
                    if (j == jEnd) break;
                    tY += deltaY;
                    j += dj;
                    hitFace.x = 0;
                    hitFace.y = -dj;
                    hitFace.z = 0;
                }
                else
                {
                    if (k == kEnd) break;
                    tZ += deltaZ;
                    k += dk;
                    hitFace.x = 0;
                    hitFace.y = 0;
                    hitFace.z = -dk;
                }
            }
            hitBlock = default(Block);
            return false;
        }
    }
}
