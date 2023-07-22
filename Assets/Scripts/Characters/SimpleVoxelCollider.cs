using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Experimental.Physics;
using Utilities;

public class SimpleVoxelCollider : MonoBehaviour
{
    public const float radius = 0.5f;


    private VoxelRaycast m_Raycaster;
    private Transform m_PlayerTransform;
    private PlayerController m_PlayerController;
    // ORDER:
    // TOP-LEFT FOOT
    // BOTTOM-LEFT FOOT
    // TOP-RIGHT FOOT
    // BOTTOM-RIGHT FOOT
    // TOP-LEFT HEAD
    // BOTTOM-LEFT HEAD
    // TOP-RIGHT HEAD
    // BOTTOM-RIGHT HEAD
    Vector3[] m_CornerOffsets = new Vector3[8] {
            new Vector3(-radius, -radius, radius),
            new Vector3(-radius, -radius, -radius),
            new Vector3(radius, -radius, radius),
            new Vector3(radius, -radius, -radius),
            new Vector3(-radius, radius, radius),
            new Vector3(-radius, radius, -radius),
            new Vector3(radius, radius, radius),
            new Vector3(radius, radius, -radius)
        };
    string[] m_DebugCornerNames = new string[8]
    {
        "Top-left foot",
        "Bottom-left foot",
        "Top-right foot",
        "Bottom-right foot",
        "Top-left head",
        "Bottom-left head",
        "Top-right head",
        "Bottom-right head"
    };

    public bool IsStuck = false;
    public bool IsPenetrating = false;
    public bool IsDebugging = true;

    void Start()
    {
        m_Raycaster = GameManager.Instance.Raycaster;
        m_PlayerController = GetComponent<PlayerController>();
        m_PlayerTransform = m_PlayerController.transform;
    }

    public bool IsCornerPenetrating(Vector3 cornerOffset, ref Vector3 penetrationVector)
    {
        Vector3 cornerWorldLocation = transform.position + cornerOffset;
        Vector3i cornerBlockLocation = new Vector3i(cornerWorldLocation, ((float input) => Mathf.FloorToInt(input)));
        Block cornerBlock;
        if (TerrainManager.Instance.GetBlockAtWorldPos_ReadOnly(cornerBlockLocation.x, cornerBlockLocation.y, cornerBlockLocation.z, out cornerBlock))
        {
            if (cornerBlock.IsSolid(Direction.Down))
            {
                penetrationVector.x = (cornerOffset.x < 0 ? ((cornerBlockLocation.x + 1) - cornerWorldLocation.x) : (cornerWorldLocation.x - cornerBlockLocation.x));
                penetrationVector.y = (cornerOffset.y < 0 ? ((cornerBlockLocation.y + 1) - cornerWorldLocation.y) : (cornerWorldLocation.y - cornerBlockLocation.y));
                penetrationVector.z = (cornerOffset.z < 0 ? ((cornerBlockLocation.z + 1) - cornerWorldLocation.z) : (cornerWorldLocation.z - cornerBlockLocation.z));
                return true;
            }
        }
        return false;
    }

    public bool IsCornerPenetrating(Vector3 cornerOffset, ref Vector3 penetrationVector, out Vector3i cornerBlockLocation)
    {
        Vector3 cornerWorldLocation = transform.position + cornerOffset;
        cornerBlockLocation = new Vector3i(cornerWorldLocation, ((float input) => Mathf.FloorToInt(input)));
        Block cornerBlock;
        if (TerrainManager.Instance.GetBlockAtWorldPos_ReadOnly(cornerBlockLocation.x, cornerBlockLocation.y, cornerBlockLocation.z, out cornerBlock))
        {
            if (cornerBlock.IsSolid(Direction.Down))
            {
                penetrationVector.x = (cornerOffset.x < 0 ? ((cornerBlockLocation.x + 1) - cornerWorldLocation.x) : (cornerWorldLocation.x - cornerBlockLocation.x));
                penetrationVector.y = (cornerOffset.y < 0 ? ((cornerBlockLocation.y + 1) - cornerWorldLocation.y) : (cornerWorldLocation.y - cornerBlockLocation.y));
                penetrationVector.z = (cornerOffset.z < 0 ? ((cornerBlockLocation.z + 1) - cornerWorldLocation.z) : (cornerWorldLocation.z - cornerBlockLocation.z));
                return true;
            }
        }
        return false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Time.timeScale = 0.1f;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Time.timeScale = 1f;
        }
        // 4 points represent the character's feet, forming a square
        // 4 points represent the character's head, forming a square
        // 2 points from the feet and 2 points from the head form a square representing each side of the character
        // check each point's coordinates; its coordinate minus player's coordinate represents its penetration into the neighboring block
        // find the axis with the lowest penetration (for each point??) and push the character that amount in the opposite direction
        Vector3i playerBlockLocation = new Vector3i(transform.position, ((float input) => Mathf.FloorToInt(input)));
        Vector3 penetrationVector = Vector3.one;
        Vector3 tempPenVector = Vector3.one;
        // for each corner, check if that corner is penetrating
        // if it's penetrating less - per axis - than any other corner then assign that to our total penetration vector
        for (int index = 0; index < 8; index++)
        {
            if (IsCornerPenetrating(m_CornerOffsets[index], ref tempPenVector))
            {
                if (Mathf.Abs(tempPenVector.x) < Mathf.Abs(penetrationVector.x)) penetrationVector.x = tempPenVector.x;
                if (Mathf.Abs(tempPenVector.y) < Mathf.Abs(penetrationVector.y)) penetrationVector.y = tempPenVector.y;
                if (Mathf.Abs(tempPenVector.z) < Mathf.Abs(penetrationVector.z)) penetrationVector.z = tempPenVector.z;
                IsPenetrating = true;
                Debug.Log(m_DebugCornerNames[index] + " is penetrating: (" + tempPenVector.x + "," + tempPenVector.y + "," + tempPenVector.z + ") [" + penetrationVector + "]");
            }
        }

        if (IsPenetrating)
        {
            // TO-DO:
            // i think this is where we would do "sliding" physics too?
            // might need more information for the sliding physics. do research.

            // move the player back according to penetrationVector
            if (!IsDebugging)
                transform.position += penetrationVector;
            Debug.Log("Penetration detected, adjusted player position by " + penetrationVector);
        }
        IsPenetrating = false;
    }
}