using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utilities;

namespace VoxelPrototype.Lighting
{
    public enum LightOperation : byte
    {
        AddingSource = 0,
        RemovingSource,
        Nothing
    }
    public class LightProcessor
    {
        // used when generating mesh
        public static bool GetVertexLightingForFace(int worldX, int worldY, int worldZ, Direction face, out byte[] vertexLightValues)
        {
            // OPTIMIZATION:
            // TO-DO:
            // the calls to TerrainManager.GetLightValueForBlock could just return default value (0) if invalid, instead of returning a bool and putting the value in an out.
            // this would mean we don't have to have 9 if/else's. would be a huge optimization.
            vertexLightValues = new byte[4];
            byte[] neighboringLightValues = new byte[9];
            // neighoringLightValues format:
            //                    top [1]
            //
            //        topLeft [8]            topRight [2]
            //
            // left [7]           center[0]                right [3]
            //
            //        bottomLeft [6]      bottomRight [4]
            //
            //                    bottom [5]
            byte lightValue = 0;
            TerrainManager m_TerrainManager = TerrainManager.Instance;
            switch (face)
            {
                case Direction.Up:
                    {
                        // the top left vertex of the top face of a cube is shared by these blocks on the same plane:
                        // [ {x-1, y, z}, {x, y, z-1}, {x-1, y, z-1}, {x, y, z} ]
                        // the top right vertex of the top face of a cube is shared by these blocks on the same plane:
                        // [ {x+1, y, z}, {x, y, z-1}, {x+1, y, z-1}, {x, y, z} ]
                        // the bottom left vertex of the top face of a cube is shared by these blocks on the same plane:
                        // [ {x-1, y, z}, {x, y, z+1}, {x-1, y, z+1}, {x, y, z} ]
                        // the bottom right vertex of the top face of a cube is shared by these blocks on the same plane:
                        // [ {x+1, y, z}, {x, y, z+1}, {x+1, y, z+1}, {x, y, z} ]
                        // the blocks we need to calculate each of the four vertices for the top face are:
                        // [ {x-1, y, z}, {x+1, y, z}, {x, y, z-1}, {x, y, z+1}, {x-1, y, z-1}, {x+1, y, z-1}, {x-1, y, z+1}, {x+1, y, z+1}, {x, y, z} ]
                        // for each block above, we need to look at the light value at:
                        // { x, y+1, z } (this can be 0)
                        // for each face, we need to look at the light value for 9 blocks.
                        // to simplify, imagine overlaying a 3x3 grid centered and on top of the current block. each block's light value must be retrieved.
                        // to get the light value for the top left vertex of the top face of a cube, we must:
                        // 1. get the light value for each of the above block's top face
                        // 2. average them together
                        // the light value of any block's top face is the block's light value at position:
                        // {x, y+1, z} (if this is a solid block then the light value is 0, but that's okay)
                        // center
                        if (m_TerrainManager.GetLightValueForBlock(worldX, worldY + 1, worldZ, out lightValue))
                            neighboringLightValues[0] = lightValue;
                        else
                            neighboringLightValues[0] = 0;
                        // top
                        if (m_TerrainManager.GetLightValueForBlock(worldX, worldY + 1, worldZ - 1, out lightValue))
                            neighboringLightValues[1] = lightValue;
                        else
                            neighboringLightValues[1] = 0;
                        // topRight
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY + 1, worldZ - 1, out lightValue))
                            neighboringLightValues[2] = lightValue;
                        else
                            neighboringLightValues[2] = 0;
                        // right
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY + 1, worldZ, out lightValue))
                            neighboringLightValues[3] = lightValue;
                        else
                            neighboringLightValues[3] = 0;
                        // bottomRight
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY + 1, worldZ + 1, out lightValue))
                            neighboringLightValues[4] = lightValue;
                        else
                            neighboringLightValues[4] = 0;
                        // bottom
                        if (m_TerrainManager.GetLightValueForBlock(worldX, worldY + 1, worldZ + 1, out lightValue))
                            neighboringLightValues[5] = lightValue;
                        else
                            neighboringLightValues[5] = 0;
                        // bottomLeft
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY + 1, worldZ + 1, out lightValue))
                            neighboringLightValues[6] = lightValue;
                        else
                            neighboringLightValues[6] = 0;
                        // left
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY + 1, worldZ, out lightValue))
                            neighboringLightValues[7] = lightValue;
                        else
                            neighboringLightValues[7] = 0;
                        // topLeft
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY + 1, worldZ - 1, out lightValue))
                            neighboringLightValues[8] = lightValue;
                        else
                            neighboringLightValues[8] = 0;

                        // top left vertex
                        vertexLightValues[0] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[1] + neighboringLightValues[7] + neighboringLightValues[8]) / 4);
                        // top right vertex
                        vertexLightValues[1] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[1] + neighboringLightValues[2] + neighboringLightValues[3]) / 4);
                        // bottom right vertex
                        vertexLightValues[2] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[3] + neighboringLightValues[4] + neighboringLightValues[5]) / 4);
                        // bottom left vertex
                        vertexLightValues[3] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[5] + neighboringLightValues[6] + neighboringLightValues[7]) / 4);
                        return true;
                    }
                case Direction.Down:
                    {
                        // the top left vertex of the bottom face of a cube is shared by these blocks on the same plane:
                        // [ {x-1, y, z}, {x, y, z+1}, {x-1, y, z+1}, {x, y, z} ]
                        // the top right vertex of the bottom face of a cube is shared by these blocks on the same plane:
                        // [ {x+1, y, z}, {x, y, z+1}, {x+1, y, z+1}, {x, y, z} ]
                        // the bottom left vertex of the bottom face of a cube is shared by these blocks on the same plane:
                        // [ {x-1, y, z}, {x, y, z-1}, {x-1, y, z-1}, {x, y, z} ]
                        // the bottom right vertex of the bottom face of a cube is shared by these blocks on the same plane:
                        // [ {x+1, y, z}, {x, y, z-1}, {x+1, y, z-1}, {x, y, z} ]
                        // the blocks we need to calculate each of the four vertices for the bottom face are:
                        // [ {x-1, y, z}, {x+1, y, z}, {x, y, z-1}, {x, y, z+1}, {x-1, y, z-1}, {x+1, y, z-1}, {x-1, y, z+1}, {x+1, y, z+1}, {x, y, z} ]
                        // for each block above, we need to look at the light value at:
                        // { x, y-1, z } (this can be 0)
                        // center
                        if (m_TerrainManager.GetLightValueForBlock(worldX, worldY - 1, worldZ, out lightValue))
                            neighboringLightValues[0] = lightValue;
                        else
                            neighboringLightValues[0] = 0;
                        // top
                        if (m_TerrainManager.GetLightValueForBlock(worldX, worldY - 1, worldZ + 1, out lightValue))
                            neighboringLightValues[1] = lightValue;
                        else
                            neighboringLightValues[1] = 0;
                        // topRight
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY - 1, worldZ + 1, out lightValue))
                            neighboringLightValues[2] = lightValue;
                        else
                            neighboringLightValues[2] = 0;
                        // right
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY - 1, worldZ, out lightValue))
                            neighboringLightValues[3] = lightValue;
                        else
                            neighboringLightValues[3] = 0;
                        // bottomRight
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY - 1, worldZ - 1, out lightValue))
                            neighboringLightValues[4] = lightValue;
                        else
                            neighboringLightValues[4] = 0;
                        // bottom
                        if (m_TerrainManager.GetLightValueForBlock(worldX, worldY - 1, worldZ - 1, out lightValue))
                            neighboringLightValues[5] = lightValue;
                        else
                            neighboringLightValues[5] = 0;
                        // bottomLeft
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY - 1, worldZ - 1, out lightValue))
                            neighboringLightValues[6] = lightValue;
                        else
                            neighboringLightValues[6] = 0;
                        // left
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY - 1, worldZ, out lightValue))
                            neighboringLightValues[7] = lightValue;
                        else
                            neighboringLightValues[7] = 0;
                        // topLeft
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY - 1, worldZ + 1, out lightValue))
                            neighboringLightValues[8] = lightValue;
                        else
                            neighboringLightValues[8] = 0;

                        // top left vertex
                        vertexLightValues[0] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[1] + neighboringLightValues[7] + neighboringLightValues[8]) / 4);
                        // top right vertex
                        vertexLightValues[1] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[1] + neighboringLightValues[2] + neighboringLightValues[3]) / 4);
                        // bottom right vertex
                        vertexLightValues[2] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[3] + neighboringLightValues[4] + neighboringLightValues[5]) / 4);
                        // bottom left vertex
                        vertexLightValues[3] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[5] + neighboringLightValues[6] + neighboringLightValues[7]) / 4);
                        return true;
                    }
                case Direction.North:
                    {
                        // the top left vertex of the northern face of a cube is shared by these blocks on the same plane:
                        // [ {x-1, y, z}, {x, y+1, z}, {x-1, y+1, z}, {x, y, z} ]
                        // the top right vertex of the northern face of a cube is shared by these blocks on the same plane:
                        // [ {x+1, y, z}, {x, y+1, z}, {x+1, y+1, z}, {x, y, z} ]
                        // the bottom left vertex of the northern face of a cube is shared by these blocks on the same plane:
                        // [ {x-1, y, z}, {x, y-1, z}, {x-1, y-1, z}, {x, y, z} ]
                        // the bottom right vertex of the northern face of a cube is shared by these blocks on the same plane:
                        // [ {x+1, y, z}, {x, y-1, z}, {x+1, y-1, z}, {x, y, z} ]
                        // the blocks we need to calculate each of the four vertices for the northern face are:
                        // [ {x-1, y, z}, {x+1, y, z}, {x, y+1, z}, {x, y-1, z}, {x-1, y+1, z}, {x+1, y+1, z}, {x-1, y-1, z}, {x+1, y-1, z}, {x, y, z} ]
                        // for each block above, we need to look at the light value at:
                        // { x, y, z+1 } (this can be 0)

                        // center
                        if (m_TerrainManager.GetLightValueForBlock(worldX, worldY, worldZ + 1, out lightValue))
                            neighboringLightValues[0] = lightValue;
                        else
                            neighboringLightValues[0] = 0;
                        // top
                        if (m_TerrainManager.GetLightValueForBlock(worldX, worldY + 1, worldZ + 1, out lightValue))
                            neighboringLightValues[1] = lightValue;
                        else
                            neighboringLightValues[1] = 0;
                        // topRight
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY + 1, worldZ + 1, out lightValue))
                            neighboringLightValues[2] = lightValue;
                        else
                            neighboringLightValues[2] = 0;
                        // right
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY, worldZ + 1, out lightValue))
                            neighboringLightValues[3] = lightValue;
                        else
                            neighboringLightValues[3] = 0;
                        // bottomRight
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY - 1, worldZ + 1, out lightValue))
                            neighboringLightValues[4] = lightValue;
                        else
                            neighboringLightValues[4] = 0;
                        // bottom
                        if (m_TerrainManager.GetLightValueForBlock(worldX, worldY - 1, worldZ + 1, out lightValue))
                            neighboringLightValues[5] = lightValue;
                        else
                            neighboringLightValues[5] = 0;
                        // bottomLeft
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY - 1, worldZ + 1, out lightValue))
                            neighboringLightValues[6] = lightValue;
                        else
                            neighboringLightValues[6] = 0;
                        // left
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY, worldZ + 1, out lightValue))
                            neighboringLightValues[7] = lightValue;
                        else
                            neighboringLightValues[7] = 0;
                        // topLeft
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY + 1, worldZ + 1, out lightValue))
                            neighboringLightValues[8] = lightValue;
                        else
                            neighboringLightValues[8] = 0;

                        // top left vertex
                        vertexLightValues[0] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[1] + neighboringLightValues[7] + neighboringLightValues[8]) / 4);
                        // top right vertex
                        vertexLightValues[1] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[1] + neighboringLightValues[2] + neighboringLightValues[3]) / 4);
                        // bottom right vertex
                        vertexLightValues[2] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[3] + neighboringLightValues[4] + neighboringLightValues[5]) / 4);
                        // bottom left vertex
                        vertexLightValues[3] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[5] + neighboringLightValues[6] + neighboringLightValues[7]) / 4);
                        return true;
                    }
                case Direction.South:
                    {
                        // the top left vertex of the southern face of a cube is shared by these blocks on the same plane:
                        // [ {x+1, y, z}, {x, y+1, z}, {x+1, y+1, z}, {x, y, z} ]
                        // the top right vertex of the southern face of a cube is shared by these blocks on the same plane:
                        // [ {x-1, y, z}, {x, y+1, z}, {x-1, y+1, z}, {x, y, z} ]
                        // the bottom left vertex of the southern face of a cube is shared by these blocks on the same plane:
                        // [ {x+1, y, z}, {x, y-1, z}, {x+1, y-1, z}, {x, y, z} ]
                        // the bottom right vertex of the southern face of a cube is shared by these blocks on the same plane:
                        // [ {x-1, y, z}, {x, y-1, z}, {x-1, y-1, z}, {x, y, z} ]
                        // the blocks we need to calculate each of the four vertices for the southern face are:
                        // [ {x-1, y, z}, {x+1, y, z}, {x, y+1, z}, {x, y-1, z}, {x-1, y+1, z}, {x+1, y+1, z}, {x-1, y-1, z}, {x+1, y-1, z}, {x, y, z} ]
                        // for each block above, we need to look at the light value at:
                        // { x, y, z-1 } (this can be 0)
                        // center
                        if (m_TerrainManager.GetLightValueForBlock(worldX, worldY, worldZ - 1, out lightValue))
                            neighboringLightValues[0] = lightValue;
                        else
                            neighboringLightValues[0] = 0;
                        // top
                        if (m_TerrainManager.GetLightValueForBlock(worldX, worldY + 1, worldZ - 1, out lightValue))
                            neighboringLightValues[1] = lightValue;
                        else
                            neighboringLightValues[1] = 0;
                        // topRight
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY + 1, worldZ - 1, out lightValue))
                            neighboringLightValues[2] = lightValue;
                        else
                            neighboringLightValues[2] = 0;
                        // right
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY, worldZ - 1, out lightValue))
                            neighboringLightValues[3] = lightValue;
                        else
                            neighboringLightValues[3] = 0;
                        // bottomRight
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY - 1, worldZ - 1, out lightValue))
                            neighboringLightValues[4] = lightValue;
                        else
                            neighboringLightValues[4] = 0;
                        // bottom
                        if (m_TerrainManager.GetLightValueForBlock(worldX, worldY - 1, worldZ - 1, out lightValue))
                            neighboringLightValues[5] = lightValue;
                        else
                            neighboringLightValues[5] = 0;
                        // bottomLeft
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY - 1, worldZ - 1, out lightValue))
                            neighboringLightValues[6] = lightValue;
                        else
                            neighboringLightValues[6] = 0;
                        // left
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY, worldZ - 1, out lightValue))
                            neighboringLightValues[7] = lightValue;
                        else
                            neighboringLightValues[7] = 0;
                        // topLeft
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY + 1, worldZ - 1, out lightValue))
                            neighboringLightValues[8] = lightValue;
                        else
                            neighboringLightValues[8] = 0;

                        // top left vertex
                        vertexLightValues[0] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[1] + neighboringLightValues[7] + neighboringLightValues[8]) / 4);
                        // top right vertex
                        vertexLightValues[1] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[1] + neighboringLightValues[2] + neighboringLightValues[3]) / 4);
                        // bottom right vertex
                        vertexLightValues[2] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[3] + neighboringLightValues[4] + neighboringLightValues[5]) / 4);
                        // bottom left vertex
                        vertexLightValues[3] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[5] + neighboringLightValues[6] + neighboringLightValues[7]) / 4);
                        return true;
                    }
                case Direction.East:
                    {
                        // the top left vertex of the eastern face of a cube is shared by these blocks on the same plane:
                        // [ {x, y, z-1}, {x, y+1, z}, {x, y+1, z-1}, {x, y, z} ]
                        // the top right vertex of the eastern face of a cube is shared by these blocks on the same plane:
                        // [ {x, y, z+1}, {x, y+1, z}, {x, y+1, z+1}, {x, y, z} ]
                        // the bottom left vertex of the eastern face of a cube is shared by these blocks on the same plane:
                        // [ {x, y, z-1}, {x, y-1, z}, {x, y-1, z-1}, {x, y, z} ]
                        // the bottom right vertex of the eastern face of a cube is shared by these blocks on the same plane:
                        // [ {x, y, z+1}, {x, y-1, z}, {x, y-1, z+1}, {x, y, z} ]
                        // the blocks we need to calculate each of the four vertices for the eastern face are:
                        // [ {x, y, z-1}, {x, y, z+1}, {x, y+1, z}, {x, y-1, z}, {x, y+1, z-1}, {x, y+1, z+1}, {x, y-1, z-1}, {x, y-1, z+1}, {x, y, z} ]
                        // for each block above, we need to look at the light value at:
                        // { x+1, y, z } (this can be 0)
                        // center
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY, worldZ, out lightValue))
                            neighboringLightValues[0] = lightValue;
                        else
                            neighboringLightValues[0] = 0;
                        // top
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY + 1, worldZ, out lightValue))
                            neighboringLightValues[1] = lightValue;
                        else
                            neighboringLightValues[1] = 0;
                        // topRight
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY + 1, worldZ + 1, out lightValue))
                            neighboringLightValues[2] = lightValue;
                        else
                            neighboringLightValues[2] = 0;
                        // right
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY, worldZ + 1, out lightValue))
                            neighboringLightValues[3] = lightValue;
                        else
                            neighboringLightValues[3] = 0;
                        // bottomRight
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY - 1, worldZ + 1, out lightValue))
                            neighboringLightValues[4] = lightValue;
                        else
                            neighboringLightValues[4] = 0;
                        // bottom
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY - 1, worldZ, out lightValue))
                            neighboringLightValues[5] = lightValue;
                        else
                            neighboringLightValues[5] = 0;
                        // bottomLeft
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY - 1, worldZ - 1, out lightValue))
                            neighboringLightValues[6] = lightValue;
                        else
                            neighboringLightValues[6] = 0;
                        // left
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY, worldZ - 1, out lightValue))
                            neighboringLightValues[7] = lightValue;
                        else
                            neighboringLightValues[7] = 0;
                        // topLeft
                        if (m_TerrainManager.GetLightValueForBlock(worldX + 1, worldY + 1, worldZ - 1, out lightValue))
                            neighboringLightValues[8] = lightValue;
                        else
                            neighboringLightValues[8] = 0;

                        // top left vertex
                        vertexLightValues[0] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[1] + neighboringLightValues[7] + neighboringLightValues[8]) / 4);
                        // top right vertex
                        vertexLightValues[1] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[1] + neighboringLightValues[2] + neighboringLightValues[3]) / 4);
                        // bottom right vertex
                        vertexLightValues[2] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[3] + neighboringLightValues[4] + neighboringLightValues[5]) / 4);
                        // bottom left vertex
                        vertexLightValues[3] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[5] + neighboringLightValues[6] + neighboringLightValues[7]) / 4);
                        return true;
                    }
                case Direction.West:
                    {
                        // the top left vertex of the western face of a cube is shared by these blocks on the same plane:
                        // [ {x, y, z+1}, {x, y+1, z}, {x, y+1, z+1}, {x, y, z} ]
                        // the top right vertex of the western face of a cube is shared by these blocks on the same plane:
                        // [ {x, y, z-1}, {x, y+1, z}, {x, y+1, z-1}, {x, y, z} ]
                        // the bottom left vertex of the western face of a cube is shared by these blocks on the same plane:
                        // [ {x, y, z+1}, {x, y-1, z}, {x, y-1, z+1}, {x, y, z} ]
                        // the bottom right vertex of the western face of a cube is shared by these blocks on the same plane:
                        // [ {x, y, z-1}, {x, y-1, z}, {x, y-1, z-1}, {x, y, z} ]
                        // the blocks we need to calculate each of the four vertices for the western face are:
                        // [ {x, y, z-1}, {x, y, z+1}, {x, y+1, z}, {x, y-1, z}, {x, y+1, z-1}, {x, y+1, z+1}, {x, y-1, z-1}, {x, y-1, z+1}, {x, y, z} ]
                        // for each block above, we need to look at the light value at:
                        // { x-1, y, z } (this can be 0)
                        // center
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY, worldZ, out lightValue))
                            neighboringLightValues[0] = lightValue;
                        else
                            neighboringLightValues[0] = 0;
                        // top
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY + 1, worldZ, out lightValue))
                            neighboringLightValues[1] = lightValue;
                        else
                            neighboringLightValues[1] = 0;
                        // topRight
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY + 1, worldZ - 1, out lightValue))
                            neighboringLightValues[2] = lightValue;
                        else
                            neighboringLightValues[2] = 0;
                        // right
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY, worldZ - 1, out lightValue))
                            neighboringLightValues[3] = lightValue;
                        else
                            neighboringLightValues[3] = 0;
                        // bottomRight
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY - 1, worldZ - 1, out lightValue))
                            neighboringLightValues[4] = lightValue;
                        else
                            neighboringLightValues[4] = 0;
                        // bottom
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY - 1, worldZ, out lightValue))
                            neighboringLightValues[5] = lightValue;
                        else
                            neighboringLightValues[5] = 0;
                        // bottomLeft
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY - 1, worldZ + 1, out lightValue))
                            neighboringLightValues[6] = lightValue;
                        else
                            neighboringLightValues[6] = 0;
                        // left
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY, worldZ + 1, out lightValue))
                            neighboringLightValues[7] = lightValue;
                        else
                            neighboringLightValues[7] = 0;
                        // topLeft
                        if (m_TerrainManager.GetLightValueForBlock(worldX - 1, worldY + 1, worldZ + 1, out lightValue))
                            neighboringLightValues[8] = lightValue;
                        else
                            neighboringLightValues[8] = 0;

                        // top left vertex
                        vertexLightValues[0] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[1] + neighboringLightValues[7] + neighboringLightValues[8]) / 4);
                        // top right vertex
                        vertexLightValues[1] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[1] + neighboringLightValues[2] + neighboringLightValues[3]) / 4);
                        // bottom right vertex
                        vertexLightValues[2] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[3] + neighboringLightValues[4] + neighboringLightValues[5]) / 4);
                        // bottom left vertex
                        vertexLightValues[3] = (byte)((int)(neighboringLightValues[0] + neighboringLightValues[5] + neighboringLightValues[6] + neighboringLightValues[7]) / 4);
                        return true;
                    }
                default:
                    return false;
            }
        }

        // only used when pressing 'F2'
        public static void PropagateSunlightRecursive(int worldX, int worldY, int worldZ, byte lightIntensity)
        {
            const int TotalBlocksX = Settings.NUM_CHUNKS * Settings.ChunkSizeX;
            const int TotalBlocksY = Settings.NUM_CHUNKS * Settings.ChunkSizeY;
            const int TotalBlocksZ = Settings.NUM_CHUNKS * Settings.ChunkSizeZ;

            if ((worldX < 0) || (worldY < 0) || (worldZ < 0) ||
                (worldX >= TotalBlocksX) || (worldY >= TotalBlocksY) || (worldZ >= TotalBlocksZ))
                return;

            Chunk[,,] m_ActiveChunks = TerrainManager.Instance.m_ActiveChunks;

            int chunkX = worldX / Settings.ChunkSizeX;
            int chunkY = worldY / Settings.ChunkSizeY;
            int chunkZ = worldZ / Settings.ChunkSizeZ;

            int localX = worldX % Settings.ChunkSizeX;
            int localY = worldY % Settings.ChunkSizeY;
            int localZ = worldZ % Settings.ChunkSizeZ;

            // DEBUG ONLY (blocks will never have ID of 0 under normal circumstances)
            if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].BlockTypeID == 0)
                return;
            // END DEBUG ONLY

            // light can't pass through solid blocks, therefore solid blocks have no light value
            if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].BlockTypeID != (byte)BlockID.Air)
                return;
            // if the block is already as bright or brighter then stop propagating (we would make it and its neighbors DARKER otherwise)
            if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].SunlightValue >= lightIntensity)
                return;

            m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].SunlightValue = lightIntensity;


            byte newLightIntensity = (byte)(lightIntensity - 1);
            // if we've run out of light, stop propagating
            if (newLightIntensity <= 0)
                return;

            PropagateSunlightRecursive(worldX + 1, worldY, worldZ, newLightIntensity);
            PropagateSunlightRecursive(worldX - 1, worldY, worldZ, newLightIntensity);
            PropagateSunlightRecursive(worldX, worldY + 1, worldZ, newLightIntensity);
            PropagateSunlightRecursive(worldX, worldY - 1, worldZ, newLightIntensity);
            PropagateSunlightRecursive(worldX, worldY, worldZ + 1, newLightIntensity);
            PropagateSunlightRecursive(worldX, worldY, worldZ - 1, newLightIntensity);
        }

        // never used
        public static void PropagateSunLight(int chunkX, int chunkY, int chunkZ, int localX, int localY, int localZ, byte lightIntensity)
        {
            IntVec3 worldPos = new IntVec3((chunkX * Settings.ChunkSizeX) + localX, (chunkY * Settings.ChunkSizeY) + localY, (chunkZ * Settings.ChunkSizeZ) + localZ);
            TerrainManager terrainManager = TerrainManager.Instance;
            Chunk[,,] m_ActiveChunks = terrainManager.m_ActiveChunks;

            Debug.Log("Propagating light for [" + chunkX + "," + chunkY + "," + chunkZ + "](" + localX + "," + localY + "," + localZ + "){" + worldPos.X + "," + worldPos.Y + "," + worldPos.Z + "}");
            Queue<ByteVec3> neighbors = new Queue<ByteVec3>();

            ByteVec3 neighborChunkPos, neighborLocalPos;

            ByteVec3 workingChunkPos, workingLocalPos;
            Block workingBlock;

            int iterations = 0;

            // right neighbor
            if (terrainManager.GetChunkLocalFromWorld(worldPos.X + 1, worldPos.Y, worldPos.Z, out neighborChunkPos, out neighborLocalPos))
            {
                workingBlock = m_ActiveChunks[neighborChunkPos.X, neighborChunkPos.Y, neighborChunkPos.Z].Blocks[neighborLocalPos.X, neighborLocalPos.Y, neighborLocalPos.Z];
                if ((workingBlock.SunlightValue < lightIntensity) && (workingBlock.BlockTypeID == (byte)BlockID.Air))
                {
                    neighbors.Enqueue(new ByteVec3(worldPos.X + 1, worldPos.Y, worldPos.Z));
                    Debug.Log("Initially enqueued [" + neighborChunkPos.X + "," + neighborChunkPos.Y + "," + neighborChunkPos.Z + "](" +
                        neighborLocalPos.X + "," + neighborLocalPos.Y + "," + neighborLocalPos.Z + "){" +
                        (worldPos.X + 1) + "," + worldPos.Y + "," + worldPos.Z + "}");
                }
            }

            // left neighbor
            if (terrainManager.GetChunkLocalFromWorld(worldPos.X - 1, worldPos.Y, worldPos.Z, out neighborChunkPos, out neighborLocalPos))
            {
                workingBlock = m_ActiveChunks[neighborChunkPos.X, neighborChunkPos.Y, neighborChunkPos.Z].Blocks[neighborLocalPos.X, neighborLocalPos.Y, neighborLocalPos.Z];
                if ((workingBlock.SunlightValue < lightIntensity) && (workingBlock.BlockTypeID == (byte)BlockID.Air))
                {
                    neighbors.Enqueue(new ByteVec3(worldPos.X - 1, worldPos.Y, worldPos.Z));
                    Debug.Log("Initially enqueued [" + neighborChunkPos.X + "," + neighborChunkPos.Y + "," + neighborChunkPos.Z + "](" +
                        neighborLocalPos.X + "," + neighborLocalPos.Y + "," + neighborLocalPos.Z + "){" +
                        (worldPos.X - 1) + "," + worldPos.Y + "," + worldPos.Z + "}");
                }
            }

            // north neighbor
            if (terrainManager.GetChunkLocalFromWorld(worldPos.X, worldPos.Y, worldPos.Z + 1, out neighborChunkPos, out neighborLocalPos))
            {
                workingBlock = m_ActiveChunks[neighborChunkPos.X, neighborChunkPos.Y, neighborChunkPos.Z].Blocks[neighborLocalPos.X, neighborLocalPos.Y, neighborLocalPos.Z];
                if ((workingBlock.SunlightValue < lightIntensity) && (workingBlock.BlockTypeID == (byte)BlockID.Air))
                {
                    neighbors.Enqueue(new ByteVec3(worldPos.X, worldPos.Y, worldPos.Z + 1));
                    Debug.Log("Initially enqueued [" + neighborChunkPos.X + "," + neighborChunkPos.Y + "," + neighborChunkPos.Z + "](" +
                        neighborLocalPos.X + "," + neighborLocalPos.Y + "," + neighborLocalPos.Z + "){" +
                        worldPos.X + "," + worldPos.Y + "," + (worldPos.Z + 1) + "}");
                }
            }

            // south neighbor
            if (terrainManager.GetChunkLocalFromWorld(worldPos.X, worldPos.Y, worldPos.Z - 1, out neighborChunkPos, out neighborLocalPos))
            {
                workingBlock = m_ActiveChunks[neighborChunkPos.X, neighborChunkPos.Y, neighborChunkPos.Z].Blocks[neighborLocalPos.X, neighborLocalPos.Y, neighborLocalPos.Z];
                if ((workingBlock.SunlightValue < lightIntensity) && (workingBlock.BlockTypeID == (byte)BlockID.Air))
                {
                    neighbors.Enqueue(new ByteVec3(worldPos.X, worldPos.Y, worldPos.Z - 1));
                    Debug.Log("Initially enqueued [" + neighborChunkPos.X + "," + neighborChunkPos.Y + "," + neighborChunkPos.Z + "](" +
                        neighborLocalPos.X + "," + neighborLocalPos.Y + "," + neighborLocalPos.Z + "){" +
                        worldPos.X + "," + worldPos.Y + "," + (worldPos.Z - 1) + "}");
                }
            }

            while (neighbors.Count > 0)
            {
                iterations++;
                if (iterations > 61)
                {
                    Debug.Log("Forcibly exiting for [" + chunkX + "," + chunkY + "," + chunkZ + "](" + localX + "," + localY + "," + localZ + "){" + worldPos.X + "," + worldPos.Y + "," + worldPos.Z + "}");
                    return;
                }
                ByteVec3 neighborWorldPos = neighbors.Dequeue();

                lightIntensity = (byte)(Settings.SUNLIGHT_VALUE - (neighborWorldPos.X == worldPos.X ? Mathf.Abs((neighborWorldPos.Z - worldPos.Z)) : Mathf.Abs((neighborWorldPos.X - worldPos.X))));
                if (lightIntensity <= 0)
                    continue;

                if (!terrainManager.GetChunkLocalFromWorld(neighborWorldPos.X, neighborWorldPos.Y, neighborWorldPos.Z, out workingChunkPos, out workingLocalPos))
                    continue;
                Debug.Log("Dequeued [" + workingChunkPos.X + "," + workingChunkPos.Y + "," + workingChunkPos.Z + "](" +
                    workingLocalPos.X + "," + workingLocalPos.Y + "," + workingLocalPos.Z + ") with light value " + lightIntensity);

                workingBlock = m_ActiveChunks[workingChunkPos.X, workingChunkPos.Y, workingChunkPos.Z].Blocks[workingLocalPos.X, workingLocalPos.Y, workingLocalPos.Z];
                workingBlock.SunlightValue = lightIntensity;

                // right neighbor
                if (terrainManager.GetChunkLocalFromWorld(neighborWorldPos.X + 1, neighborWorldPos.Y, neighborWorldPos.Z, out neighborChunkPos, out neighborLocalPos))
                {
                    workingBlock = m_ActiveChunks[neighborChunkPos.X, neighborChunkPos.Y, neighborChunkPos.Z].Blocks[neighborLocalPos.X, neighborLocalPos.Y, neighborLocalPos.Z];
                    if ((workingBlock.SunlightValue < lightIntensity) && (workingBlock.BlockTypeID == (byte)BlockID.Air))
                    {
                        neighbors.Enqueue(new ByteVec3(neighborWorldPos.X + 1, neighborWorldPos.Y, neighborWorldPos.Z));
                        Debug.Log("Enqueued [" + neighborChunkPos.X + "," + neighborChunkPos.Y + "," + neighborChunkPos.Z + "]("
                            + neighborLocalPos.X + "," + neighborLocalPos.Y + "," + neighborLocalPos.Z + "){"
                            + (neighborWorldPos.X + 1) + "," + neighborWorldPos.Y + "," + neighborWorldPos.Z + "}");
                    }
                }

                // left neighbor
                if (terrainManager.GetChunkLocalFromWorld(neighborWorldPos.X - 1, neighborWorldPos.Y, neighborWorldPos.Z, out neighborChunkPos, out neighborLocalPos))
                {
                    workingBlock = m_ActiveChunks[neighborChunkPos.X, neighborChunkPos.Y, neighborChunkPos.Z].Blocks[neighborLocalPos.X, neighborLocalPos.Y, neighborLocalPos.Z];
                    if ((workingBlock.SunlightValue < lightIntensity) && (workingBlock.BlockTypeID == (byte)BlockID.Air))
                    {
                        neighbors.Enqueue(new ByteVec3(neighborWorldPos.X - 1, neighborWorldPos.Y, neighborWorldPos.Z));
                        Debug.Log("Enqueued [" + neighborChunkPos.X + "," + neighborChunkPos.Y + "," + neighborChunkPos.Z + "]("
                            + neighborLocalPos.X + "," + neighborLocalPos.Y + "," + neighborLocalPos.Z + "){"
                            + (neighborWorldPos.X - 1) + "," + neighborWorldPos.Y + "," + neighborWorldPos.Z + "}");
                    }
                }

                // north neighbor
                if (terrainManager.GetChunkLocalFromWorld(neighborWorldPos.X, neighborWorldPos.Y, neighborWorldPos.Z + 1, out neighborChunkPos, out neighborLocalPos))
                {
                    workingBlock = m_ActiveChunks[neighborChunkPos.X, neighborChunkPos.Y, neighborChunkPos.Z].Blocks[neighborLocalPos.X, neighborLocalPos.Y, neighborLocalPos.Z];
                    if ((workingBlock.SunlightValue < lightIntensity) && (workingBlock.BlockTypeID == (byte)BlockID.Air))
                    {
                        neighbors.Enqueue(new ByteVec3(neighborWorldPos.X, neighborWorldPos.Y, neighborWorldPos.Z + 1));
                        Debug.Log("Enqueued [" + neighborChunkPos.X + "," + neighborChunkPos.Y + "," + neighborChunkPos.Z + "]("
                            + neighborLocalPos.X + "," + neighborLocalPos.Y + "," + neighborLocalPos.Z + "){"
                            + neighborWorldPos.X + "," + neighborWorldPos.Y + "," + (neighborWorldPos.Z + 1) + "}");
                    }
                }

                // south neighbor
                if (terrainManager.GetChunkLocalFromWorld(neighborWorldPos.X, neighborWorldPos.Y, neighborWorldPos.Z - 1, out neighborChunkPos, out neighborLocalPos))
                {
                    workingBlock = m_ActiveChunks[neighborChunkPos.X, neighborChunkPos.Y, neighborChunkPos.Z].Blocks[neighborLocalPos.X, neighborLocalPos.Y, neighborLocalPos.Z];
                    if ((workingBlock.SunlightValue < lightIntensity) && (workingBlock.BlockTypeID == (byte)BlockID.Air))
                    {
                        neighbors.Enqueue(new ByteVec3(neighborWorldPos.X, neighborWorldPos.Y, neighborWorldPos.Z - 1));
                        Debug.Log("Enqueued [" + neighborChunkPos.X + "," + neighborChunkPos.Y + "," + neighborChunkPos.Z + "]("
                            + neighborLocalPos.X + "," + neighborLocalPos.Y + "," + neighborLocalPos.Z + "){"
                            + neighborWorldPos.X + "," + neighborWorldPos.Y + "," + (neighborWorldPos.Z - 1) + "}");
                    }
                }
            }

            Debug.Log("Done for [" + chunkX + "," + chunkY + "," + chunkZ + "](" + localX + "," + localY + "," + localZ + "){" + worldPos.X + "," + worldPos.Y + "," + worldPos.Z + "}");
        }

        // primary, used when generating terrain
        public static void PropagateSunlight(Chunk chunk)
        {
            const int TotalBlocksX = Settings.NUM_CHUNKS * Settings.ChunkSizeX;
            const int TotalBlocksY = Settings.NUM_CHUNKS * Settings.ChunkSizeY;
            const int TotalBlocksZ = Settings.NUM_CHUNKS * Settings.ChunkSizeZ;

            // NOTE: chunk.SunlightBlocks is in WORLD coords, not local chunk coords!
            Queue<Vector4i> neighbors = new Queue<Vector4i>();
            Chunk[,,] m_ActiveChunks = TerrainManager.Instance.m_ActiveChunks;
            int chunkX, chunkY, chunkZ;
            int localX, localY, localZ;


            foreach (Vector3i blockPos in chunk.SunlightBlocks)
            {
                // left neighbor
                neighbors.Enqueue(new Vector4i((byte)(blockPos.x - 1), blockPos.y, blockPos.z, Settings.SUNLIGHT_VALUE - 1));
                // right neighbor
                neighbors.Enqueue(new Vector4i((byte)(blockPos.x + 1), blockPos.y, blockPos.z, Settings.SUNLIGHT_VALUE - 1));
                // bottom neighbor
                neighbors.Enqueue(new Vector4i(blockPos.x, (byte)(blockPos.y - 1), blockPos.z, Settings.SUNLIGHT_VALUE - 1));
                // top neighbor
                neighbors.Enqueue(new Vector4i(blockPos.x, (byte)(blockPos.y + 1), blockPos.z, Settings.SUNLIGHT_VALUE - 1));
                // front neighbor
                neighbors.Enqueue(new Vector4i(blockPos.x, blockPos.y, (byte)(blockPos.z - 1), Settings.SUNLIGHT_VALUE - 1));
                // back neighbor
                neighbors.Enqueue(new Vector4i(blockPos.x, blockPos.y, (byte)(blockPos.z + 1), Settings.SUNLIGHT_VALUE - 1));
                while (neighbors.Count > 0)
                {
                    Vector4i neighborPos = neighbors.Dequeue();
                    if ((neighborPos.x < 0) || (neighborPos.y < 0) || (neighborPos.z < 0) ||
                        (neighborPos.x >= TotalBlocksX) || (neighborPos.y >= TotalBlocksY) || (neighborPos.z >= TotalBlocksZ))
                        continue;
                    chunkX = neighborPos.x / Settings.ChunkSizeX;
                    chunkY = neighborPos.y / Settings.ChunkSizeY;
                    chunkZ = neighborPos.z / Settings.ChunkSizeZ;
                    localX = neighborPos.x % Settings.ChunkSizeX;
                    localY = neighborPos.y % Settings.ChunkSizeY;
                    localZ = neighborPos.z % Settings.ChunkSizeZ;

                    // DEBUG ONLY (blocks will never have ID of 0 under normal circumstances)
                    if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].BlockTypeID == 0)
                        continue;
                    // end debug only

                    // light can't penetrate solid objects, so don't give it a light value
                    if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].BlockTypeID != (byte)BlockID.Air)
                        continue;
                    // if it's already more lit then don't light it with a darker value...
                    if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].SunlightValue >= neighborPos.w)
                        continue;
                    // this block passed the tests, give it the light value
                    m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].SunlightValue = (byte)neighborPos.w;

                    byte newLightIntensity = (byte)(neighborPos.w - 1);
                    // if the new light value is 0 or less then we've run out of light to propagate
                    // so don't even add the neighbors
                    if (newLightIntensity <= 0)
                        continue;

                    // we've still got some light left, so let's spread it to the neighbors
                    // left neighbor
                    neighbors.Enqueue(new Vector4i(neighborPos.x - 1, neighborPos.y, neighborPos.z, newLightIntensity));
                    // right neighbor
                    neighbors.Enqueue(new Vector4i(neighborPos.x + 1, neighborPos.y, neighborPos.z, newLightIntensity));
                    // bottom neighbor
                    neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y - 1, neighborPos.z, newLightIntensity));
                    // top neighbor
                    neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y + 1, neighborPos.z, newLightIntensity));
                    // front neighbor
                    neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y, neighborPos.z - 1, newLightIntensity));
                    // back neighbor
                    neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y, neighborPos.z + 1, newLightIntensity));
                }
            }
        }

        // primary, used when placing / destroying blocks
        /// <summary>
        /// Propagates light outward from the supplied location according to the supplied lighting value
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="worldZ"></param>
        /// <param name="startingLightValue">The block at [worldX, worldY, worldZ]'s lighting value minus one</param>
        public static void PropagateLightFrom(int worldX, int worldY, int worldZ, byte startingLightValue)
        {
            const int TotalBlocksX = Settings.NUM_CHUNKS * Settings.ChunkSizeX;
            const int TotalBlocksY = Settings.NUM_CHUNKS * Settings.ChunkSizeY;
            const int TotalBlocksZ = Settings.NUM_CHUNKS * Settings.ChunkSizeZ;

            Queue<Vector4i> neighbors = new Queue<Vector4i>();
            Chunk[,,] m_ActiveChunks = TerrainManager.Instance.m_ActiveChunks;
            int chunkX, chunkY, chunkZ;
            int localX, localY, localZ;

            // left neighbor
            neighbors.Enqueue(new Vector4i(worldX - 1, worldY, worldZ, startingLightValue - 1));
            // right neighbor
            neighbors.Enqueue(new Vector4i(worldX + 1, worldY, worldZ, startingLightValue - 1));
            // bottom neighbor
            neighbors.Enqueue(new Vector4i(worldX, worldY - 1, worldZ, startingLightValue - 1));
            // top neighbor
            neighbors.Enqueue(new Vector4i(worldX, worldY + 1, worldZ, startingLightValue - 1));
            // front neighbor
            neighbors.Enqueue(new Vector4i(worldX, worldY, worldZ - 1, startingLightValue - 1));
            // back neighbor
            neighbors.Enqueue(new Vector4i(worldX, worldY, worldZ + 1, startingLightValue - 1));
            while (neighbors.Count > 0)
            {
                Vector4i neighborPos = neighbors.Dequeue();
                if ((neighborPos.x < 0) || (neighborPos.y < 0) || (neighborPos.z < 0) ||
                        (neighborPos.x >= TotalBlocksX) || (neighborPos.y >= TotalBlocksY) || (neighborPos.z >= TotalBlocksZ))
                    continue;
                chunkX = (int)(neighborPos.x / Settings.ChunkSizeX);
                chunkY = (int)(neighborPos.y / Settings.ChunkSizeY);
                chunkZ = (int)(neighborPos.z / Settings.ChunkSizeZ);
                localX = (int)(neighborPos.x % Settings.ChunkSizeX);
                localY = (int)(neighborPos.y % Settings.ChunkSizeY);
                localZ = (int)(neighborPos.z % Settings.ChunkSizeZ);

                // DEBUG ONLY (blocks will never have ID of 0 under normal circumstances)
                if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].BlockTypeID == 0)
                    continue;
                // end debug only

                // light can't penetrate solid objects, so don't give it a light value
                if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].BlockTypeID != (byte)BlockID.Air)
                    continue;
                // if it's already more lit then don't light it with a darker value...
                if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].SunlightValue >= neighborPos.w)
                    continue;
                // this block passed the tests, give it the light value
                m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].SunlightValue = (byte)neighborPos.w;

                byte newLightIntensity = (byte)(neighborPos.w - 1);
                // if the new light value is 0 or less then we've run out of light to propagate
                // so don't even add the neighbors
                if (newLightIntensity <= 0)
                    continue;

                // we've still got some light left, so let's spread it to the neighbors
                // left neighbor
                neighbors.Enqueue(new Vector4i(neighborPos.x - 1, neighborPos.y, neighborPos.z, newLightIntensity));
                // right neighbor
                neighbors.Enqueue(new Vector4i(neighborPos.x + 1, neighborPos.y, neighborPos.z, newLightIntensity));
                // bottom neighbor
                neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y - 1, neighborPos.z, newLightIntensity));
                // top neighbor
                neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y + 1, neighborPos.z, newLightIntensity));
                // front neighbor
                neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y, neighborPos.z - 1, newLightIntensity));
                // back neighbor
                neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y, neighborPos.z + 1, newLightIntensity));

            }
        }

        public static void RemoveLightSourceAt(int worldX, int worldY, int worldZ, byte removedLightValue)
        {
            const int TotalBlocksX = Settings.NUM_CHUNKS * Settings.ChunkSizeX;
            const int TotalBlocksY = Settings.NUM_CHUNKS * Settings.ChunkSizeY;
            const int TotalBlocksZ = Settings.NUM_CHUNKS * Settings.ChunkSizeZ;

            int chunkX, chunkY, chunkZ, localX, localY, localZ;
            Chunk[,,] m_ActiveChunks = TerrainManager.Instance.m_ActiveChunks;

            Queue<Vector4i> neighbors = new Queue<Vector4i>();
            List<Vector4i> tempLightSources = new List<Vector4i>();

            // left neighbor
            neighbors.Enqueue(new Vector4i(worldX - 1, worldY, worldZ, removedLightValue - 1));
            // right neighbor
            neighbors.Enqueue(new Vector4i(worldX + 1, worldY, worldZ, removedLightValue - 1));
            // bottom neighbor
            neighbors.Enqueue(new Vector4i(worldX, worldY - 1, worldZ, removedLightValue - 1));
            // top neighbor
            neighbors.Enqueue(new Vector4i(worldX, worldY + 1, worldZ, removedLightValue - 1));
            // front neighbor
            neighbors.Enqueue(new Vector4i(worldX, worldY, worldZ - 1, removedLightValue - 1));
            // back neighbor
            neighbors.Enqueue(new Vector4i(worldX, worldY, worldZ + 1, removedLightValue - 1));
            while (neighbors.Count > 0)
            {
                Vector4i neighborPos = neighbors.Dequeue();
                if ((neighborPos.x < 0) || (neighborPos.y < 0) || (neighborPos.z < 0) ||
                        (neighborPos.x >= TotalBlocksX) || (neighborPos.y >= TotalBlocksY) || (neighborPos.z >= TotalBlocksZ))
                    continue;
                chunkX = (int)(neighborPos.x / Settings.ChunkSizeX);
                chunkY = (int)(neighborPos.y / Settings.ChunkSizeY);
                chunkZ = (int)(neighborPos.z / Settings.ChunkSizeZ);
                localX = (int)(neighborPos.x % Settings.ChunkSizeX);
                localY = (int)(neighborPos.y % Settings.ChunkSizeY);
                localZ = (int)(neighborPos.z % Settings.ChunkSizeZ);

                // DEBUG ONLY (blocks will never have ID of 0 under normal circumstances)
                if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].BlockTypeID == 0)
                    continue;
                // end debug only

                // light can't penetrate solid objects, so don't give it a light value
                if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].BlockTypeID != (byte)BlockID.Air)
                    continue;

                // if it has a higher light value then it's being lit by a different source, so add it for propagation later
                if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].SunlightValue >= neighborPos.w)
                {
                    tempLightSources.Add(new Vector4i(neighborPos.x, neighborPos.y, neighborPos.z, m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].SunlightValue));
                    // and we don't care about its neighbors anymore, so continue onward
                    continue;
                }

                // this block is (probably) being lit by this source, so set its light value to 0.
                // if it's being lit by multiple sources the value will be updated in a later step
                m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].SunlightValue = 0;

                byte newLightIntensity = (byte)(neighborPos.w - 1);
                // if the new light value is 0 or less then we've run out of light to propagate
                // so don't even add the neighbors
                if (newLightIntensity <= 0)
                    continue;

                // we've still got some light left, so let's spread it to the neighbors
                // left neighbor
                neighbors.Enqueue(new Vector4i(neighborPos.x - 1, neighborPos.y, neighborPos.z, neighborPos.w - 1));
                // right neighbor
                neighbors.Enqueue(new Vector4i(neighborPos.x + 1, neighborPos.y, neighborPos.z, neighborPos.w - 1));
                // bottom neighbor
                neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y - 1, neighborPos.z, neighborPos.w - 1));
                // top neighbor
                neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y + 1, neighborPos.z, neighborPos.w - 1));
                // front neighbor
                neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y, neighborPos.z - 1, neighborPos.w - 1));
                // back neighbor
                neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y, neighborPos.z + 1, neighborPos.w - 1));

            }

            foreach (Vector4i lightSource in tempLightSources)
            {
                // left neighbor
                neighbors.Enqueue(new Vector4i(lightSource.x - 1, lightSource.y, lightSource.z, lightSource.w - 1));
                // right neighbor
                neighbors.Enqueue(new Vector4i(lightSource.x + 1, lightSource.y, lightSource.z, lightSource.w - 1));
                // bottom neighbor
                neighbors.Enqueue(new Vector4i(lightSource.x, lightSource.y - 1, lightSource.z, lightSource.w - 1));
                // top neighbor
                neighbors.Enqueue(new Vector4i(lightSource.x, lightSource.y + 1, lightSource.z, lightSource.w - 1));
                // front neighbor
                neighbors.Enqueue(new Vector4i(lightSource.x, lightSource.y, lightSource.z - 1, lightSource.w - 1));
                // back neighbor
                neighbors.Enqueue(new Vector4i(lightSource.x, lightSource.y, lightSource.z + 1, lightSource.w - 1));
                while (neighbors.Count > 0)
                {
                    Vector4i neighborPos = neighbors.Dequeue();
                    if ((neighborPos.x < 0) || (neighborPos.y < 0) || (neighborPos.z < 0) ||
                            (neighborPos.x >= TotalBlocksX) || (neighborPos.y >= TotalBlocksY) || (neighborPos.z >= TotalBlocksZ))
                        continue;
                    chunkX = (int)(neighborPos.x / Settings.ChunkSizeX);
                    chunkY = (int)(neighborPos.y / Settings.ChunkSizeY);
                    chunkZ = (int)(neighborPos.z / Settings.ChunkSizeZ);
                    localX = (int)(neighborPos.x % Settings.ChunkSizeX);
                    localY = (int)(neighborPos.y % Settings.ChunkSizeY);
                    localZ = (int)(neighborPos.z % Settings.ChunkSizeZ);

                    // DEBUG ONLY (blocks will never have ID of 0 under normal circumstances)
                    if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].BlockTypeID == 0)
                        continue;
                    // end debug only

                    // light can't penetrate solid objects, so don't give it a light value
                    if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].BlockTypeID != (byte)BlockID.Air)
                        continue;
                    // if it's already more lit then don't light it with a darker value...
                    if (m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].SunlightValue >= neighborPos.w)
                        continue;
                    // this block passed the tests, give it the light value
                    m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ].SunlightValue = (byte)neighborPos.w;

                    byte newLightIntensity = (byte)(neighborPos.w - 1);
                    // if the new light value is 0 or less then we've run out of light to propagate
                    // so don't even add the neighbors
                    if (newLightIntensity <= 0)
                        continue;

                    // we've still got some light left, so let's spread it to the neighbors
                    // left neighbor
                    neighbors.Enqueue(new Vector4i(neighborPos.x - 1, neighborPos.y, neighborPos.z, neighborPos.w - 1));
                    // right neighbor
                    neighbors.Enqueue(new Vector4i(neighborPos.x + 1, neighborPos.y, neighborPos.z, neighborPos.w - 1));
                    // bottom neighbor
                    neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y - 1, neighborPos.z, neighborPos.w - 1));
                    // top neighbor
                    neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y + 1, neighborPos.z, neighborPos.w - 1));
                    // front neighbor
                    neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y, neighborPos.z - 1, neighborPos.w - 1));
                    // back neighbor
                    neighbors.Enqueue(new Vector4i(neighborPos.x, neighborPos.y, neighborPos.z + 1, neighborPos.w - 1));
                }
            }
        }
    }
}
