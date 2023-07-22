using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public List<Vector3i> SunlightBlocks = new List<Vector3i>(1024);

    public Block[,,] blocks = new Block[Settings.ChunkSizeX, Settings.ChunkSizeY, Settings.ChunkSizeZ];
    public MeshRenderer Renderer;
    public MeshFilter OpaqueMeshFilter;
    public MeshFilter TransparentMeshFilter;
    public MeshFilter Water_FluidMeshFilter;
    public MeshCollider OpaqueMeshCollider;

    public bool Water_FluidMesh_NeedsRegen = false;
    public bool OpaqueMesh_NeedsRegen = false;
    public bool TransparentMesh_NeedsRegen = false;

    public int ChunkPosX, ChunkPosY, ChunkPosZ;
    // temp design flaw
    TerrainManager m_TerrainManager;


    public Block[,,] Blocks { get { return blocks; } }

    public void Initialize(TerrainManager manager, ByteVec3 chunkPosition)
    {
        Renderer = GetComponent<MeshRenderer>();
        OpaqueMeshFilter = GetComponent<MeshFilter>();
        TransparentMeshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
        Water_FluidMeshFilter = transform.GetChild(1).GetComponent<MeshFilter>();

        OpaqueMeshCollider = GetComponent<MeshCollider>();

        m_TerrainManager = manager;
        ChunkPosX = chunkPosition.X;
        ChunkPosY = chunkPosition.Y;
        ChunkPosZ = chunkPosition.Z;
    }

    public void Update()
    {
        if (Water_FluidMesh_NeedsRegen)
        {

        }
    }
}