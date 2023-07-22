using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VoxelPrototype.Lighting;

namespace VoxelPrototype.MeshGenerators
{
    public class NaiveMeshGenerator
    {
        // WARNING:
        // Everything to do with this variable is debug only!
        // Remove once raycasting works again with centered voxels
        // Centered voxels should be DEFAULT (-0.5f and +0.5f)
        public static bool bCenterMesh = false;

        public static bool GenerateMesh(Chunk chunk, out Mesh opaqueMesh, out Mesh transparentMesh, out Mesh waterMesh)
        {

            Block[,,] blocks = chunk.blocks;
            int chunkPosX = chunk.ChunkPosX, chunkPosY = chunk.ChunkPosY, chunkPosZ = chunk.ChunkPosZ;
            int worldX, worldY, worldZ;

            List<Vector3> opaqueVerts = new List<Vector3>(1024);
            List<int> opaqueIndices = new List<int>(1536);
            List<Vector3> opaqueUVs = new List<Vector3>(1024);
            List<Vector2> opaqueLightData = new List<Vector2>(1024);
            List<Vector3> opaqueVoxelPositions = new List<Vector3>(1024);

            List<Vector3> transparentVerts = new List<Vector3>(1024);
            List<int> transparentIndices = new List<int>(1536);
            List<Vector3> transparentUVs = new List<Vector3>(1024);
            List<Vector2> transparentLightData = new List<Vector2>(1024);
            List<Vector3> transparentVoxelPositions = new List<Vector3>(1024);

            List<Vector3> fluidVerts = new List<Vector3>(1024);
            List<int> fluidIndices = new List<int>(1536);
            List<Vector3> fluidUVs = new List<Vector3>(1024);
            List<Vector2> fluidLightData = new List<Vector2>(1024);
            List<Vector3> fluidVoxelPositions = new List<Vector3>(1024);

            List<Vector3> workingVerts;
            List<int> workingIndices;
            List<Vector3> workingUVs;
            List<Vector2> workingLightData;
            List<Vector3> workingVoxelPositions;
            int numExistingQuads = 0;

            byte[] vertexLightData;
            float fluidDepth = 0f;

            for (int x = 0; x < Settings.ChunkSizeX; x++)
            {
                for (int y = 0; y < Settings.ChunkSizeY; y++)
                {
                    for (int z = 0; z < Settings.ChunkSizeZ; z++)
                    {
                        IBlockType curBlock = BlockTypes.GetBlockType(blocks[x, y, z].BlockTypeID);
                        if (curBlock.IsAir)
                        {
                            // can't render air :)
                            continue;
                        }
                        if (curBlock.IsFluid)
                        {
                            workingVerts = fluidVerts;
                            workingIndices = fluidIndices;
                            workingUVs = fluidUVs;
                            workingLightData = fluidLightData;
                            workingVoxelPositions = fluidVoxelPositions;

                            fluidDepth = blocks[x, y, z].ExtraData & 15;
                        }
                        else if (curBlock.IsTransparent)
                        {
                            workingVerts = transparentVerts;
                            workingIndices = transparentIndices;
                            workingUVs = transparentUVs;
                            workingLightData = transparentLightData;
                            workingVoxelPositions = transparentVoxelPositions;
                        }
                        else
                        {
                            workingVerts = opaqueVerts;
                            workingIndices = opaqueIndices;
                            workingUVs = opaqueUVs;
                            workingLightData = opaqueLightData;
                            workingVoxelPositions = opaqueVoxelPositions;
                        }
                        worldX = (chunkPosX * Settings.ChunkSizeX) + x;
                        worldY = (chunkPosY * Settings.ChunkSizeY) + y;
                        worldZ = (chunkPosZ * Settings.ChunkSizeZ) + z;
                        // backface is determined like so:
                        // if we are looking down the POSITIVE axis (toward +) are we seeing that face?
                        // example: if we position at 0,0,0 and we look down the positive Z axis (such that we see 0,0,1 in front of us)
                        // then we are not seeing the NORTH face of 0,0,1, we are seeing the SOUTH face. so, NORTH is a backface.
                        if (blocks[x, y, z].IsVisible((byte)Direction.North))
                        {
                            numExistingQuads = workingVerts.Count / 4;
                            if (bCenterMesh)
                            {
                                // TO-DO:
                                // Factor in fluid depth when dealing with fluid blocks (only for bCenterMesh)
                                // top left
                                workingVerts.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                                // top right
                                workingVerts.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
                                // bottom right
                                workingVerts.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
                                // bottom left
                                workingVerts.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
                            }
                            else
                            {
                                if (curBlock.IsFluid)
                                {
                                    // top left
                                    workingVerts.Add(new Vector3(x + 1f, y + 1f, z + 1f));
                                    // top right
                                    workingVerts.Add(new Vector3(x, y + 1f, z + 1f));
                                }
                                else
                                {
                                    // top left
                                    workingVerts.Add(new Vector3(x + 1f, y + 1f, z + 1f));
                                    // top right
                                    workingVerts.Add(new Vector3(x, y + 1f, z + 1f));
                                }
                                // bottom right
                                workingVerts.Add(new Vector3(x, y, z + 1f));
                                // bottom left
                                workingVerts.Add(new Vector3(x + 1f, y, z + 1f));
                            }

                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));

                            workingIndices.Add(0 + (4 * numExistingQuads));
                            workingIndices.Add(2 + (4 * numExistingQuads));
                            workingIndices.Add(3 + (4 * numExistingQuads));
                            //
                            workingIndices.Add(0 + (4 * numExistingQuads));
                            workingIndices.Add(1 + (4 * numExistingQuads));
                            workingIndices.Add(2 + (4 * numExistingQuads));

                            workingUVs.Add(new Vector3(0f, 1f, curBlock.GetTextureLayer(Direction.North)));
                            workingUVs.Add(new Vector3(1f, 1f, curBlock.GetTextureLayer(Direction.North)));
                            workingUVs.Add(new Vector3(1f, 0f, curBlock.GetTextureLayer(Direction.North)));
                            workingUVs.Add(new Vector3(0f, 0f, curBlock.GetTextureLayer(Direction.North)));
                            if (LightProcessor.GetVertexLightingForFace(worldX, worldY, worldZ, Direction.North, out vertexLightData))
                            {
                                // add in order: top left, top right, bottom right, bottom left
                                workingLightData.Add(new Vector2(((float)vertexLightData[0] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[1] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[2] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[3] / Settings.SUNLIGHT_VALUE), 0f));
                            }
                        }
                        if (blocks[x, y, z].IsVisible((byte)Direction.South))
                        {
                            numExistingQuads = workingVerts.Count / 4;
                            if (bCenterMesh)
                            {
                                // top left
                                workingVerts.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
                                // top right
                                workingVerts.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
                                // bottom right
                                workingVerts.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
                                // bottom left
                                workingVerts.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
                            }
                            else
                            {
                                // top left
                                workingVerts.Add(new Vector3(x, y + 1f, z));
                                // top right
                                workingVerts.Add(new Vector3(x + 1f, y + 1f, z));
                                // bottom right
                                workingVerts.Add(new Vector3(x + 1f, y, z));
                                // bottom left
                                workingVerts.Add(new Vector3(x, y, z));
                            }

                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));

                            workingIndices.Add(0 + (4 * numExistingQuads));
                            workingIndices.Add(2 + (4 * numExistingQuads));
                            workingIndices.Add(3 + (4 * numExistingQuads));
                            //
                            workingIndices.Add(0 + (4 * numExistingQuads));
                            workingIndices.Add(1 + (4 * numExistingQuads));
                            workingIndices.Add(2 + (4 * numExistingQuads));

                            workingUVs.Add(new Vector3(0f, 1f, curBlock.GetTextureLayer(Direction.South)));
                            workingUVs.Add(new Vector3(1f, 1f, curBlock.GetTextureLayer(Direction.South)));
                            workingUVs.Add(new Vector3(1f, 0f, curBlock.GetTextureLayer(Direction.South)));
                            workingUVs.Add(new Vector3(0f, 0f, curBlock.GetTextureLayer(Direction.South)));
                            if (LightProcessor.GetVertexLightingForFace(worldX, worldY, worldZ, Direction.South, out vertexLightData))
                            {
                                // add in order: top left, top right, bottom right, bottom left
                                workingLightData.Add(new Vector2(((float)vertexLightData[1] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[0] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[3] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[2] / Settings.SUNLIGHT_VALUE), 0f));
                            }
                        }
                        if (blocks[x, y, z].IsVisible((byte)Direction.East))
                        {
                            numExistingQuads = workingVerts.Count / 4;
                            if (bCenterMesh)
                            {
                                // top left
                                workingVerts.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
                                // top right
                                workingVerts.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                                // bottom right
                                workingVerts.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
                                // bottom left
                                workingVerts.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
                            }
                            else
                            {
                                // top left
                                workingVerts.Add(new Vector3(x + 1f, y + 1f, z));
                                // top right
                                workingVerts.Add(new Vector3(x + 1f, y + 1f, z + 1f));
                                // bottom right
                                workingVerts.Add(new Vector3(x + 1f, y, z + 1f));
                                // bottom left
                                workingVerts.Add(new Vector3(x + 1f, y, z));
                            }

                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));

