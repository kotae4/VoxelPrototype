// CREDITS:
// This entire system (including CollisionExp.cs) is sourced from: https://paginas.fe.up.pt/~ei12085/blog/aabb_collision_handling.php
// Author: Luis Eduardo Reis, BrendanL.K, Kenton Hamaluik et al.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utilities;

namespace Experimental.Physics.Colliders
{
    // TO-DO:
    // change to base controller when AI controllers are implemented
    [RequireComponent(typeof(PlayerController))]
    public class ExpVoxelCollider : MonoBehaviour
    {
        [SerializeField]
        bool shouldCollideWithLevel = true;

        PlayerController m_Controller;

        private void Start()
        {
            m_Controller = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (!shouldCollideWithLevel) return;

            // TODO revisit this and get rid of infinite-looking loop
            while(true)
            {
                // calculate the movement vector for this frame
                // current position - previous position [previous position is retrieved from entity controller component]
                Vector3 deltaPos = transform.position - m_Controller.PreviousPosition;
                // TO-DO:
                // hard-coded 0.5f should be (entitySize.x / 2)
                Vector3i minAABBBounds = new Utilities.Vector3i(Mathf.FloorToInt(Mathf.Min(transform.position.x, m_Controller.PreviousPosition.x) - 0.5f),
                    Mathf.FloorToInt(Mathf.Min(transform.position.y, m_Controller.PreviousPosition.y) - 0.5f),
                    Mathf.FloorToInt(Mathf.Min(transform.position.z, m_Controller.PreviousPosition.z) - 0.5f));

                Vector3i maxAABBBounds = new Vector3i(Mathf.FloorToInt(Mathf.Max(transform.position.x, m_Controller.PreviousPosition.x) + 0.5f),
                    Mathf.FloorToInt(Mathf.Max(transform.position.y, m_Controller.PreviousPosition.y) + 0.5f),
                    Mathf.FloorToInt(Mathf.Max(transform.position.z, m_Controller.PreviousPosition.z) + 0.5f));

                float penetration = 1f, tempPenetration;
                Vector3 hitNormal = Vector3.zero, tempHitNormal;
                Block workingBlock;

                for (int y = minAABBBounds.y; y <= maxAABBBounds.y; y++)
                {
                    for (int z = minAABBBounds.z; z <= maxAABBBounds.z; z++)
                    {
                        for (int x = minAABBBounds.x; x <= maxAABBBounds.x; x++)
                        {
                            if (TerrainManager.Instance.GetBlockAtWorldPos_ReadOnly(x,y,z, out workingBlock))
                            {
                                if (!workingBlock.IsSolid(Direction.Down)) continue;
                            }
                            else
                            {
                                Debug.LogWarning("Collider tried getting nonexistent block\n[Min: " + minAABBBounds + "\nMax: " + maxAABBBounds + "\nCur: (" + x + "," + y + "," + z + ")]");
                                continue;
                            }
                            // TO-DO:
                            // hard-coded 0.5f should be entitySize / 2
                            // second argument should be entitySize, but it's 1 for now. fourth argument remains Vector3.one.
                            tempPenetration = CollisionExp.sweepAABB(
                                new Vector3(m_Controller.PreviousPosition.x - 0.5f, m_Controller.PreviousPosition.y - 0.5f, m_Controller.PreviousPosition.z - 0.5f),
                                Vector3.one,
                                new Vector3(x, y, z),
                                Vector3.one,
                                deltaPos,
                                out tempHitNormal);

                            if (tempPenetration < penetration)
                            {
                                penetration = tempPenetration;
                                hitNormal = tempHitNormal;
                            }
                        }
                    }
                }

                float epsilon = 0.001f;
                //Vector3 debugPos = transform.position;
                //Debug.Log("Current position: " + transform.position);
                transform.position = new Vector3(
                    m_Controller.PreviousPosition.x + (penetration * deltaPos.x) + (epsilon * hitNormal.x),
                    m_Controller.PreviousPosition.y + (penetration * deltaPos.y) + (epsilon * hitNormal.y),
                    m_Controller.PreviousPosition.z + (penetration * deltaPos.z) + (epsilon * hitNormal.z));
                //Debug.Log("New position: " + transform.position);
                //debugPos -= transform.position;
                //Debug.Log("Change: " + debugPos + "\nPenetration: " + penetration + "\nhitNormal: " + hitNormal);

                if (penetration == 1f) break;

                // wall sliding


            }
        }
    }
}
