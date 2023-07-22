using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class UI_DebugMenu : MonoBehaviour
{

    [SerializeField]
    Material m_HighlightMaterial;
    [SerializeField]
    InputField m_InputX, m_InputY, m_InputZ;

    int x, y, z;
    bool hasCoords = false;
    bool showingMenu = false;

    public void btnDrawOutline_OnClick()
    {
        if ((!int.TryParse(m_InputX.text, out x)) || (!int.TryParse(m_InputY.text, out y)) || (!int.TryParse(m_InputZ.text, out z)))
        {
            Debug.LogError("Error: Could not parse x,y,z input coordinates.");
            return;
        }
        hasCoords = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showingMenu = !showingMenu;
            Cursor.lockState = showingMenu ? CursorLockMode.Confined : CursorLockMode.Locked;
            Cursor.visible = showingMenu ? true : false;
        }
        if (!hasCoords)
            return;
        DrawOutlineFace(new Vector3i(x, y, z), new Vector3i(1, 0, 0));
        DrawOutlineFace(new Vector3i(x, y, z), new Vector3i(-1, 0, 0));
        DrawOutlineFace(new Vector3i(x, y, z), new Vector3i(0, 1, 0));
        DrawOutlineFace(new Vector3i(x, y, z), new Vector3i(0, -1, 0));
        DrawOutlineFace(new Vector3i(x, y, z), new Vector3i(0, 0, 1));
        DrawOutlineFace(new Vector3i(x, y, z), new Vector3i(0, 0, -1));
    }

    void DrawOutlineFace(Vector3i position, Vector3i face)
    {
        Mesh outlineMesh = new Mesh();
        // linestrip, so 4 vertices to form a 2d box and 5 indices
        Vector3[] verts = new Vector3[4];
        int[] indices = new int[5] { 0, 1, 2, 3, 0 };
        if (face.x == 1)
        {
            verts[0] = new Vector3(0.5f, -0.5f, -0.5f);
            verts[1] = new Vector3(0.5f, 0.5f, -0.5f);
            verts[2] = new Vector3(0.5f, 0.5f, 0.5f);
            verts[3] = new Vector3(0.5f, -0.5f, 0.5f);
        }
        else if (face.x == -1)
        {
            verts[0] = new Vector3(-0.5f, -0.5f, 0.5f);
            verts[1] = new Vector3(-0.5f, 0.5f, 0.5f);
            verts[2] = new Vector3(-0.5f, 0.5f, -0.5f);
            verts[3] = new Vector3(-0.5f, -0.5f, -0.5f);
        }
        else if (face.y == 1)
        {
            verts[0] = new Vector3(-0.5f, 0.5f, -0.5f);
            verts[1] = new Vector3(-0.5f, 0.5f, 0.5f);
            verts[2] = new Vector3(0.5f, 0.5f, 0.5f);
            verts[3] = new Vector3(0.5f, 0.5f, -0.5f);
        }
        else if (face.y == -1)
        {
            verts[0] = new Vector3(-0.5f, -0.5f, 0.5f);
            verts[1] = new Vector3(-0.5f, -0.5f, -0.5f);
            verts[2] = new Vector3(0.5f, -0.5f, -0.5f);
            verts[3] = new Vector3(0.5f, -0.5f, 0.5f);
        }
        else if (face.z == 1)
        {
            verts[0] = new Vector3(0.5f, -0.5f, 0.5f);
            verts[1] = new Vector3(0.5f, 0.5f, 0.5f);
            verts[2] = new Vector3(-0.5f, 0.5f, 0.5f);
            verts[3] = new Vector3(-0.5f, -0.5f, 0.5f);
        }
        else if (face.z == -1)
        {
            verts[0] = new Vector3(-0.5f, -0.5f, -0.5f);
            verts[1] = new Vector3(-0.5f, 0.5f, -0.5f);
            verts[2] = new Vector3(0.5f, 0.5f, -0.5f);
            verts[3] = new Vector3(0.5f, -0.5f, -0.5f);
        }
        outlineMesh.vertices = verts;
        outlineMesh.SetIndices(indices, MeshTopology.LineStrip, 0);
        Graphics.DrawMesh(outlineMesh, position, Quaternion.identity, m_HighlightMaterial, 0);
    }
}