                            workingIndices.Add(0 + (4 * numExistingQuads));
                            workingIndices.Add(2 + (4 * numExistingQuads));
                            workingIndices.Add(3 + (4 * numExistingQuads));
                            //
                            workingIndices.Add(0 + (4 * numExistingQuads));
                            workingIndices.Add(1 + (4 * numExistingQuads));
                            workingIndices.Add(2 + (4 * numExistingQuads));

                            workingUVs.Add(new Vector3(0f, 1f, curBlock.GetTextureLayer(Direction.East)));
                            workingUVs.Add(new Vector3(1f, 1f, curBlock.GetTextureLayer(Direction.East)));
                            workingUVs.Add(new Vector3(1f, 0f, curBlock.GetTextureLayer(Direction.East)));
                            workingUVs.Add(new Vector3(0f, 0f, curBlock.GetTextureLayer(Direction.East)));
                            if (LightProcessor.GetVertexLightingForFace(worldX, worldY, worldZ, Direction.East, out vertexLightData))
                            {
                                // add in order: top left, top right, bottom right, bottom left
                                workingLightData.Add(new Vector2(((float)vertexLightData[0] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[1] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[2] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[3] / Settings.SUNLIGHT_VALUE), 0f));
                            }
                        }
                        if (blocks[x, y, z].IsVisible((byte)Direction.West))
                        {
                            numExistingQuads = workingVerts.Count / 4;
                            if (bCenterMesh)
                            {
                                // top left
                                workingVerts.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
                                // top right
                                workingVerts.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
                                // bottom right
                                workingVerts.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
                                // bottom left
                                workingVerts.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
                            }
                            else
                            {
                                // top left
                                workingVerts.Add(new Vector3(x, y + 1f, z + 1f));
                                // top right
                                workingVerts.Add(new Vector3(x, y + 1f, z));
                                // bottom right
                                workingVerts.Add(new Vector3(x, y, z));
                                // bottom left
                                workingVerts.Add(new Vector3(x, y, z + 1f));
                            }

                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));

                            workingIndices.Add(0 + (4 * numExistingQuads));
                            workingIndices.Add(2 + (4 * numExistingQuads));
                            workingIndices.Add(3 + (4 * numExistingQuads));
                            //
                            workingIndices.Add(0 + (4 * numExistingQuads));
                            workingIndices.Add(1 + (4 * numExistingQuads));
                            workingIndices.Add(2 + (4 * numExistingQuads));

                            workingUVs.Add(new Vector3(0f, 1f, curBlock.GetTextureLayer(Direction.West)));
                            workingUVs.Add(new Vector3(1f, 1f, curBlock.GetTextureLayer(Direction.West)));
                            workingUVs.Add(new Vector3(1f, 0f, curBlock.GetTextureLayer(Direction.West)));
                            workingUVs.Add(new Vector3(0f, 0f, curBlock.GetTextureLayer(Direction.West)));
                            if (LightProcessor.GetVertexLightingForFace(worldX, worldY, worldZ, Direction.West, out vertexLightData))
                            {
                                // add in order: top left, top right, bottom right, bottom left
                                workingLightData.Add(new Vector2(((float)vertexLightData[0] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[1] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[2] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[3] / Settings.SUNLIGHT_VALUE), 0f));
                            }
                        }
                        if (blocks[x, y, z].IsVisible((byte)Direction.Up))
                        {
                            numExistingQuads = workingVerts.Count / 4;
                            if (bCenterMesh)
                            {
                                // top left
                                workingVerts.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
                                // top right
                                workingVerts.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                                // bottom right
                                workingVerts.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
                                // bottom left
                                workingVerts.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
                            }
                            else
                            {
                                // top left
                                workingVerts.Add(new Vector3(x, y + 1f, z + 1f));
                                // top right
                                workingVerts.Add(new Vector3(x + 1f, y + 1f, z + 1f));
                                // bottom right
                                workingVerts.Add(new Vector3(x + 1f, y + 1f, z));
                                // bottom left
                                workingVerts.Add(new Vector3(x, y + 1f, z));
                            }

                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));

                            workingIndices.Add(0 + (4 * numExistingQuads));
                            workingIndices.Add(2 + (4 * numExistingQuads));
                            workingIndices.Add(3 + (4 * numExistingQuads));
                            //
                            workingIndices.Add(0 + (4 * numExistingQuads));
                            workingIndices.Add(1 + (4 * numExistingQuads));
                            workingIndices.Add(2 + (4 * numExistingQuads));

                            workingUVs.Add(new Vector3(0f, 1f, curBlock.GetTextureLayer(Direction.Up)));
                            workingUVs.Add(new Vector3(1f, 1f, curBlock.GetTextureLayer(Direction.Up)));
                            workingUVs.Add(new Vector3(1f, 0f, curBlock.GetTextureLayer(Direction.Up)));
                            workingUVs.Add(new Vector3(0f, 0f, curBlock.GetTextureLayer(Direction.Up)));
                            if (LightProcessor.GetVertexLightingForFace(worldX, worldY, worldZ, Direction.Up, out vertexLightData))
                            {
                                // add in order: top left, top right, bottom right, bottom left
                                workingLightData.Add(new Vector2(((float)vertexLightData[3] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[2] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[1] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[0] / Settings.SUNLIGHT_VALUE), 0f));
                            }
                        }
                        if (blocks[x, y, z].IsVisible((byte)Direction.Down))
                        {
                            numExistingQuads = workingVerts.Count / 4;
                            if (bCenterMesh)
                            {
                                // top left
                                workingVerts.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
                                // top right
                                workingVerts.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
                                // bottom right
                                workingVerts.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
                                // bottom left
                                workingVerts.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
                            }
                            else
                            {
                                // top left
                                workingVerts.Add(new Vector3(x, y, z));
                                // top right
                                workingVerts.Add(new Vector3(x + 1f, y, z));
                                // bottom right
                                workingVerts.Add(new Vector3(x + 1f, y, z + 1f));
                                // bottom left
                                workingVerts.Add(new Vector3(x, y, z + 1f));
                            }

                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));
                            workingVoxelPositions.Add(new Vector3(worldX, worldY, worldZ));

                            workingIndices.Add(0 + (4 * numExistingQuads));
                            workingIndices.Add(2 + (4 * numExistingQuads));
                            workingIndices.Add(3 + (4 * numExistingQuads));
                            //
                            workingIndices.Add(0 + (4 * numExistingQuads));
                            workingIndices.Add(1 + (4 * numExistingQuads));
                            workingIndices.Add(2 + (4 * numExistingQuads));

                            workingUVs.Add(new Vector3(0f, 1f, curBlock.GetTextureLayer(Direction.Down)));
                            workingUVs.Add(new Vector3(1f, 1f, curBlock.GetTextureLayer(Direction.Down)));
                            workingUVs.Add(new Vector3(1f, 0f, curBlock.GetTextureLayer(Direction.Down)));
                            workingUVs.Add(new Vector3(0f, 0f, curBlock.GetTextureLayer(Direction.Down)));
                            if (LightProcessor.GetVertexLightingForFace(worldX, worldY, worldZ, Direction.Down, out vertexLightData))
                            {
                                // add in order: top left, top right, bottom right, bottom left
                                workingLightData.Add(new Vector2(((float)vertexLightData[0] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[1] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[2] / Settings.SUNLIGHT_VALUE), 0f));
                                workingLightData.Add(new Vector2(((float)vertexLightData[3] / Settings.SUNLIGHT_VALUE), 0f));
                            }
                        }
                    }
                }
            }

            //Debug.Log(string.Format("Chunk[{0},{1},{2}] verts: {3}, indices: {4}, uvs: {5}, lightData: {6}", ChunkPosX, ChunkPosY, ChunkPosZ, verts.Count, indices.Count, uvs.Count, lightData.Count));
            //Debug.Log(string.Format("verts: {0}, indices: {1}, uvs: {2}, lightData: {3}", verts.Count, indices.Count, uvs.Count, lightData.Count));
            opaqueMesh = new Mesh();
            opaqueMesh.SetVertices(opaqueVerts);
            opaqueMesh.SetTriangles(opaqueIndices, 0);
            opaqueMesh.SetUVs(0, opaqueUVs);
            opaqueMesh.SetUVs(1, opaqueLightData);
            opaqueMesh.SetUVs(2, opaqueVoxelPositions);

            transparentMesh = new Mesh();
            transparentMesh.SetVertices(transparentVerts);
            transparentMesh.SetTriangles(transparentIndices, 0);
            transparentMesh.SetUVs(0, transparentUVs);
            transparentMesh.SetUVs(1, transparentLightData);
            transparentMesh.SetUVs(2, transparentVoxelPositions);

            waterMesh = new Mesh();
            waterMesh.SetVertices(fluidVerts);
            waterMesh.SetTriangles(fluidIndices, 0);
            waterMesh.SetUVs(0, fluidUVs);
            waterMesh.SetUVs(1, fluidLightData);
            waterMesh.SetUVs(2, fluidVoxelPositions);
            return true;
        }
    }
}
