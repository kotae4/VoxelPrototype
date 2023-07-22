using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Experimental.Physics;
using Utilities;

public class GameManager : MonoBehaviour
{
    private static GameManager _Instance;
    public static GameManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                GameManager gm = FindObjectOfType<GameManager>();
                if (gm == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _Instance = go.AddComponent<GameManager>();
                }
                else
                    _Instance = gm;
            }
            return _Instance;
        }
    }

    public string[] TexturePaths;

    private VoxelRaycast m_Raycaster;

    internal VoxelRaycast Raycaster { get { return m_Raycaster; } }

    private void Awake()
    {
        Screen.lockCursor = true;
        m_Raycaster = new VoxelRaycast(new PhysBoundsInt(Vector3i.zero, new Vector3i(Settings.NUM_CHUNKS * Settings.ChunkSizeX, (Settings.WORLD_HEIGHT / Settings.ChunkSizeY) * Settings.ChunkSizeY, Settings.NUM_CHUNKS * Settings.ChunkSizeZ)));
    }
}