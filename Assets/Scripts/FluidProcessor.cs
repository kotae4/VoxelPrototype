using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace VoxelPrototype.Fluids
{
    public enum EFlowDirection : byte
    {
        Down = 0,
        North = 1,
        East = 2,
        NorthEast = 3,
        South = 4,
        NONE_Z = 5,
        SouthEast = 6,
        West = 8,
        NorthWest = 9,
        NONE_X = 10,
        SouthWest = 12,
        ALL = 15,
    };
    public class FluidProcessor
    {
        // fluid is kind of similar to lighting in its propagation
        // we basically need to "spread" to each surrounding neighbor until we run out of fluid. each time we spread, the neighbor gets our fluid minus one
        // for aesthetic purposes, we add an additional first step: we look 5 blocks in each direction for an air block. if an air block is found we spread that direction only.
        // this creates a smooth-looking waterfall
        // furthermore: we never look at the neighbor above us

        class FluidSim_Tick
        {
            public Vector3i Origin;
            public Queue<FluidSim_TickData> Neighbors = new Queue<FluidSim_TickData>(256);
            public bool isRemoval = false;
        }

        struct FluidSim_TickData
        {
            public int x, y, z;
            public EFlowDirection FlowDirection;
            public byte Depth;

            public FluidSim_TickData(int X, int Y, int Z)
            {
                x = X;
                y = Y;
                z = Z;

                FlowDirection = EFlowDirection.Down;
                Depth = 0;
            }
            public FluidSim_TickData(int X, int Y, int Z, EFlowDirection dir, byte depth)
            {
                x = X;
                y = Y;
                z = Z;

                FlowDirection = dir;
                Depth = depth;
            }
        }

        List<FluidSim_Tick> m_Ticks = new List<FluidSim_Tick>();
        bool needsMeshUpdate = false;

        public void PropagateFluidFrom(int worldX, int worldY, int worldZ)
        {
            ByteVec3 originChunkPos, originLocalPos;
            if (!TerrainManager.Instance.GetChunkLocalFromWorld(worldX, worldY, worldZ, out originChunkPos, out originLocalPos))
            {
                UnityEngine.Debug.LogError("[ERROR] Could not get block from world pos. Can't add source!");
                return;
            }
            Block targetBlock = TerrainManager.Instance.m_ActiveChunks[originChunkPos.X, originChunkPos.Y, originChunkPos.Z].blocks[originLocalPos.X, originLocalPos.Y, originLocalPos.Z];
            // POTENTIAL BUG:
            // should we check that the placement is valid here or should the caller?
            //IBlockType targetType = BlockTypes.GetBlockType(targetBlock.BlockTypeID);
            UnityEngine.Debug.Log("[Fluid] Placing source block via TerrainManager");
            TerrainManager.Instance.m_ActiveChunks[originChunkPos.X, originChunkPos.Y, originChunkPos.Z].blocks[originLocalPos.X, originLocalPos.Y, originLocalPos.Z].ExtraData |= (byte)15;
            TerrainManager.Instance.PlaceBlock(originChunkPos.X, originChunkPos.Y, originChunkPos.Z, originLocalPos.X, originLocalPos.Y, originLocalPos.Z, BlockID.Water);

            FluidSim_Tick newTick = new FluidSim_Tick();
            newTick.isRemoval = false;
            newTick.Origin = new Vector3i(worldX, worldY, worldZ);

            const int TotalBlocksX = Settings.NUM_CHUNKS * Settings.ChunkSizeX;
            const int TotalBlocksZ = Settings.NUM_CHUNKS * Settings.ChunkSizeZ;

            // right
            if ((worldX + 1) < TotalBlocksX)
                newTick.Neighbors.Enqueue(new FluidSim_TickData(worldX + 1, worldY, worldZ, EFlowDirection.East, 15));
            // left
            if ((worldX - 1) >= 0)
                newTick.Neighbors.Enqueue(new FluidSim_TickData(worldX - 1, worldY, worldZ, EFlowDirection.West, 15));
            // forward
            if ((worldZ + 1) < TotalBlocksZ)
                newTick.Neighbors.Enqueue(new FluidSim_TickData(worldX, worldY, worldZ + 1, EFlowDirection.North, 15));
            // backward
            if ((worldZ - 1) >= 0)
                newTick.Neighbors.Enqueue(new FluidSim_TickData(worldX, worldY, worldZ - 1, EFlowDirection.South, 15));
            // down
            if ((worldY - 1) >= 0)
                newTick.Neighbors.Enqueue(new FluidSim_TickData(worldX, worldY - 1, worldZ, EFlowDirection.Down, 15));

            UnityEngine.Debug.Log("[Fluid] Adding new source tick operation");
            m_Ticks.Add(newTick);
            needsMeshUpdate = true;
        }

        public void RemoveFluidAt(int worldX, int worldY, int worldZ, BlockID newBlock = BlockID.Air)
        {
            ByteVec3 originChunkPos, originLocalPos;
            if (!TerrainManager.Instance.GetChunkLocalFromWorld(worldX, worldY, worldZ, out originChunkPos, out originLocalPos))
            {
                UnityEngine.Debug.LogError("[ERROR] Could not get block from world pos. Can't remove fluid!");
                return;
            }
            if (!BlockTypes.GetBlockType(TerrainManager.Instance.m_ActiveChunks[originChunkPos.X, originChunkPos.Y, originChunkPos.Z].blocks[originLocalPos.X, originLocalPos.Y, originLocalPos.Z].BlockTypeID).IsFluid)
            {
                UnityEngine.Debug.LogError("[ERROR] Attempted to remove non-fluid block via FluidProcessor");
                return;
            }
            TerrainManager.Instance.m_ActiveChunks[originChunkPos.X, originChunkPos.Y, originChunkPos.Z].blocks[originLocalPos.X, originLocalPos.Y, originLocalPos.Z].ExtraData = 0;
            UnityEngine.Debug.Log("[Fluid] Replacing source block with new block via TerrainManager");
            TerrainManager.Instance.PlaceBlock(originChunkPos.X, originChunkPos.Y, originChunkPos.Z, originLocalPos.X, originLocalPos.Y, originLocalPos.Z, newBlock);

            FluidSim_Tick newTick = new FluidSim_Tick();
            newTick.isRemoval = true;
            newTick.Origin = new Vector3i(worldX, worldY, worldZ);

            const int TotalBlocksX = Settings.NUM_CHUNKS * Settings.ChunkSizeX;
            const int TotalBlocksZ = Settings.NUM_CHUNKS * Settings.ChunkSizeZ;

            // right
            if ((worldX + 1) < TotalBlocksX)
                newTick.Neighbors.Enqueue(new FluidSim_TickData(worldX + 1, worldY, worldZ, EFlowDirection.East, 0));
            // left
            if ((worldX - 1) >= 0)
                newTick.Neighbors.Enqueue(new FluidSim_TickData(worldX - 1, worldY, worldZ, EFlowDirection.West, 0));
            // forward
            if ((worldZ + 1) < TotalBlocksZ)
                newTick.Neighbors.Enqueue(new FluidSim_TickData(worldX, worldY, worldZ + 1, EFlowDirection.North, 0));
            // backward
            if ((worldZ - 1) >= 0)
                newTick.Neighbors.Enqueue(new FluidSim_TickData(worldX, worldY, worldZ - 1, EFlowDirection.South, 0));
            // down
            if ((worldY - 1) >= 0)
                newTick.Neighbors.Enqueue(new FluidSim_TickData(worldX, worldY - 1, worldZ, EFlowDirection.Down, 0));

            UnityEngine.Debug.Log("[Fluid] Adding new removal tick operation");
            m_Ticks.Add(newTick);
            needsMeshUpdate = true;
        }

        public void UpdateFluidSim()
        {
            //UnityEngine.Debug.Log("[Fluid] Updating sim!");
            const int TotalBlocksX = Settings.NUM_CHUNKS * Settings.ChunkSizeX;
            const int TotalBlocksY = Settings.NUM_CHUNKS * Settings.ChunkSizeY;
            const int TotalBlocksZ = Settings.NUM_CHUNKS * Settings.ChunkSizeZ;

            Chunk[,,] m_ActiveChunks = TerrainManager.Instance.m_ActiveChunks;
            int chunkX, chunkY, chunkZ;
            int localX, localY, localZ;

            List<Vector3i> uniqueChunks = new List<Vector3i>(16);

            for (int index = 0; index < m_Ticks.Count; index++)
            {
                FluidSim_Tick tick = m_Ticks[index];
                ByteVec3 originChunkPos, originLocalPos;
                if (!TerrainManager.Instance.GetChunkLocalFromWorld(tick.Origin.x, tick.Origin.y, tick.Origin.z, out originChunkPos, out originLocalPos))
                {
                    UnityEngine.Debug.LogError("[ERROR] Could not get block from fluid origin. Skipping this tick operation!");
                    continue;
                }
                byte depth = (byte)(m_ActiveChunks[originChunkPos.X, originChunkPos.Y, originChunkPos.Z].blocks[originLocalPos.X, originLocalPos.Y, originLocalPos.Z].ExtraData & 15);
                if (!tick.isRemoval)
                {
                    UnityEngine.Debug.Log("[Fluid] Processing non-removal tick");
                    // we are adding fluids
                    if (depth == 0)
                    {
                        m_Ticks.RemoveAt(index);
                        index--;
                        continue;
                    }
                    int curNeighbors = tick.Neighbors.Count, processedNeighbors = 0;
                    if (curNeighbors == 0)
                    {
                        m_Ticks.RemoveAt(index);
                        index--;
                        continue;
                    }
                    while (processedNeighbors <= curNeighbors)
                    {
                        FluidSim_TickData neighborData;
                        neighborData = tick.Neighbors.Dequeue();
                        processedNeighbors++;

                        // propagate to each neighbor by setting their depth value to curDepth - 1
                        if ((neighborData.x < 0) || (neighborData.y < 0) || (neighborData.z < 0) ||
                            (neighborData.x >= TotalBlocksX) || (neighborData.y >= TotalBlocksY) || (neighborData.z >= TotalBlocksZ))
                            continue;
                        chunkX = (int)(neighborData.x / Settings.ChunkSizeX);
                        chunkY = (int)(neighborData.y / Settings.ChunkSizeY);
                        chunkZ = (int)(neighborData.z / Settings.ChunkSizeZ);
                        localX = (int)(neighborData.x % Settings.ChunkSizeX);
                        localY = (int)(neighborData.y % Settings.ChunkSizeY);
                        localZ = (int)(neighborData.z % Settings.ChunkSizeZ);

                        Vector3i chunkCoord = new Vector3i(chunkX, chunkY, chunkZ);
                        if (!uniqueChunks.Contains(chunkCoord))
                            uniqueChunks.Add(chunkCoord);

                        Block curBlock = m_ActiveChunks[chunkX, chunkY, chunkZ].blocks[localX, localY, localZ];

                        // DEBUG ONLY (blocks will never have ID of 0 under normal circumstances)
                        if (curBlock.BlockTypeID == 0)
                            continue;
                        // end debug only

                        // we can only spread to water or air blocks
                        if ((curBlock.BlockTypeID != (byte)BlockID.Air) && (curBlock.BlockTypeID != (byte)BlockID.Water))
                            continue;

                        // no use spreading to something that's already as deep or deeper
                        // POTENTIAL BUG:
                        // could there be a point to spreading to something that is *as* deep? to update the flow maybe?
                        if ((curBlock.ExtraData & 15) >= neighborData.Depth)
                            continue;

                        // this block passed all the tests, give it the depth value
                        // NOTE:
                        // make sure all the block's extra data is set before calling into the TerrainManager
                        m_ActiveChunks[chunkX, chunkY, chunkZ].blocks[localX, localY, localZ].ExtraData |= (byte)(neighborData.Depth);
                        //m_ActiveChunks[chunkX, chunkY, chunkZ].blocks[localX, localY, localZ].BlockTypeID = (byte)BlockID.Water;
                        // POTENTIAL BUG:
                        // The order of operations here might mess up. This needs to operate only on the upper 4 bits of the ExtraData byte.
                        // But it also needs to OR those bits so a block can have more than one flow direction.
                        m_ActiveChunks[chunkX, chunkY, chunkZ].blocks[localX, localY, localZ].ExtraData |= (byte)((byte)neighborData.FlowDirection << 4);
                        UnityEngine.Debug.Log("[Fluid] Placing water block as part of tick operation via TerrainManager");
                        TerrainManager.Instance.PlaceBlock(chunkX, chunkY, chunkZ, localX, localY, localZ, BlockID.Water);
                        needsMeshUpdate = true;

                        byte newDepth = (byte)(neighborData.Depth - 1);
                        // if the new depth value is 0 or less then we've run out of fluid to propagate
                        // so don't even add the neighbors
                        if (newDepth <= 0)
                            continue;


                        // continue propagation by adding each neighbor's neigbors
                        // right
                        tick.Neighbors.Enqueue(new FluidSim_TickData(neighborData.x + 1, neighborData.y, neighborData.z, EFlowDirection.East, newDepth));
                        // left
                        tick.Neighbors.Enqueue(new FluidSim_TickData(neighborData.x - 1, neighborData.y, neighborData.z, EFlowDirection.West, newDepth));
                        // forward
                        tick.Neighbors.Enqueue(new FluidSim_TickData(neighborData.x, neighborData.y, neighborData.z + 1, EFlowDirection.North, newDepth));
                        // backward
                        tick.Neighbors.Enqueue(new FluidSim_TickData(neighborData.x, neighborData.y, neighborData.z - 1, EFlowDirection.South, newDepth));
                        // down
                        tick.Neighbors.Enqueue(new FluidSim_TickData(neighborData.x, neighborData.y - 1, neighborData.z, EFlowDirection.Down, newDepth));
                    }
                }
                else
                {
                    UnityEngine.Debug.Log("[Fluid] Processing removal tick");
                    // we are removing a fluid block
                    m_ActiveChunks[originChunkPos.X, originChunkPos.Y, originChunkPos.Z].blocks[originLocalPos.X, originLocalPos.Y, originLocalPos.Z].BlockTypeID = (byte)BlockID.Air;
                    m_ActiveChunks[originChunkPos.X, originChunkPos.Y, originChunkPos.Z].blocks[originLocalPos.X, originLocalPos.Y, originLocalPos.Z].ExtraData = 0;
                    int curNeighbors = tick.Neighbors.Count, processedNeighbors = 0;
                    while (processedNeighbors <= curNeighbors)
                    {
                        FluidSim_TickData neighborData = tick.Neighbors.Dequeue();
                        processedNeighbors++;
                        // propagate to each neighbor by setting their depth value to curDepth - 1
                        if ((neighborData.x < 0) || (neighborData.y < 0) || (neighborData.z < 0) ||
                            (neighborData.x >= TotalBlocksX) || (neighborData.y >= TotalBlocksY) || (neighborData.z >= TotalBlocksZ))
                            continue;
                        chunkX = (int)(neighborData.x / Settings.ChunkSizeX);
                        chunkY = (int)(neighborData.y / Settings.ChunkSizeY);
                        chunkZ = (int)(neighborData.z / Settings.ChunkSizeZ);
                        localX = (int)(neighborData.x % Settings.ChunkSizeX);
                        localY = (int)(neighborData.y % Settings.ChunkSizeY);
                        localZ = (int)(neighborData.z % Settings.ChunkSizeZ);

                        Vector3i chunkCoord = new Vector3i(chunkX, chunkY, chunkZ);
                        if (!uniqueChunks.Contains(chunkCoord))
                            uniqueChunks.Add(chunkCoord);

                        Block curBlock = m_ActiveChunks[chunkX, chunkY, chunkZ].blocks[localX, localY, localZ];

                        // DEBUG ONLY (blocks will never have ID of 0 under normal circumstances)
                        if (curBlock.BlockTypeID == 0)
                            continue;
                        // end debug only

                        // we're only updating fluid blocks
                        if (curBlock.BlockTypeID != (byte)BlockID.Water)
                            continue;

                        // if it's deeper than our origin then we know it's upstream, so any modification to our origin can't possibly effect it or its neighbors, so we can skip it
                        // POTENTIAL BUG:
                        // not sure how to handle blocks with equal depth, might need to change this to just '>'
                        if ((curBlock.ExtraData & 15) >= depth)
                            continue;

                        // if it has less depth than our origin then we know the fluid should be removed
                        //m_ActiveChunks[chunkX, chunkY, chunkZ].blocks[localX, localY, localZ].BlockTypeID = (byte)BlockID.Air;
                        // NOTE:
                        // clear the block's extra data before calling into TerrainManager
                        m_ActiveChunks[chunkX, chunkY, chunkZ].blocks[localX, localY, localZ].ExtraData = 0;
                        TerrainManager.Instance.PlaceBlock(chunkX, chunkY, chunkZ, localX, localY, localZ, BlockID.Air);

                        needsMeshUpdate = true;

                        // continue propagation by adding each neighbor's neigbors
                        // right
                        tick.Neighbors.Enqueue(new FluidSim_TickData(neighborData.x + 1, neighborData.y, neighborData.z, EFlowDirection.East, 0));
                        // left
                        tick.Neighbors.Enqueue(new FluidSim_TickData(neighborData.x - 1, neighborData.y, neighborData.z, EFlowDirection.West, 0));
                        // forward
                        tick.Neighbors.Enqueue(new FluidSim_TickData(neighborData.x, neighborData.y, neighborData.z + 1, EFlowDirection.North, 0));
                        // backward
                        tick.Neighbors.Enqueue(new FluidSim_TickData(neighborData.x, neighborData.y, neighborData.z - 1, EFlowDirection.South, 0));
                        // down
                        tick.Neighbors.Enqueue(new FluidSim_TickData(neighborData.x, neighborData.y - 1, neighborData.z, EFlowDirection.Down, 0));
                    }
                }
            }
            //UnityEngine.Debug.Log("[Fluid] Done with sim update");
            if (needsMeshUpdate)
            {
                foreach (Vector3i chunkPos in uniqueChunks)
                {
                    m_ActiveChunks[chunkPos.x, chunkPos.y, chunkPos.z].Water_FluidMesh_NeedsRegen = true;
                }
            }
            needsMeshUpdate = false;
        }
    }
}
