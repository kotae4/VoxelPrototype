using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Experimental.Physics;
using Utilities;
using UnityEngine.UI;

namespace Game.Equipment
{
    public enum EquipSlot : byte
    {
        None,
        Head,
        Chest,
        Legs,
        Feet,
        Offhand,
        Ring
    }

    public class PlayerEquipment : MonoBehaviour
    {
        [SerializeField]
        Transform[] m_EquipmentBoneTransforms;

        // TO-DO:
        // do all this when armor is added
    }
}