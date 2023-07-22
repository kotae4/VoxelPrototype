using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Game.Equipment;

namespace Game
{
    public interface IEquippable : IBinarySerializable, IDisplayable, IStackable
    {
        GameObject WorldPrefab { get; }
        // the displayed name of the item
        string Name { get; }
        // the slot (if any) that the item can be equipped in
        EquipSlot Slot { get; }
        // the displayed description of the item
        string Description { get; }

        // used by the inventory system to deep-copy and assign a new UID. deep-copying prevents many design compromises and dupe exploits.
        IEquippable GetCopy(bool generateUID = false);
    }
}