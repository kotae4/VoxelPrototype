using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Experimental.Physics;
using Utilities;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    [SerializeField]
    // NOTE:
    // only public for debugging. make private later.
    public Transform m_HeadTransform;
    [SerializeField]
    Material m_HighlightMaterial;
    [SerializeField]
    RectTransform m_QuickbarSelector;
    [SerializeField]
    int[] m_QuickbarPresetPositions;

    private VoxelRaycast m_Raycaster;
    private Vector3i m_PositionDamagedBlock;
    private float m_AmountDamagedBlock;
    private Material _backingChunkSharedMaterial;
    private Material m_ChunkSharedMaterial { get { if (_backingChunkSharedMaterial == null) _backingChunkSharedMaterial = TerrainManager.Instance.GetChunkMaterial(); return _backingChunkSharedMaterial; } }

    private int m_QuickbarSelectedIndex;
    private BetterItemCollection m_QuickbarItems = new BetterItemCollection(10);

    void Start()
    {
        m_Raycaster = GameManager.Instance.Raycaster;
    }

    void Update()
    {
        Block hitBlock;
        Vector3i hitPos, hitFace;
        
        if (m_Raycaster.Raycast(m_HeadTransform.position, m_HeadTransform.forward, 5f, CollisionResponseHandler, out hitPos, out hitFace, out hitBlock))
        {
            //Debug.Log("Raycast succeeded, looking at block '" + ((BlockID)hitBlock.BlockTypeID) + "' at " + hitPos + " (face: " + hitFace + ")");
            DrawOutlineFace(hitPos, hitFace);
        }

        DoInput();
    }

    void DoInput()
    {
        if (Input.GetKeyDown(KeyCode.End))
        {
            Screen.lockCursor = (Screen.lockCursor == false) ? true : false;
        }
        float deltaScrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (deltaScrollWheel > 0f)
        {
            m_QuickbarSelectedIndex++;
            if (m_QuickbarSelectedIndex >= m_QuickbarPresetPositions.Length)
                m_QuickbarSelectedIndex = 0;
            m_QuickbarSelector.anchoredPosition = new Vector2(m_QuickbarPresetPositions[m_QuickbarSelectedIndex], m_QuickbarSelector.anchoredPosition.y);
        }
        else if (deltaScrollWheel < 0f)
        {
            m_QuickbarSelectedIndex--;
            if (m_QuickbarSelectedIndex < 0)
                m_QuickbarSelectedIndex = m_QuickbarPresetPositions.Length - 1;
            m_QuickbarSelector.anchoredPosition = new Vector2(m_QuickbarPresetPositions[m_QuickbarSelectedIndex], m_QuickbarSelector.anchoredPosition.y);
        }
        else if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl))
        {
            // place water source
            Block hitBlock;
            Vector3i hitPos, hitFace;
            if (m_Raycaster.Raycast(m_HeadTransform.position, m_HeadTransform.forward, 5f, CollisionResponseHandler, out hitPos, out hitFace, out hitBlock))
            {
                hitPos = hitPos + hitFace;
                Debug.Log("Raycast succeeded, placing block '" + ((BlockID)hitBlock.BlockTypeID) + "' at " + hitPos);
                TerrainManager.Instance.m_FluidProcessor.PropagateFluidFrom(hitPos.x, hitPos.y, hitPos.z);
            }
        }
        else if (Input.GetMouseButton(0))
        {
            Block hitBlock;
            Vector3i hitPos, hitFace;
            if (m_Raycaster.Raycast(m_HeadTransform.position, m_HeadTransform.forward, 5f, CollisionResponseHandler, out hitPos, out hitFace, out hitBlock))
            {
                Debug.Log("Raycast succeeded, destroying block '" + ((BlockID)hitBlock.BlockTypeID) + "' at " + hitPos);
                // BUG: if current block is different from previous block, reset amount damaged
                m_PositionDamagedBlock = hitPos;
                // 2 seconds to destroy a block with your fists
                m_AmountDamagedBlock += 50f * Time.deltaTime;
                m_ChunkSharedMaterial.SetVector("_DamagePos", new Vector4(hitPos.x, hitPos.y, hitPos.z, 0));
                if (m_AmountDamagedBlock >= 100f)
                {
                    TerrainManager.Instance.PlaceBlockAtWorldPos(hitPos.x, hitPos.y, hitPos.z, BlockID.Air);
                    m_AmountDamagedBlock = 0f;
                }
                m_ChunkSharedMaterial.SetFloat("_Health", ((100f - m_AmountDamagedBlock) / 100f));
                Debug.Log($"Set block health to {((100f - m_AmountDamagedBlock) / 100f)}");
            }
            else
            {
                m_AmountDamagedBlock = 0f;
                m_ChunkSharedMaterial.SetFloat("_Health", ((100f - m_AmountDamagedBlock) / 100f));
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Block hitBlock;
            Vector3i hitPos, hitFace;
            if (m_Raycaster.Raycast(m_HeadTransform.position, m_HeadTransform.forward, 5f, CollisionResponseHandler, out hitPos, out hitFace, out hitBlock))
            {
                hitPos = hitPos + hitFace;
                Debug.Log("Raycast succeeded, placing block '" + ((BlockID)hitBlock.BlockTypeID) + "' at " + hitPos);
                TerrainManager.Instance.PlaceBlockAtWorldPos(hitPos.x, hitPos.y, hitPos.z, BlockID.Glass);
            }
        }
        else
        {
            /*
            Debug.Log("Reset block damage because no input was detected");
            m_AmountDamagedBlock = 0f;
            if (m_ChunkSharedMaterial != null)
                m_ChunkSharedMaterial.SetFloat("_Health", ((100f - m_AmountDamagedBlock) / 100f));
            */
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Block hitBlock;
            Vector3i hitPos, hitFace;
            if (m_Raycaster.Raycast(m_HeadTransform.position, m_HeadTransform.forward, 10f, CollisionResponseHandler, out hitPos, out hitFace, out hitBlock))
            {
                byte lightValue;
                if (TerrainManager.Instance.GetLightValueForBlock(hitPos.x, hitPos.y, hitPos.z, out lightValue))
                {
                    Debug.Log("Raycast succeeded, saw '" + ((BlockID)hitBlock.BlockTypeID) + "' with light level " + lightValue + " at " + hitPos);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Home))
        {
            if (Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    void DrawOutlineFace(Vector3i position, Vector3i face)
    {
        Mesh outlineMesh = new Mesh();
        // linestrip, so 4 vertices to form a 2d box and 5 indices
        Vector3[] verts = new Vector3[4];
        int[] indices = new int[5] { 0, 1, 2, 3, 0 };

        float positive = VoxelPrototype.MeshGenerators.NaiveMeshGenerator.bCenterMesh ? 0.5f : 1f;
        float negative = VoxelPrototype.MeshGenerators.NaiveMeshGenerator.bCenterMesh ? -0.5f : 0f;

        if (face.x == 1)
        {
            verts[0] = new Vector3(positive, negative, negative);
            verts[1] = new Vector3(positive, positive, negative);
            verts[2] = new Vector3(positive, positive, positive);
            verts[3] = new Vector3(positive, negative, positive);
        }
        else if (face.x == -1)
        {
            verts[0] = new Vector3(negative, negative, positive);
            verts[1] = new Vector3(negative, positive, positive);
            verts[2] = new Vector3(negative, positive, negative);
            verts[3] = new Vector3(negative, negative, negative);
        }
        else if (face.y == 1)
        {
            verts[0] = new Vector3(negative, positive, negative);
            verts[1] = new Vector3(negative, positive, positive);
            verts[2] = new Vector3(positive, positive, positive);
            verts[3] = new Vector3(positive, positive, negative);
        }
        else if (face.y == -1)
        {
            verts[0] = new Vector3(negative, negative, positive);
            verts[1] = new Vector3(negative, negative, negative);
            verts[2] = new Vector3(positive, negative, negative);
            verts[3] = new Vector3(positive, negative, positive);
        }
        else if (face.z == 1)
        {
            verts[0] = new Vector3(positive, negative, positive);
            verts[1] = new Vector3(positive, positive, positive);
            verts[2] = new Vector3(negative, positive, positive);
            verts[3] = new Vector3(negative, negative, positive);
        }
        else if (face.z == -1)
        {
            verts[0] = new Vector3(negative, negative, negative);
            verts[1] = new Vector3(negative, positive, negative);
            verts[2] = new Vector3(positive, positive, negative);
            verts[3] = new Vector3(positive, negative, negative);
        }
        outlineMesh.vertices = verts;
        outlineMesh.SetIndices(indices, MeshTopology.LineStrip, 0);
        Graphics.DrawMesh(outlineMesh, position, Quaternion.identity, m_HighlightMaterial, 0);
    }

    bool CollisionResponseHandler(Vector3i position, Vector3i hitNormal, out Block hitBlock)
    {
        // If the callback returns a true value, the traversal will be stopped.
        if (TerrainManager.Instance.GetBlockAtWorldPos_ReadOnly(position.x, position.y, position.z, out hitBlock))
        {
            if ((hitBlock.BlockTypeID <= 0) || (hitBlock.BlockTypeID == (byte)BlockID.Air))
                return false;
        }
        return true;
    }
}