using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Utilities;

[CustomEditor(typeof(SimpleVoxelCollider))]
public class EditorColliderVisualizer : Editor
{
    private void OnSceneGUI()
    {
        SimpleVoxelCollider col = (SimpleVoxelCollider)target;
        Transform transform = col.transform;
        float radius = SimpleVoxelCollider.radius;
        Vector3[] cornerOffsets = new Vector3[8] {
            new Vector3(-radius, -radius, radius),
            new Vector3(-radius, -radius, -radius),
            new Vector3(radius, -radius, radius),
            new Vector3(radius, -radius, -radius),
            new Vector3(-radius, radius, radius),
            new Vector3(-radius, radius, -radius),
            new Vector3(radius, radius, radius),
            new Vector3(radius, radius, -radius)
        };
        Color[] cornerColors = new Color[8] { Color.black, Color.blue, Color.blue, Color.blue, Color.cyan, Color.cyan, Color.cyan, Color.cyan };
        Vector3[] cornerPenetrations = new Vector3[8]
        {
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
        };
        Vector3i[] cornerBlocks = new Vector3i[8]
        {
            Vector3i.zero,
            Vector3i.zero,
            Vector3i.zero,
            Vector3i.zero,
            Vector3i.zero,
            Vector3i.zero,
            Vector3i.zero,
            Vector3i.zero,
        };
        Handles.color = Color.magenta;
        Handles.DrawWireCube(transform.position, new Vector3(radius*2, radius*2, radius*2));

        if (col.IsDebugging)
        {
            for (int index = 0; index < 8; index++)
            {
                if (col.IsCornerPenetrating(cornerOffsets[index], ref cornerPenetrations[index], out cornerBlocks[index]))
                {
                    cornerColors[index] = Color.red;
                    Time.timeScale = 0f;
                    // lines to the corner block location
                    Handles.color = Color.red;
                    Handles.DrawLine(transform.position + cornerOffsets[index], cornerBlocks[index]);
                }
            }
        }

        if (Event.current.type == EventType.Repaint)
        {
            Vector3 cornerWorldLocation;
            float dotSize = 0.01f;
            float arrowSize = 0.8f;

            // center body
            Handles.color = Color.yellow;
            Handles.DotHandleCap(0, transform.position, Quaternion.identity, 0.1f, EventType.Repaint);

            // corners
            for (int index = 0; index < 8; index++)
            {
                // dots for corner locations
                Handles.color = cornerColors[index];
                cornerWorldLocation = transform.position + cornerOffsets[index];
                Handles.DotHandleCap(
                    0,
                    cornerWorldLocation,
                    Quaternion.identity,
                    dotSize,
                    EventType.Repaint
                    );
                // arrows for penetration
                Handles.color = Color.white;
                if (cornerPenetrations[index] != Vector3.zero)
                {
                    if (cornerPenetrations[index].x != 0f)
                    {
                        Handles.ArrowHandleCap(
                            0,
                            transform.position + cornerOffsets[index],
                            Quaternion.LookRotation(cornerPenetrations[index].x > 0f ? Vector3.right : Vector3.left),
                            arrowSize * cornerPenetrations[index].x,
                            EventType.Repaint
                            );
                    }
                    if (cornerPenetrations[index].y != 0f)
                    {
                        Handles.ArrowHandleCap(
                            0,
                            transform.position + cornerOffsets[index],
                            Quaternion.LookRotation(cornerPenetrations[index].y > 0f ? Vector3.up : Vector3.down),
                            arrowSize * cornerPenetrations[index].y,
                            EventType.Repaint
                            );
                    }
                    if (cornerPenetrations[index].z != 0f)
                    {
                        Handles.ArrowHandleCap(
                            0,
                            transform.position + cornerOffsets[index],
                            Quaternion.LookRotation(cornerPenetrations[index].z > 0f ? Vector3.forward : Vector3.back),
                            arrowSize * cornerPenetrations[index].z,
                            EventType.Repaint
                            );
                    }
                }
            }
        }
    }   
}