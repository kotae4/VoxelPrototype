using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using VoxelPrototype.Lighting;
using VoxelPrototype.TerrainGenerators;
using VoxelPrototype.MeshGenerators;
using VoxelPrototype.Fluids;

public class TerrainManager : MonoBehaviour
{
    private static TerrainManager _Instance;
    public static TerrainManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                TerrainManager tm = FindObjectOfType<TerrainManager>();
                if (tm == null)
                {
                    GameObject go = new GameObject("TerrainManager");
                    _Instance = go.AddComponent<TerrainManager>();
                }
                else
                    _Instance = tm;
            }
            return _Instance;
        }
    }


    [SerializeField]
    Chunk ChunkPrefab;
    [SerializeField]
    bool DebugStartup = false;

    // 16,8,16 chunks = 2048 total chunks.
    // 16,16,16 blocks = 4096 blocks per chunk = 8,388,608 total blocks
    public Chunk[,,] m_ActiveChunks = new Chunk[Settings.NUM_CHUNKS, Settings.WORLD_HEIGHT / Settings.ChunkSizeY, Settings.NUM_CHUNKS];

    public FluidProcessor m_FluidProcessor = new FluidProcessor();
    float lastFluidUpdate = 0f;
    const float FLUID_UPDATE_RATE = 3.0f;

    static int NumChunksInstantiated = 0;

    bool EnsureSingleInstance()
    {
        if ((_Instance != null) && (_Instance != this))
        {
            // enforce single GO
            Destroy(this.gameObject);
            Debug.Log(name + " LevelManager Instance exists, destroying self");
            return false;
        }
        _Instance = this;
        return true;
    }

    void Awake()
    {
        Debug.Log(name + " TerrainManager.Awake()");
        if (!EnsureSingleInstance())
            return;
    }

    private void Start()
    {
        if (DebugStartup == false)
        {
            Debug.Log("Creating Chunk GOs");
            CreatePooledChunkGameobjects();
            Debug.Log("Generating chunk data");
            CreateChunkData();
            Debug.Log("Generating chunk meshes");
            CreateChunkMeshes();
            Debug.Log("Done generating chunks");
        }
    }

    public Material GetChunkMaterial()
    {
        if (NumChunksInstantiated == 0)
            return null;
        return m_ActiveChunks[0, 0, 0].Renderer.sharedMaterial;
    }

    public bool GetBlockAtWorldPos_ReadOnly(int x, int y, int z, out Block retVal)
    {
        const int TotalBlocksX = Settings.NUM_CHUNKS * Settings.ChunkSizeX;
        const int TotalBlocksY = Settings.NUM_CHUNKS * Settings.ChunkSizeY;
        const int TotalBlocksZ = Settings.NUM_CHUNKS * Settings.ChunkSizeZ;

        // TO-DO:
        // REMOVE THIS SANITY CHECK WHEN LOADING IS PROPER
        if (m_ActiveChunks == null)
        {
            retVal = default(Block);
            return false;
        }
        // END TO-DO

        if ((x < 0) || (y < 0) || (z < 0) ||
            (x >= TotalBlocksX) || (y >= TotalBlocksY) || (z >= TotalBlocksZ))
        {
            retVal = default(Block);
            return false;
        }

        int chunkX = x / Settings.ChunkSizeX;
        int chunkY = y / Settings.ChunkSizeY;
        int chunkZ = z / Settings.ChunkSizeZ;

        int localX = x % Settings.ChunkSizeX;
        int localY = y % Settings.ChunkSizeY;
        int localZ = z % Settings.ChunkSizeZ;

        // TO-DO:
        // REMOVE THIS SANITY CHECK WHEN LOADING IS PROPER
        if (m_ActiveChunks[chunkX, chunkY, chunkZ] == null)
        {
            retVal = default(Block);
            return false;
        }
        // END TO-DO

        retVal = m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ];
        return true;
    }

    void CreatePooledChunkGameobjects()
    {
        for (int x = 0; x < Settings.NUM_CHUNKS; x++)
        {
            for (int y = 0; y < (Settings.WORLD_HEIGHT / Settings.ChunkSizeY); y++)
            {
                for (int z = 0; z < Settings.NUM_CHUNKS; z++)
                {
                    m_ActiveChunks[x,y,z] = Instantiate(ChunkPrefab, new Vector3(x * Settings.ChunkSizeX, y * Settings.ChunkSizeY, z * Settings.ChunkSizeX), Quaternion.identity);
                    m_ActiveChunks[x,y,z].name = "Chunk_" + x.ToString() + "_" + y.ToString() + "_" + z.ToString();
                    m_ActiveChunks[x, y, z].Initialize(this, new ByteVec3(x, y, z));
                    NumChunksInstantiated++;
                }
            }
        }
    }

    void CreateChunkData()
    {
        // 13, 12, 7 is the target chunk. it has surrounding chunks so it can properly generate lighting.
        int worldX, worldY, worldZ;
        for (int chunkX = 12; chunkX < 15; chunkX++)
        {
            for (int chunkZ = 6; chunkZ < 9; chunkZ++)
            {
                SimplexTerrainGenerator.Generate(chunkX, chunkZ);
            }
        }
        for (int chunkX = 12; chunkX < 15; chunkX++)
        {
            for (int chunkY = 0; chunkY < (Settings.WORLD_HEIGHT / Settings.ChunkSizeY); chunkY++)
            {
                for (int chunkZ = 6; chunkZ < 9; chunkZ++)
                {
                    LightProcessor.PropagateSunlight(m_ActiveChunks[chunkX, chunkY, chunkZ]);
                }
            }
        }
    }

    void CreateChunkMeshes()
    {
        for (int x = 0; x < Settings.NUM_CHUNKS; x++)
        {
            for (int y = 0; y < (Settings.WORLD_HEIGHT / Settings.ChunkSizeY); y++)
            {
                for (int z = 0; z < Settings.NUM_CHUNKS; z++)
                {
                    Mesh opaqueMesh, transparentMesh, waterMesh;
                    NaiveMeshGenerator.GenerateMesh(m_ActiveChunks[x, y, z], out opaqueMesh, out transparentMesh, out waterMesh);
                    m_ActiveChunks[x, y, z].OpaqueMeshFilter.mesh = opaqueMesh;
                    m_ActiveChunks[x, y, z].TransparentMeshFilter.mesh = transparentMesh;
                    m_ActiveChunks[x, y, z].Water_FluidMeshFilter.mesh = waterMesh;
                    // probably broken now, but we don't use it anyway
                    m_ActiveChunks[x, y, z].OpaqueMeshCollider.sharedMesh = opaqueMesh;
                }
            }
        }
    }

    void Update()
    {
        // NOTE:
        // should probably move the fluid processor logic to... the fluid processor class. duh.
        lastFluidUpdate += Time.deltaTime;
        if (lastFluidUpdate >= FLUID_UPDATE_RATE)
        {
            m_FluidProcessor.UpdateFluidSim();
            lastFluidUpdate = 0f;
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            CreatePooledChunkGameobjects();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            CreateChunkData();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            NaiveMeshGenerator.bCenterMesh = !NaiveMeshGenerator.bCenterMesh;
            Debug.Log("CenterMesh: " + NaiveMeshGenerator.bCenterMesh);
            CreateChunkMeshes();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            CreateChunkMeshes();
        }
        if (Input.GetKeyDown(KeyCode.F1))
        {
            for (int chunkX = 12; chunkX < 15; chunkX++)
            {
                for (int chunkY = 0; chunkY < (Settings.WORLD_HEIGHT / Settings.ChunkSizeY); chunkY++)
                {
                    for (int chunkZ = 6; chunkZ < 9; chunkZ++)
                    {
                        LightProcessor.PropagateSunlight(m_ActiveChunks[chunkX, chunkY, chunkZ]);
                    }
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            int worldX, worldY, worldZ;
            for (int chunkX = 12; chunkX < 15; chunkX++)
            {
                for (int chunkY = 0; chunkY < (Settings.WORLD_HEIGHT / Settings.ChunkSizeY); chunkY++)
                {
                    for (int chunkZ = 6; chunkZ < 9; chunkZ++)
                    {
                        for (int localX = 0; localX < Settings.ChunkSizeX; localX++)
                        {
                            for (int localZ = 0; localZ < Settings.ChunkSizeZ; localZ++)
                            {
                                for (int localY = Settings.ChunkSizeY - 1; localY >= 0; localY--)
                                {
                                    Block workingBlock = m_ActiveChunks[chunkX, chunkY, chunkZ].Blocks[localX, localY, localZ];
                                    if ((workingBlock.BlockTypeID == (byte)BlockID.Air) && (workingBlock.SunlightValue == Settings.SUNLIGHT_VALUE))
                                    {
                                        worldX = (chunkX * Settings.ChunkSizeX) + localX;
                                        worldY = (chunkY * Settings.ChunkSizeY) + localY;
                                        worldZ = (chunkZ * Settings.ChunkSizeZ) + localZ;
                                        LightProcessor.PropagateSunlightRecursive(worldX, worldY, worldZ, workingBlock.SunlightValue);

                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            PlaceSphere(new Utilities.Vec3s(104, 30, 55), 5);
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            PlaceBlock(12, 12, 7, 0, 0, 0, BlockID.Water);
            PlaceBlock(12, 12, 7, 0, 0, 1, BlockID.Water);
        }
    }

    public List<Vec3s> GetBlocksWithinSphere(Vec3s center, short radius)
    {
        List<Vec3s> retVal = new List<Utilities.Vec3s>();
        int minX = center.X - radius, minY = center.Y - radius, minZ = center.Z - radius;
        int maxX = center.X + radius, maxY = center.Y + radius, maxZ = center.Z + radius;
        int diffX, diffY, diffZ;
        int distance;
        for (int worldX = minX; worldX < maxX; worldX++)
        {
            for (int worldY = minY; worldY < maxY; worldY++)
            {
                for (int worldZ = minZ; worldZ < maxZ; worldZ++)
                {
                    diffX = worldX - center.X;
                    diffY = worldY - center.Y;
                    diffZ = worldZ - center.Z;
                    distance = (int)System.Math.Sqrt(diffX * diffX + diffY * diffY + diffZ * diffZ);
                    if (distance <= radius)
                        retVal.Add(new Utilities.Vec3s((short)worldX, (short)worldY, (short)worldZ));
                }
            }
        }
        return retVal;
    }

    public void PlaceSphere(Vec3s center, short radius)
    {
        int minX = center.X - radius, minY = center.Y - radius, minZ = center.Z - radius;
        int maxX = center.X + radius, maxY = center.Y + radius, maxZ = center.Z + radius;
        int diffX, diffY, diffZ;
        int distance;
        for (int worldX = minX; worldX < maxX; worldX++)
        {
            for (int worldY = minY; worldY < maxY; worldY++)
            {
                for (int worldZ = minZ; worldZ < maxZ; worldZ++)
                {
                    diffX = worldX - center.X;
                    diffY = worldY - center.Y;
                    diffZ = worldZ - center.Z;
                    distance = (int)System.Math.Sqrt(diffX * diffX + diffY * diffY + diffZ * diffZ);
                    if (distance > radius)
                        continue;
                    PlaceBlockAtWorldPos(worldX, worldY, worldZ, BlockID.Debug);
                }
            }
        }
    }

    public void PlaceBlockAtWorldPos(int worldX, int worldY, int worldZ, BlockID blockType, bool skipMeshUpdate = false)
    {
        ByteVec3 chunkPos, localPos;
        if (GetChunkLocalFromWorld(worldX, worldY, worldZ, out chunkPos, out localPos))
            PlaceBlock(chunkPos.X, chunkPos.Y, chunkPos.Z, localPos.X, localPos.Y, localPos.Z, blockType, skipMeshUpdate);
        else
            Debug.LogError("Error placing block; could not get local coords from world coords");
    }

    public void PlaceBlock(int chunkX, int chunkY, int chunkZ, int localX, int localY, int localZ, BlockID blockType, bool skipMeshUpdate = false)
    {
        IBlockType newType = BlockTypes.GetBlockType((byte)blockType);
        Chunk centerChunk = m_ActiveChunks[chunkX, chunkY, chunkZ];

        int worldX = (localX + (chunkX * Settings.ChunkSizeX)), worldY = (localY + (chunkY * Settings.ChunkSizeX)), worldZ = (localZ + (chunkZ * Settings.ChunkSizeX));
        // TO-DO:
        // optimize all light regeneration.
        // everything to do with lighting below is sloppy as heck and leads to 20ms frame spikes
        // we just need to add a "PropagateLightFrom" function
        // then we go through each neighbor (already doing this below for visibility) and get the highest light value and assign it to this block if air

        if ((centerChunk.Blocks[localX, localY, localZ].IsBlockingColumnSunlight) && (newType.IsAir))
        {
            // if the block being replaced is responsible for blocking sunlight along its y-value and it's being replaced with air
            // then we need to set the new block to be a sunlight block for proper light propagation
            // (light needs to be propagated regardless, this is just necessary to ensure propagation has all the correct data)
            centerChunk.Blocks[localX, localY, localZ].IsBlockingColumnSunlight = false;
            centerChunk.Blocks[localX, localY, localZ].SunlightValue = Settings.SUNLIGHT_VALUE;
            centerChunk.SunlightBlocks.Add(new Vector3i((localX + (chunkX * Settings.ChunkSizeX)), (localY + (chunkY * Settings.ChunkSizeX)), (localZ + (chunkZ * Settings.ChunkSizeX))));
        }
        else if ((centerChunk.Blocks[localX, localY, localZ].BlockTypeID == (byte)BlockID.Air) && (!newType.IsAir))
        {
            // if the previous block was a sunlight block and it's being replaced by a non-air block then we need to remove it from the chunk's SunlightBlocks list
            if (centerChunk.SunlightBlocks.Remove(new Vector3i(worldX, worldY, worldZ)) == false)
            {
                Debug.Log("Failed to remove existing sunlight block when placing new non-air block");
            }
            else
            {
                centerChunk.Blocks[localX, localY, localZ].IsBlockingColumnSunlight = true;
            }
        }
        LightOperation lightOperation = LightOperation.Nothing;
        byte highestLightValue = 0;
        // if the previous block had light
        // then this is either a lit -> non-lit or a lit -> lit transition
        if (centerChunk.Blocks[localX, localY, localZ].SunlightValue > 0)
        {
            // if the new block is solid and provides no light of its own
            // then this is a lit -> non-lit transition
            if ((newType.LightSourceIntensity == 0) && (!newType.IsAir) && (!newType.IsTransparent))
            {
                lightOperation = LightOperation.RemovingSource;
                highestLightValue = centerChunk.Blocks[localX, localY, localZ].SunlightValue;
            }
            // otherwise it's a lit -> lit transition
            else
            {
                lightOperation = LightOperation.AddingSource;
            }
        }
        // otherwise it's either a non-lit -> non-lit or a non-lit -> lit transition
        else
        {
            // if the new block is solid and provides no light of its own
            // then this is a non-lit -> non-lit transition
            if ((newType.LightSourceIntensity == 0) && (!newType.IsAir) && (!newType.IsTransparent))
                lightOperation = LightOperation.Nothing;
            // otherwise it's a non-lit -> lit transition
            else
                lightOperation = LightOperation.AddingSource;
        }


        centerChunk.Blocks[localX, localY, localZ].BlockTypeID = (byte)blockType;
        bool neighborsVisible = newType.IsAir || newType.IsTransparent || newType.IsFluid;
        bool areWeVisible = false;
        bool areTheyVisible = false;
        Queue<Chunk> chunksPendingRegen = new Queue<Chunk>(6);
        chunksPendingRegen.Enqueue(centerChunk);
        // set all 6 neighbors to have their appropriate face visible

        IBlockType neighborType;
        // ============================================
        // ============== Left Neighbor ===============
        // ============================================
        if (localX - 1 < 0)
        {
            // get the chunk to the left if it exists
            if (chunkX - 1 >= 0)
            {
                // -1 in centerChunk is ChunkSizeX in neighboring chunk
                neighborType = BlockTypes.GetBlockType(m_ActiveChunks[chunkX - 1, chunkY, chunkZ].Blocks[(Settings.ChunkSizeX - 1), localY, localZ].BlockTypeID);
                // NOTE:
                // TO-DO:
                // OPTIMIZATION: there is an early exit condition here, can't think of it at the moment. but think about what the most common case is and check that first.

                // so, when updating the neighbor's visibility, we want to factor in fluids too.
                // so, a neighbor is visible if we are transparent, air, or a fluid, BUT if we are fluid and they are fluid then they are NOT visible
                areTheyVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborsVisible;
                // we want our right face to be visible if the neighbor is air, transparent, or fluid. additionally, if the neighbor is fluid AND *we* are fluid then we DON'T want to be visible.
                areWeVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborType.IsAir || neighborType.IsTransparent || neighborType.IsFluid;
                m_ActiveChunks[chunkX - 1, chunkY, chunkZ].Blocks[(Settings.ChunkSizeX - 1), localY, localZ].SetVisible((byte)Direction.East, areTheyVisible);
                centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.West, areWeVisible);
                // add left chunk neighbor for regen
                chunksPendingRegen.Enqueue(m_ActiveChunks[chunkX - 1, chunkY, chunkZ]);
                // check if this neighbor has the highest light source
                if (lightOperation != LightOperation.RemovingSource && m_ActiveChunks[chunkX - 1, chunkY, chunkZ].Blocks[(Settings.ChunkSizeX - 1), localY, localZ].SunlightValue > highestLightValue)
                    highestLightValue = m_ActiveChunks[chunkX - 1, chunkY, chunkZ].Blocks[(Settings.ChunkSizeX - 1), localY, localZ].SunlightValue;
            }
            else
            {
                centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.West, true);
            }
        }
        else
        {
            neighborType = BlockTypes.GetBlockType(centerChunk.Blocks[(localX - 1), localY, localZ].BlockTypeID);
            areTheyVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborsVisible;
            centerChunk.Blocks[(localX - 1), localY, localZ].SetVisible((byte)Direction.East, areTheyVisible);
            areWeVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborType.IsAir || neighborType.IsTransparent || neighborType.IsFluid;
            centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.West, areWeVisible);
            // check if this neighbor has the highest light source
            if (lightOperation != LightOperation.RemovingSource && centerChunk.Blocks[(localX - 1), localY, localZ].SunlightValue > highestLightValue)
                highestLightValue = centerChunk.Blocks[(localX - 1), localY, localZ].SunlightValue;
        }

        // ============================================
        // =========== Right Neighbor =================
        // ============================================
        if (localX + 1 >= Settings.ChunkSizeX)
        {
            // get the chunk to the right if it exists
            if (chunkX + 1 < Settings.NUM_CHUNKS)
            {
                // Settings.ChunkSizeX in centerChunk is 0 in neighboring chunk
                neighborType = BlockTypes.GetBlockType(m_ActiveChunks[chunkX + 1, chunkY, chunkZ].Blocks[0, localY, localZ].BlockTypeID);
                areTheyVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborsVisible;
                m_ActiveChunks[chunkX + 1, chunkY, chunkZ].Blocks[0, localY, localZ].SetVisible((byte)Direction.West, areTheyVisible);
                areWeVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborType.IsAir || neighborType.IsTransparent || neighborType.IsFluid;
                centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.East, areWeVisible);
                // add right chunk neighbor for regen
                chunksPendingRegen.Enqueue(m_ActiveChunks[chunkX + 1, chunkY, chunkZ]);
                // check if this neighbor has the highest light source
                if (lightOperation != LightOperation.RemovingSource && m_ActiveChunks[chunkX + 1, chunkY, chunkZ].Blocks[0, localY, localZ].SunlightValue > highestLightValue)
                    highestLightValue = m_ActiveChunks[chunkX + 1, chunkY, chunkZ].Blocks[0, localY, localZ].SunlightValue;
            }
            else
            {
                centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.East, true);
            }
        }
        else
        {
            neighborType = BlockTypes.GetBlockType(centerChunk.Blocks[(localX + 1), localY, localZ].BlockTypeID);
            areTheyVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborsVisible;
            centerChunk.Blocks[(localX + 1), localY, localZ].SetVisible((byte)Direction.West, areTheyVisible);
            areWeVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborType.IsAir || neighborType.IsTransparent || neighborType.IsFluid;
            centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.East, areWeVisible);
            // check if this neighbor has the highest light source
            if (lightOperation != LightOperation.RemovingSource && centerChunk.Blocks[(localX + 1), localY, localZ].SunlightValue > highestLightValue)
                highestLightValue = centerChunk.Blocks[(localX + 1), localY, localZ].SunlightValue;
        }

        // ============================================
        // =========== Bottom Neighbor ================
        // ============================================
        if (localY - 1 < 0)
        {
            // get the chunk in below of us if it exists
            if (chunkY - 1 >= 0)
            {
                // -1 in centerChunk is ChunkSizeZ in neighboring chunk
                neighborType = BlockTypes.GetBlockType(m_ActiveChunks[chunkX, chunkY - 1, chunkZ].Blocks[localX, (Settings.ChunkSizeY - 1), localZ].BlockTypeID);
                areTheyVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborsVisible;
                m_ActiveChunks[chunkX, chunkY - 1, chunkZ].Blocks[localX, (Settings.ChunkSizeY - 1), localZ].SetVisible((byte)Direction.Up, areTheyVisible);
                areWeVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborType.IsAir || neighborType.IsTransparent || neighborType.IsFluid;
                centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.Down, areWeVisible);
                // add bottom neighbor chunk for regen
                chunksPendingRegen.Enqueue(m_ActiveChunks[chunkX, chunkY - 1, chunkZ]);
                // check if this neighbor has the highest light source
                if (lightOperation != LightOperation.RemovingSource && m_ActiveChunks[chunkX, chunkY - 1, chunkZ].Blocks[localX, (Settings.ChunkSizeY - 1), localZ].SunlightValue > highestLightValue)
                    highestLightValue = m_ActiveChunks[chunkX, chunkY - 1, chunkZ].Blocks[localX, (Settings.ChunkSizeY - 1), localZ].SunlightValue;
            }
            else
            {
                centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.Down, true);
            }
        }
        else
        {
            neighborType = BlockTypes.GetBlockType(centerChunk.Blocks[localX, localY - 1, localZ].BlockTypeID);
            areTheyVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborsVisible;
            centerChunk.Blocks[localX, localY - 1, localZ].SetVisible((byte)Direction.Up, areTheyVisible);
            areWeVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborType.IsAir || neighborType.IsTransparent || neighborType.IsFluid;
            centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.Down, areWeVisible);
            // check if this neighbor has the highest light source
            if (lightOperation != LightOperation.RemovingSource && centerChunk.Blocks[localX, localY - 1, localZ].SunlightValue > highestLightValue)
                highestLightValue = centerChunk.Blocks[localX, localY - 1, localZ].SunlightValue;
        }


        // ============================================
        // ============== Top Neighbor ================
        // ============================================
        if (localY + 1 >= Settings.ChunkSizeY)
        {
            // get the chunk to the top if it exists
            if (chunkY + 1 < (Settings.WORLD_HEIGHT / Settings.ChunkSizeY))
            {
                // Settings.ChunkSizeX in centerChunk is 0 in neighboring chunk
                neighborType = BlockTypes.GetBlockType(m_ActiveChunks[chunkX, chunkY + 1, chunkZ].Blocks[localX, 0, localZ].BlockTypeID);
                areTheyVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborsVisible;
                m_ActiveChunks[chunkX, chunkY + 1, chunkZ].Blocks[localX, 0, localZ].SetVisible((byte)Direction.Down, areTheyVisible);
                areWeVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborType.IsAir || neighborType.IsTransparent || neighborType.IsFluid;
                centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.Up, areWeVisible);
                // add top neighbor chunk for regen
                chunksPendingRegen.Enqueue(m_ActiveChunks[chunkX, chunkY + 1, chunkZ]);
                // check if this neighbor has the highest light source
                if (lightOperation != LightOperation.RemovingSource && m_ActiveChunks[chunkX, chunkY + 1, chunkZ].Blocks[localX, 0, localZ].SunlightValue > highestLightValue)
                    highestLightValue = m_ActiveChunks[chunkX, chunkY + 1, chunkZ].Blocks[localX, 0, localZ].SunlightValue;
            }
            else
            {
                centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.Up, true);
            }
        }
        else
        {
            neighborType = BlockTypes.GetBlockType(centerChunk.Blocks[localX, localY + 1, localZ].BlockTypeID);
            areTheyVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborsVisible;
            centerChunk.Blocks[localX, localY + 1, localZ].SetVisible((byte)Direction.Down, areTheyVisible);
            areWeVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborType.IsAir || neighborType.IsTransparent || neighborType.IsFluid;
            centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.Up, areWeVisible);
            // check if this neighbor has the highest light source
            if (lightOperation != LightOperation.RemovingSource && centerChunk.Blocks[localX, localY + 1, localZ].SunlightValue > highestLightValue)
                highestLightValue = centerChunk.Blocks[localX, localY + 1, localZ].SunlightValue;
        }

        // ============================================
        // ============= Front Neighbor ===============
        // ============================================
        if (localZ - 1 < 0)
        {
            // get the chunk in front of us if it exists
            if (chunkZ - 1 >= 0)
            {
                // -1 in centerChunk is ChunkSizeZ in neighboring chunk
                neighborType = BlockTypes.GetBlockType(m_ActiveChunks[chunkX, chunkY, chunkZ - 1].Blocks[localX, localY, (Settings.ChunkSizeZ - 1)].BlockTypeID);
                areTheyVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborsVisible;
                m_ActiveChunks[chunkX, chunkY, chunkZ - 1].Blocks[localX, localY, (Settings.ChunkSizeZ - 1)].SetVisible((byte)Direction.North, areTheyVisible);
                areWeVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborType.IsAir || neighborType.IsTransparent || neighborType.IsFluid;
                centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.South, areWeVisible);
                // add front neighbor chunk for regen
                chunksPendingRegen.Enqueue(m_ActiveChunks[chunkX, chunkY, chunkZ - 1]);
                // check if this neighbor has the highest light source
                if (lightOperation != LightOperation.RemovingSource && m_ActiveChunks[chunkX, chunkY, chunkZ - 1].Blocks[localX, localY, (Settings.ChunkSizeZ - 1)].SunlightValue > highestLightValue)
                    highestLightValue = m_ActiveChunks[chunkX, chunkY, chunkZ - 1].Blocks[localX, localY, (Settings.ChunkSizeZ - 1)].SunlightValue;
            }
            else
            {
                centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.South, true);
            }
        }
        else
        {
            neighborType = BlockTypes.GetBlockType(centerChunk.Blocks[localX, localY, localZ - 1].BlockTypeID);
            areTheyVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborsVisible;
            centerChunk.Blocks[localX, localY, localZ - 1].SetVisible((byte)Direction.North, areTheyVisible);
            areWeVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborType.IsAir || neighborType.IsTransparent || neighborType.IsFluid;
            centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.South, areWeVisible);
            // check if this neighbor has the highest light source
            if (lightOperation != LightOperation.RemovingSource && centerChunk.Blocks[localX, localY, localZ - 1].SunlightValue > highestLightValue)
                highestLightValue = centerChunk.Blocks[localX, localY, localZ - 1].SunlightValue;
        }

        // ============================================
        // ============ Back Neighbor =================
        // ============================================
        if (localZ + 1 >= Settings.ChunkSizeZ)
        {
            // get the chunk to the rear if it exists
            if (chunkZ + 1 < Settings.NUM_CHUNKS)
            {
                // Settings.ChunkSizeX in centerChunk is 0 in neighboring chunk
                neighborType = BlockTypes.GetBlockType(m_ActiveChunks[chunkX, chunkY, chunkZ + 1].Blocks[localX, localY, 0].BlockTypeID);
                areTheyVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborsVisible;
                m_ActiveChunks[chunkX, chunkY, chunkZ + 1].Blocks[localX, localY, 0].SetVisible((byte)Direction.South, areTheyVisible);
                areWeVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborType.IsAir || neighborType.IsTransparent || neighborType.IsFluid;
                centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.North, areWeVisible);
                // add back neighbor chunk for regen
                chunksPendingRegen.Enqueue(m_ActiveChunks[chunkX, chunkY, chunkZ + 1]);
                // check if this neighbor has the highest light source
                if (lightOperation != LightOperation.RemovingSource && m_ActiveChunks[chunkX, chunkY, chunkZ + 1].Blocks[localX, localY, 0].SunlightValue > highestLightValue)
                    highestLightValue = m_ActiveChunks[chunkX, chunkY, chunkZ + 1].Blocks[localX, localY, 0].SunlightValue;
            }
            else
            {
                centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.North, true);
            }
        }
        else
        {
            neighborType = BlockTypes.GetBlockType(centerChunk.Blocks[localX, localY, localZ + 1].BlockTypeID);
            areTheyVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborsVisible;
            centerChunk.Blocks[localX, localY, localZ + 1].SetVisible((byte)Direction.South, areTheyVisible);
            areWeVisible = (neighborType.IsFluid && newType.IsFluid) ? false : neighborType.IsAir || neighborType.IsTransparent || neighborType.IsFluid;
            centerChunk.Blocks[localX, localY, localZ].SetVisible((byte)Direction.North, areWeVisible);
            // check if this neighbor has the highest light source
            if (lightOperation != LightOperation.RemovingSource && centerChunk.Blocks[localX, localY, localZ + 1].SunlightValue > highestLightValue)
                highestLightValue = centerChunk.Blocks[localX, localY, localZ + 1].SunlightValue;
        }

        // now regen the effected chunks
        if (skipMeshUpdate)
            return;
        // re-propagate light. this must be done regardless of whether sunlight data has changed.
        // we can't assume anything about the surroundings
        if (lightOperation == LightOperation.AddingSource)
        {
            // use highest light value, whether it's from a neighbor or the new block itself
            if ((centerChunk.Blocks[localX, localY, localZ].SunlightValue < highestLightValue) && (newType.LightSourceIntensity < highestLightValue))
                highestLightValue -= 1;
            else
                highestLightValue = (newType.LightSourceIntensity >= centerChunk.Blocks[localX, localY, localZ].SunlightValue ? newType.LightSourceIntensity : centerChunk.Blocks[localX, localY, localZ].SunlightValue);
            LightProcessor.PropagateLightFrom(worldX, worldY, worldZ, highestLightValue);
        }
        else if (lightOperation == LightOperation.RemovingSource)
        {
            LightProcessor.RemoveLightSourceAt(worldX, worldY, worldZ, highestLightValue);
        }
        Chunk curRegenChunk;
        while (chunksPendingRegen.Count > 0)
        {
            curRegenChunk = chunksPendingRegen.Dequeue();
            Mesh opaqueMesh, transparentMesh, fluidMesh;
            NaiveMeshGenerator.GenerateMesh(curRegenChunk, out opaqueMesh, out transparentMesh, out fluidMesh);
            curRegenChunk.OpaqueMeshFilter.mesh = opaqueMesh;
            curRegenChunk.TransparentMeshFilter.mesh = transparentMesh;
            curRegenChunk.Water_FluidMeshFilter.mesh = fluidMesh;
            // probably broken now, but we don't use it anyway
            curRegenChunk.OpaqueMeshCollider.sharedMesh = opaqueMesh;
        }
    }

    public bool GetChunkLocalFromWorld(int worldPosX, int worldPosY, int worldPosZ, out ByteVec3 chunkPos, out ByteVec3 localPos)
    {
        const int TotalBlocksX = Settings.NUM_CHUNKS * Settings.ChunkSizeX;
        const int TotalBlocksY = Settings.NUM_CHUNKS * Settings.ChunkSizeY;
        const int TotalBlocksZ = Settings.NUM_CHUNKS * Settings.ChunkSizeZ;

        chunkPos = new ByteVec3();
        localPos = new ByteVec3();

        if ((worldPosX < 0) || (worldPosY < 0) || (worldPosZ < 0) ||
            (worldPosX >= TotalBlocksX) || (worldPosY >= TotalBlocksY) || (worldPosZ >= TotalBlocksZ))
            return false;

        chunkPos.X = worldPosX / Settings.ChunkSizeX;
        chunkPos.Y = worldPosY / Settings.ChunkSizeY;
        chunkPos.Z = worldPosZ / Settings.ChunkSizeZ;

        localPos.X = worldPosX % Settings.ChunkSizeX;
        localPos.Y = worldPosY % Settings.ChunkSizeY;
        localPos.Z = worldPosZ % Settings.ChunkSizeZ;

        return true;
    }

    public bool GetLightValueForBlock(int worldX, int worldY, int worldZ, out byte lightValue)
    {
        lightValue = 0;
        const int TotalBlocksX = Settings.NUM_CHUNKS * Settings.ChunkSizeX;
        const int TotalBlocksY = Settings.NUM_CHUNKS * Settings.ChunkSizeY;
        const int TotalBlocksZ = Settings.NUM_CHUNKS * Settings.ChunkSizeZ;

        if ((worldX < 0) || (worldY < 0) || (worldZ < 0) ||
            (worldX >= TotalBlocksX) || (worldY >= TotalBlocksY) || (worldZ >= TotalBlocksZ))
            return false;

        lightValue = m_ActiveChunks[(worldX / Settings.ChunkSizeX), (worldY / Settings.ChunkSizeY), (worldZ / Settings.ChunkSizeZ)].Blocks[(worldX % Settings.ChunkSizeX), (worldY % Settings.ChunkSizeY), (worldZ % Settings.ChunkSizeZ)].SunlightValue;
        return true;
    }
}