using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Equipment;

namespace Game
{
    public class BaseResource
    {
        // the world prefab index, used to retrieve prefab from GameManager
        public string ResourcePath = "";
        public string FriendlyName = "";

        [NonSerialized]
        internal GameObject WorldGO;
    }
}