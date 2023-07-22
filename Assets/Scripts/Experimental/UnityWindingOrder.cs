using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class UnityWindingOrder : MonoBehaviour
{
    MeshFilter m_MeshFilter;

    private void Start()
    {
        m_MeshFilter = GetComponent<MeshFilter>();

        Mesh newMesh = new Mesh();
        m_MeshFilter.mesh = newMesh;


        /* 0---1
         * |  /|
         * | / |
         * 2---3
         * */

        Vector3[] verts = new Vector3[4];
        verts[0] = new Vector3(0f, 5f, 0f);
        verts[1] = new Vector3(5f, 5f, 0f);
        verts[2] = Vector3.zero;
        verts[3] = new Vector3(5f, 0f, 0f);

        int[] indices = new int[6] { 2, 1, 0,
                                     2, 3, 1 };

        /* Results:
         * 0,1,2 works 
         * 2,0,1 works
         * 1,2,0 works
         * 
         * 2,1,0 DOES NOT work
         * 
         * 2,1,3 works
         * 1,3,2 works
         * 3,2,1 works
         * 
         * 2,3,1 DOES NOT work
         * 
         * Conclusion:
         * So literally make a clock motion with your mouse. Move from one vertex to the next mimicking the motion of a clock hand.
         * If you cannot go from any one vertex to any other using the motion of a clock hand then it won't work.
         * Additionally, when it doesn't work Unity won't display any part of the mesh, even in scene view w/ wireframe
         */

        newMesh.vertices = verts;
        newMesh.triangles = indices;
    }
}