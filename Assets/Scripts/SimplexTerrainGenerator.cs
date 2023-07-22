using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utilities;

namespace VoxelPrototype.TerrainGenerators
{
    public class SimplexTerrainGenerator
    {
        static ChunkNoise m_ChunkNoise = new ChunkNoise(seed: 1361341);
        static float[,] m_HeightMap = new float[Settings.ChunkSizeX, Settings.ChunkSizeX];

        public static void Generate(int chunkX, int chunkZ)
        {
            TerrainManager terrainManager = TerrainManager.Instance;
            Chunk[,,] m_ActiveChunks = terrainManager.m_ActiveChunks;
            Block[,,] blocks = m_ActiveChunks[chunkX, 0, chunkZ].Blocks;
            int localX, localY, localZ;
            int worldX, worldY, worldZ;
            int curChunkY = 0;
            bool hasGrassTop, hasBlockedSunlight;
            int height;
            int topmostSolidBlock;
            byte sunlightValue = Settings.SUNLIGHT_VALUE;

            // Calculate heightmap
            m_ChunkNoise.FillMap2D(m_HeightMap, chunkX, chunkZ, octaves: 5, startFrequency: .03f, startAmplitude: 5);

            // Fill chunk with blocks
            for (localX = 0; localX < Settings.ChunkSizeX; localX++)
            {
                for (localZ = 0; localZ < Settings.ChunkSizeX; localZ++)
                {
                    // Create ground
                    height = Mathf.RoundToInt(Settings.SEA_LEVEL + m_HeightMap[localX, localZ]);
                    curChunkY = 0;
                    blocks = terrainManager.m_ActiveChunks[chunkX, curChunkY, chunkZ].Blocks;
                    for (worldY = 0, localY = 0; worldY < height; worldY++, localY++)
                    {
                        if (localY == Settings.ChunkSizeY)
                        {
                            localY = 0;
                            curChunkY = (worldY / Settings.ChunkSizeY);
                            blocks = m_ActiveChunks[chunkX, curChunkY, chunkZ].Blocks;
                        }
                        terrainManager.PlaceBlock(chunkX, curChunkY, chunkZ, localX, localY, localZ, BlockID.Stone, true);
                    }

                    // Create mountains
                    worldX = localX + chunkX * Settings.ChunkSizeX;
                    worldY = height;
                    worldZ = localZ + chunkZ * Settings.ChunkSizeX;
                    hasGrassTop = false;
                    hasBlockedSunlight = false;
                    topmostSolidBlock = height;
                    curChunkY = (worldY / Settings.ChunkSizeY);
                    blocks = m_ActiveChunks[chunkX, (worldY / Settings.ChunkSizeY), chunkZ].Blocks;

                    for (worldY = height, localY = (height % Settings.ChunkSizeY); worldY < Settings.WORLD_HEIGHT; worldY++, localY++)
                    {
                        if (localY == Settings.ChunkSizeY)
                        {
                            localY = (worldY % Settings.ChunkSizeY);
                            curChunkY = (worldY / Settings.ChunkSizeY);
                            blocks = m_ActiveChunks[chunkX, curChunkY, chunkZ].Blocks;
                        }
                        float noiseValue3D = m_ChunkNoise.GetValue3D(worldX, worldY, worldZ, octaves: 6, startFrequency: .05f, startAmplitude: 1);
                        if (noiseValue3D > 0)
                        {
                            if (worldY == Settings.WORLD_HEIGHT - 1)
                            {
                                terrainManager.PlaceBlock(chunkX, curChunkY, chunkZ, localX, (worldY % Settings.ChunkSizeY), localZ, BlockID.Grass, true);
                            }
                            else
                            {
                                terrainManager.PlaceBlock(chunkX, curChunkY, chunkZ, localX, (worldY % Settings.ChunkSizeY), localZ, BlockID.Dirt, true);
                            }
                            topmostSolidBlock = worldY;
                        }
                        else
                        {
                            terrainManager.PlaceBlock(chunkX, curChunkY, chunkZ, localX, (worldY % Settings.ChunkSizeY), localZ, BlockID.Air, true);
                        }
                    }
                    curChunkY = (topmostSolidBlock / Settings.ChunkSizeY);
                    blocks = m_ActiveChunks[chunkX, curChunkY, chunkZ].Blocks;
                    localY = topmostSolidBlock % Settings.ChunkSizeY;
                    // ensure the topmost solid block is grass. aesthetic upgrade.
                    // BUG: adds grass block on top of air sometimes
                    terrainManager.PlaceBlock(chunkX, curChunkY, chunkZ, localX, localY, localZ, BlockID.Grass, true);
                    // used for lighting. mark the topmost solid block as the one that blocks sunlight for every block beneath it.
                    // this will simplify regeneration when that block is destroyed
                    blocks[localX, localY, localZ].IsBlockingColumnSunlight = true;
                    topmostSolidBlock = topmostSolidBlock + 1;
                    if (topmostSolidBlock >= Settings.WORLD_HEIGHT)
                        continue;
                    curChunkY = (topmostSolidBlock / Settings.ChunkSizeY);
                    blocks = m_ActiveChunks[chunkX, curChunkY, chunkZ].Blocks;
                    localY = topmostSolidBlock % Settings.ChunkSizeY;
                    // now go through all the air blocks and set their light values to 15
                    for (worldY = topmostSolidBlock, localY = (topmostSolidBlock % Settings.ChunkSizeY); worldY < Settings.WORLD_HEIGHT; worldY++, localY++)
                    {
                        if (localY == Settings.ChunkSizeY)
                        {
                            localY = (worldY % Settings.ChunkSizeY);
                            curChunkY = (worldY / Settings.ChunkSizeY);
                            blocks = m_ActiveChunks[chunkX, curChunkY, chunkZ].Blocks;
                        }
                        // NOTE:
                        // should not be necessary. remove once everything works.
                        if (blocks[localX, localY, localZ].BlockTypeID != (byte)BlockID.Air) continue;
                        blocks[localX, localY, localZ].SunlightValue = Settings.SUNLIGHT_VALUE;
                        // we also want the chunk to be aware of the positions of each sunlight block
                        // this, again, will speed up regeneration of lighting data
                        m_ActiveChunks[chunkX, curChunkY, chunkZ].SunlightBlocks.Add(new Vector3i(worldX, worldY, worldZ));
                    }
                }
            }
        }
    }
}
