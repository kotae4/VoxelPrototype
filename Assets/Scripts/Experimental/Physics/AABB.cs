using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Experimental.Physics
{
    public class AABB
    {
        Vector3 min, max;
        Bounds test;

        bool isPointInsideAABB(Vector3 point)
        {
            
            return (point.x >= test.min.x && point.x <= test.max.x) &&
                   (point.y >= test.min.y && point.y <= test.max.y) &&
                   (point.z >= test.min.z && point.z <= test.max.z);
        }
    }
}
