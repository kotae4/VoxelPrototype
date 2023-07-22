// The MIT License (MIT)
//
// Copyright (c) 2012-2013 Mikola Lysenko
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VoxelPrototype.Lighting;
using Utilities;


namespace VoxelPrototype.MeshGenerators
{
    // NOTE: only partially implemented. probably broken.
    public class GreedyMeshGenerator
    {
        public static Mesh GenerateMesh(Chunk chunk)
        {
            Block[,,] blocks = chunk.blocks;

            /*
 * These are just working variables for the algorithm - almost all taken 
 * directly from Mikola Lysenko's javascript implementation.
 */
            int i, j, k, l, w, h, u, v, n = 0;

            int[] x = new int[] { 0, 0, 0 };
            int[] q = new int[] { 0, 0, 0 };
            int[] du = new int[] { 0, 0, 0 };
            int[] dv = new int[] { 0, 0, 0 };

            /*
             * We create a mask - this will contain the groups of matching voxel faces 
             * as we proceed through the chunk in 6 directions - once for each face.
             */
            IBlockType[] mask = new IBlockType[Settings.ChunkSizeX * Settings.ChunkSizeY];
            IntVec3[] maskLocations = new IntVec3[Settings.ChunkSizeX * Settings.ChunkSizeY];

            /*
             * These are just working variables to hold two faces during comparison.
             */
            IBlockType voxelFace, voxelFace1;

            Direction side = (Direction)0;

            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            List<Vector3> uvs = new List<Vector3>();
            List<Vector2> lightData = new List<Vector2>();

            /**
             * We start with the lesser-spotted boolean for-loop (also known as the old flippy floppy). 
             * 
             * The variable backFace will be TRUE on the first iteration and FALSE on the second - this allows 
             * us to track which direction the indices should run during creation of the quad.
             * 
             * This loop runs twice, and the inner loop 3 times - totally 6 iterations - one for each 
             * voxel face.
             */
            for (bool backFace = true, b = false; b != backFace; backFace = backFace && b, b = !b)
            {

                /*
                 * We sweep over the 3 dimensions - most of what follows is well described by Mikola Lysenko 
                 * in his post - and is ported from his Javascript implementation.  Where this implementation 
                 * diverges, I've added commentary.
                 */
                for (int d = 0; d < 3; d++)
                {

                    u = (d + 1) % 3;
                    v = (d + 2) % 3;

                    x[0] = 0;
                    x[1] = 0;
                    x[2] = 0;

                    q[0] = 0;
                    q[1] = 0;
                    q[2] = 0;
                    q[d] = 1;

                    /*
                     * Here we're keeping track of the side that we're meshing.
                     */
                    if (d == 0) { side = (backFace ? Direction.West : Direction.East); }
                    else if (d == 1) { side = (backFace ? Direction.Down : Direction.Up); }
                    else if (d == 2) { side = (backFace ? Direction.South : Direction.North); }

                    /*
                     * We move through the dimension from front to back
                     */
                    for (x[d] = -1; x[d] < Settings.ChunkSizeX;)
                    {

                        /*
                         * -------------------------------------------------------------------
                         *   We compute the mask
                         * -------------------------------------------------------------------
                         */
                        n = 0;

                        for (x[v] = 0; x[v] < Settings.ChunkSizeY; x[v]++)
                        {

                            for (x[u] = 0; x[u] < Settings.ChunkSizeX; x[u]++)
                            {

                                /*
                                 * Here we retrieve two voxel faces for comparison.
                                 */
                                voxelFace = (x[d] >= 0) ? BlockTypes.GetBlockType(blocks[x[0], x[1], x[2]].BlockTypeID) : null;
                                voxelFace1 = (x[d] < Settings.ChunkSizeX - 1) ? BlockTypes.GetBlockType(blocks[x[0] + q[0], x[1] + q[1], x[2] + q[2]].BlockTypeID) : null;
                                /*
                                 * Note that we're using the equals function in the voxel face class here, which lets the faces 
                                 * be compared based on any number of attributes.
                                 * 
                                 * Also, we choose the face to add to the mask depending on whether we're moving through on a backface or not.
                                 */

                                // TO-DO:
                                // add light value comparison (already contained in Block class, but *not* IBlock interface)
                                if ((voxelFace != null)
                                    && (voxelFace1 != null)
                                    && (voxelFace.Equals(voxelFace1)))
                                {
                                    mask[n++] = null;
                                }
                                else
                                {
                                    if (backFace)
                                    {
                                        if ((x[d] < Settings.ChunkSizeX - 1) && (blocks[x[0] + q[0], x[1] + q[1], x[2] + q[2]].IsVisible((byte)side)))
                                        {
                                            maskLocations[n].X = x[0] + q[0];
                                            maskLocations[n].Y = x[1] + q[1];
                                            maskLocations[n].Z = x[2] + q[2];
                                            mask[n++] = voxelFace1;
                                        }
                                        else
                                            mask[n++] = null;
                                    }
                                    else
                                    {
                                        if ((x[d] >= 0) && (blocks[x[0], x[1], x[2]].IsVisible((byte)side)))
                                        {
                                            maskLocations[n].X = x[0];
                                            maskLocations[n].Y = x[1];
                                            maskLocations[n].Z = x[2];
                                            mask[n++] = voxelFace;
                                        }
                                        else
                                            mask[n++] = null;
                                    }
                                    /*
                                    if (((blocks[x[0] + q[0], x[1] + q[1], x[2] + q[2]].IsVisible((byte)Direction.East)) && (blocks[x[0], x[1], x[2]].IsVisible((byte)Direction.East))))
                                    {
                                        maskLocations[n].X = backFace ? x[0] + q[0] : x[0];
                                        maskLocations[n].Y = backFace ? x[1] + q[1] : x[1];
                                        maskLocations[n].Z = backFace ? x[2] + q[2] : x[2];
                                        mask[n++] = backFace ? voxelFace1 : voxelFace;
                                    }
                                    else
                                        mask[n++] = null;
                                        */
                                }

                                /*
                                   mask[n++] = ((voxelFace != null && voxelFace1 != null && voxelFace.Equals(voxelFace1)))
                                           ? null
                                           : backFace ? voxelFace1 : voxelFace;
                                  */
                            }
                        }

                        x[d]++;

                        /*
                         * Now we generate the mesh for the mask
                         */
                        n = 0;

                        for (j = 0; j < Settings.ChunkSizeY; j++)
                        {

                            for (i = 0; i < Settings.ChunkSizeX;)
                            {

                                if (mask[n] != null)
                                {

                                    /*
                                     * We compute the width
                                     */
                                    for (w = 1; i + w < Settings.ChunkSizeX && mask[n + w] != null && mask[n + w].Equals(mask[n]); w++) { }

                                    /*
                                     * Then we compute height
                                     */
                                    bool done = false;

                                    for (h = 1; j + h < Settings.ChunkSizeY; h++)
                                    {

                                        for (k = 0; k < w; k++)
                                        {

                                            if (mask[n + k + h * Settings.ChunkSizeX] == null || !mask[n + k + h * Settings.ChunkSizeX].Equals(mask[n])) { done = true; break; }
                                        }

                                        if (done) { break; }
                                    }
                                    /*
                                     * Here we check the "transparent" attribute in the VoxelFace class to ensure that we don't mesh 
                                     * any culled faces.
                                     */
                                    if ((mask[n].BlockTypeID != (byte)BlockID.Air) && (blocks[maskLocations[n].X, maskLocations[n].Y, maskLocations[n].Z].IsVisible((byte)side)))
                                    //if (mask[n].BlockTypeID != (byte)BlockID.Air)
                                    {
                                        /*
                                         * Add quad
                                         */
                                        x[u] = i;
                                        x[v] = j;

                                        du[0] = 0;
                                        du[1] = 0;
                                        du[2] = 0;
                                        du[u] = w;

                                        dv[0] = 0;
                                        dv[1] = 0;
                                        dv[2] = 0;
                                        dv[v] = h;

                                        /*
                                         * And here we call the quad function in order to render a merged quad in the scene.
                                         * 
                                         * We pass mask[n] to the function, which is an instance of the VoxelFace class containing 
                                         * all the attributes of the face - which allows for variables to be passed to shaders - for 
                                         * example lighting values used to create ambient occlusion.
                                         */
                                        quad(new Vector3(x[0], x[1], x[2]),
                                             new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]),
                                             new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]),
                                             new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]),
                                             side,
                                             w,
                                             h,
                                             mask[n],
                                             backFace,
                                             vertices,
                                             indices,
                                             uvs,
                                             lightData);
                                        // vertexLighting array is formatted like so:
                                        // top left, top right, bottom right, bottom left
                                        byte[] vertexLighting;
                                        if (LightProcessor.GetVertexLightingForFace(maskLocations[n].X, maskLocations[n].Y, maskLocations[n].Z, side, out vertexLighting))
                                        {
                                            // top left
                                            lightData.Add(new Vector2((vertexLighting[0] / Settings.SUNLIGHT_VALUE), 0f));
                                            // top right
                                            lightData.Add(new Vector2((vertexLighting[1] / Settings.SUNLIGHT_VALUE), 0f));
                                            // bottom right
                                            lightData.Add(new Vector2((vertexLighting[2] / Settings.SUNLIGHT_VALUE), 0f));
                                            // bottom left
                                            lightData.Add(new Vector2((vertexLighting[3] / Settings.SUNLIGHT_VALUE), 0f));
                                        }
                                        /*
                                        Vector3 v1 = new Vector3(x[0], x[1], x[2]);
                                        Vector3 v2 = new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]);
                                        Vector3 v3 = new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]);
                                        Vector3 v4 = new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]);

                                        vertices.AddRange(new Vector3[]{ v1, v4, v2, v3});
                                        if (backFace) indices.AddRange(new int[] { 2, 0, 1, 1, 3, 2 });
                                        else indices.AddRange(new int[] { 2, 3, 1, 1, 0, 2 });
                                        */
                                    }
                                    /*
                                     * We zero out the mask
                                     */
                                    for (l = 0; l < h; ++l)
                                    {

                                        for (k = 0; k < w; ++k)
                                        {
                                            mask[n + k + l * Settings.ChunkSizeX] = null;
                                            maskLocations[n + k + l * Settings.ChunkSizeX].X = -1;
                                            maskLocations[n + k + l * Settings.ChunkSizeX].Y = -1;
                                            maskLocations[n + k + l * Settings.ChunkSizeX].Z = -1;
                                        }
                                    }

                                    /*
                                     * And then finally increment the counters and continue
                                     */
                                    i += w;
                                    n += w;

                                }
                                else
                                {

                                    i++;
                                    n++;
                                }
                            }
                        }
                    }
                }
            }

            Mesh retVal = new Mesh();
            retVal.SetVertices(vertices);
            retVal.SetTriangles(indices, 0);
            retVal.SetUVs(0, uvs);
            retVal.SetUVs(1, lightData);
            return retVal;
        }

        static void quad(Vector3 bottomLeft,
          Vector3 topLeft,
          Vector3 topRight,
          Vector3 bottomRight,
          Direction side,
          float width,
          float height,
          IBlockType voxel,
          bool backFace,
          List<Vector3> vertices,
          List<int> indices,
          List<Vector3> uvs,
          List<Vector2> lightData)
        {
            Vector3[] verts = new Vector3[4];

            verts[0] = topLeft;
            verts[1] = topRight;
            verts[2] = bottomLeft;
            verts[3] = bottomRight;

            /* Up / East North/South   West
             * 0---1     2--------0    1---0
             * | \ |     |        |    |   |
             * |  \|  -> |        | -> |   |
             * 2---3     3--------1    3---2
             * Unity uses CW winding order. (2, 0, 1) and (1, 3, 2) for front face.
             * */

            int numExistingQuads = vertices.Count / 4;
            int[] indexes = (!backFace) ? new int[]
            { 0 + (4 * numExistingQuads), 3 + (4 * numExistingQuads), 2 + (4 * numExistingQuads),
            0 + (4 * numExistingQuads), 1 + (4 * numExistingQuads), 3 + (4 * numExistingQuads)
            } : new int[]
            { 0 + (4 * numExistingQuads), 2 + (4 * numExistingQuads), 3 + (4 * numExistingQuads),
            0 + (4 * numExistingQuads), 3 + (4 * numExistingQuads), 1 + (4 * numExistingQuads) };
            /*
            Debug.Log("Face: " + side + "\nWidth: " + width + " Height: " + height +
                "\nTopLeft: " + topLeft + " \nBottomLeft: " + bottomLeft + "\nTopRight: " + topRight + "\nTopLeft: " + topLeft);
            */
            Vector3[] texcoords = new Vector3[4];
            byte voxelTextureLayer = voxel.GetTextureLayer(side);
            switch (side)
            {
                case Direction.Up:
                case Direction.East:
                    {
                        texcoords[0] = new Vector3(0f, 1f * width, voxelTextureLayer);
                        texcoords[1] = new Vector3(1f * height, 1f * width, voxelTextureLayer);
                        texcoords[2] = new Vector3(0f, 0f, voxelTextureLayer);
                        texcoords[3] = new Vector3(1f * height, 0f, voxelTextureLayer);
                        break;
                    }
                case Direction.Down:
                    {
                        // this puts 0 in the bottom right
                        texcoords[0] = new Vector3(0f, 1f * width, voxelTextureLayer);
                        texcoords[1] = new Vector3(1f * height, 1f * width, voxelTextureLayer);
                        texcoords[2] = new Vector3(0f, 0f, voxelTextureLayer);
                        texcoords[3] = new Vector3(1f * height, 0f, voxelTextureLayer);
                        // this puts 0 in the top right
                        /*
                        texcoords[0] = new Vector3(1f * width, 0f, voxelTextureLayer);
                        texcoords[1] = new Vector3(0f, 0f, voxelTextureLayer);
                        texcoords[2] = new Vector3(1f * width, 1f * height, voxelTextureLayer);
                        texcoords[3] = new Vector3(0f, 1f * height, voxelTextureLayer);
                        */
                        // this works but the texture mapping is blurry on 1 and 2 widths
                        /*
                        texcoords[0] = new Vector3(0f, 0f, voxelTextureLayer);
                        texcoords[1] = new Vector3(1f * width, 0f, voxelTextureLayer);
                        texcoords[2] = new Vector3(0f, 1f * height, voxelTextureLayer);
                        texcoords[3] = new Vector3(1f * width, 1f * height, voxelTextureLayer);
                        */
                        break;
                    }
                case Direction.West:
                    {
                        texcoords[0] = new Vector3(1f * height, 1f * width, voxelTextureLayer);
                        texcoords[1] = new Vector3(0f, 1f * width, voxelTextureLayer);
                        texcoords[2] = new Vector3(1f * height, 0f, voxelTextureLayer);
                        texcoords[3] = new Vector3(0f, 0f, voxelTextureLayer);
                        break;
                    }
                case Direction.South:
                    {
                        // [{0,0}, {0,1}, {1,0}, {1,1}] -> weird stretching
                        // [{0,1}, {1,0}, {1,1}, {0,0}] -> weird stretching
                        // [{1,0}, {1,1}, {0,0}, {0,1}] -> weird stretching
                        // [{1,1}, {0,0}, {0,1}, {1,0}] -> weird stretching
                        // default has the grass side going vertically...
                        // this has the grass side at the bottom
                        /*
                        texcoords[0] = new Vector3(1f * width, 1f * height, voxelTextureLayer);
                        texcoords[1] = new Vector3(1f * width, 0f, voxelTextureLayer);
                        texcoords[2] = new Vector3(0f, 1f * height, voxelTextureLayer);
                        texcoords[3] = new Vector3(0f, 0f, voxelTextureLayer);
                        */
                        // this works
                        texcoords[0] = new Vector3(1f * width, 0f, voxelTextureLayer);
                        texcoords[1] = new Vector3(1f * width, 1f * height, voxelTextureLayer);
                        texcoords[2] = new Vector3(0f, 0f, voxelTextureLayer);
                        texcoords[3] = new Vector3(0f, 1f * height, voxelTextureLayer);

                        break;
                    }
                case Direction.North:
                    {
                        // this puts 0 on the top right
                        /*
                        texcoords[0] = new Vector3(1f * width, 0f, voxelTextureLayer);
                        texcoords[1] = new Vector3(1f * width, 1f * height, voxelTextureLayer);
                        texcoords[2] = new Vector3(0f, 0f, voxelTextureLayer);
                        texcoords[3] = new Vector3(0f, 1f * height, voxelTextureLayer);
                        // this puts 0 on the bottom right
                        texcoords[0] = new Vector3(1f * width, 1f * height, voxelTextureLayer);
                        texcoords[1] = new Vector3(1f * width, 0f, voxelTextureLayer);
                        texcoords[2] = new Vector3(0f, 1f * height, voxelTextureLayer);
                        texcoords[3] = new Vector3(0f, 0f, voxelTextureLayer);
                        */
                        // this works
                        texcoords[0] = new Vector3(0f, 0f, voxelTextureLayer);
                        texcoords[1] = new Vector3(0f, 1f * height, voxelTextureLayer);
                        texcoords[2] = new Vector3(1f * width, 0f, voxelTextureLayer);
                        texcoords[3] = new Vector3(1f * width, 1f * height, voxelTextureLayer);
                        break;
                    }
                default:
                    {
                        texcoords[0] = new Vector3(0f, 1f * height, voxelTextureLayer);
                        texcoords[1] = new Vector3(1f * width, 1f * height, voxelTextureLayer);
                        texcoords[2] = new Vector3(0f, 0f, voxelTextureLayer);
                        texcoords[3] = new Vector3(1f * width, 0f, voxelTextureLayer);
                        break;
                    }
            }

            vertices.AddRange(verts);
            indices.AddRange(indexes);
            uvs.AddRange(texcoords);
            //tileInfo.AddRange(tileData);
        }
    }
}
