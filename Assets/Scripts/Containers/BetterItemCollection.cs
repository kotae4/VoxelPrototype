using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Game;
using System.IO;

[System.Serializable]
public class BetterItemCollection : BetterCollection<BaseItem>
{
    public BetterItemCollection() : base() { }

    public BetterItemCollection(int capacity) : base(capacity) { }

    private bool DistributeStack(BaseItem item)
    {
        int itemsToCreate = item.StackSize;
        List<BaseItem> existingItems = FindAll_ByID(item.ID);
        bool bOverflow = false;
        foreach (BaseItem existingItem in existingItems)
        {
            if (existingItem.StackSize == existingItem.MaxStackSize)
            {
                bOverflow = true;
                continue;
            }
            if ((existingItem.StackSize + itemsToCreate) > existingItem.MaxStackSize)
            {
                itemsToCreate -= (existingItem.MaxStackSize - existingItem.StackSize);
                existingItem.StackSize = existingItem.MaxStackSize;
                bOverflow = true;
                OnChanged(this, existingItem, false, true);
                continue;
            }
            else
            {
                // we can fit it all into this existingItem's stack
                existingItem.StackSize += itemsToCreate;
                itemsToCreate = 0;
                bOverflow = false;
                OnChanged(this, existingItem, false, true);
                return true;
            }
        }
        if (bOverflow)
        {
            int newStacks = Mathf.CeilToInt((float)itemsToCreate / (float)item.MaxStackSize);
            BaseItem copyItem;
            bool bFirstRun = true;
            for (int counter = newStacks; counter > 0; counter--)
            {
                if (_Current + 1 <= _Capacity)
                {
                    // for the first run, we add the item we're passed, keeping its UID
                    // for every run after, we're creating an entirely new item (with its own UID)
                    if (bFirstRun)
                        copyItem = item;
                    else
                        copyItem = (BaseItem)item.GetCopy(true);
                    bFirstRun = false;
                    if (counter - 1 == 0)
                    {
                        if (itemsToCreate == item.MaxStackSize)
                            (copyItem as IStackable).StackSize = item.MaxStackSize;
                        else
                            (copyItem as IStackable).StackSize = itemsToCreate % item.MaxStackSize;
                    }
                    else
                    {
                        (copyItem as IStackable).StackSize = item.MaxStackSize;
                    }
                    itemsToCreate -= (copyItem as IStackable).StackSize;
                    _Collection[_Current++] = copyItem;
                    _Count++;
                    OnChanged(this, copyItem, false, false);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            // this is the first item of this type being added
            // so we add a copy of the item we're passed, including the UID
            if (_Current + 1 <= _Capacity)
            {
                _Collection[_Current++] = item;
                _Count++;
                OnChanged(this, item, false, false);
                return true;
            }
        }
        return false;
    }

    public override bool Add(BaseItem item)
    {
        if (item == null)
        {
            Debug.LogError("Error (tried adding null item to container)");
            return false;
        }
        if (item.MaxStackSize > 1)
        {
            return DistributeStack(item);
        }
        else if (_Current + 1 <= _Capacity)
        {
            _Collection[_Current++] = item;
            _Count++;
            OnChanged(this, item, false, false);
            return true;
        }
        return false;
    }

    public int GetTotalQuantity(int itemID)
    {
        int quantity = 0;
        foreach (IEquippable item in _Collection)
        {
            // TO-DO:
            // modify the deserialization to allow this minor optimization below
            // (the problem is that Equipment needs the nulls inbetween, whereas Inventory *can't* have nulls in between)
            /*
            if (item == null)
                break;
            */
            // TO-DO:
            // when the above is accomplished, remove this next line
            if (item == null) continue;
            if (item.ID == itemID)
            {
                quantity += item.StackSize;
            }
        }
        return quantity;
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(_Capacity);
        writer.Write(_Current);
        //writer.Write(_Count);
        int index;
        for (index = 0; index < _Capacity; index++)
        {
            if (_Collection[index] == null)
            {
                writer.Write((int)-1);
            }
            else
            {
                _Collection[index].Serialize(writer);
            }
        }
    }

    public override void Deserialize(BinaryReader reader)
    {
        // after reading Capacity, be sure to initialize _Collection
        int capacity = reader.ReadInt32();
        int current = reader.ReadInt32();
        //int count = reader.ReadInt32();
        _Collection = new BaseItem[capacity];
        int index;
        for (index = 0; index < capacity; index++)
        {
            int itemID = reader.ReadInt32();
            if (itemID == -1)
            {
                this[index] = null;
                _Current -= (index + 1);
            }
            else
            {
                // rewind position in stream
                reader.BaseStream.Position -= 4;
                // now deserialize
                BaseItem newBaseItem = new BaseItem();
                newBaseItem.Deserialize(reader);
                this[index] = newBaseItem;
            }
        }
        _Current = current;
        //_Count = count;
    }
}