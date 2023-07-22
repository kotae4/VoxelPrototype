using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Game;
using System.Collections;

namespace Game
{
    public class BetterCollection<T> : IBetterList<T> where T : class, IDisplayable
    {
        public delegate void OnCollectionChanged(BetterCollection<T> sender, T item, bool removed, bool stackUpdate);
        public delegate void OnCollectionCleared(BetterCollection<T> sender);
        protected T[] _Collection;
        protected int _Current = 0;
        protected int _Count = 0;
        protected int _Capacity;
        public int Capacity { get { return _Capacity; } }
        /// <summary>
        /// Always set to the furthest untouched index.
        /// Note that indices before _Current may be made null, but _Current remains unchanged.
        /// </summary>
        public int Current { get { return _Current; } }
        /// <summary>
        /// Always set to the number of non-null elements in the collection.
        /// This is accurate even when _Current is ahead of null'd elements.
        /// </summary>
        public int Count { get { return _Count; } }
        public event OnCollectionChanged Changed;
        public event OnCollectionCleared Cleared;

        public BetterCollection()
        {

        }

        public BetterCollection(int capacity)
        {
            _Capacity = capacity;
            _Collection = new T[_Capacity];
        }

        protected virtual void OnChanged(BetterCollection<T> self, T item, bool removed, bool stackUpdate)
        {
            if (Changed != null)
                Changed(self, item, removed, stackUpdate);
        }

        protected virtual void OnCleared(BetterCollection<T> self)
        {
            if (Cleared != null)
                Cleared(self);
        }

        public T this[int index]
        {
            get
            {
                if (index > _Capacity)
                {
                    Debug.LogError("ERROR (read out of bounds)");
                    return default(T);
                }
                return _Collection[index];
            }
            set
            {
                if (index > _Capacity)
                {
                    Debug.LogError("ERROR (write out of bounds)");
                    return;
                }
                if (_Current < index + 1)
                    _Current = index + 1;
                if (value == null)
                {
                    T temp = _Collection[index];
                    _Collection[index] = value;
                    if (temp != null)
                        _Count--;
                    OnChanged(this, temp, true, false);
                }
                else
                {
                    if (_Collection[index] == null)
                        _Count++;
                    _Collection[index] = value;
                    OnChanged(this, _Collection[index], false, false);
                }
            }
        }

        /// <summary>
        /// Assigns item to the first unused index.
        /// Note: indices set to null after instantiation are considered used.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool Add(T item)
        {
            if (item == null)
            {
                Debug.LogError("Error (tried adding null item to container)");
                return false;
            }
            if (_Current + 1 <= _Capacity)
            {
                // this assigns it to _Current, then increments _Current
                _Collection[_Current++] = item;
                _Count++;
                OnChanged(this, item, false, false);
                return true;
            }
            return false;
        }

        public virtual bool Remove_ByRef(T item)
        {
            Debug.Log("Removing item, _Current: " + _Current.ToString());
            for (int index = _Current - 1; index >= 0; index--)
            {
                Debug.Log("Checking index " + index.ToString());
                if (_Collection[index] == item)
                {
                    _Collection[index] = null;
                    _Count--;
                    // TO-DO:
                    // BUG:
                    // so the current issue is that _Current is only decremented when removing the last element in the collection
                    // this is desirable in the case that null elements are still considered elements
                    // but completely breaks all other use cases
                    // i'm essentially using this collection as both an array and a list...
                    // i think i need to re-evaluate my needs and split this into two classes if necessary
                    // one solution is to re-order the elements upon removal
                    // but this breaks the array functionality as the indices of elements will change with no notification to users
                    if (index == _Current - 1)
                        _Current--;
                    OnChanged(this, item, true, false);
                    return true;
                }
            }
            return false;
        }

        public virtual bool Remove_ByUID(int UID)
        {
            for (int index = _Capacity - 1; index >= 0; index--)
            {
                Debug.Log("Checking index " + index.ToString() + " Capacity: " + _Capacity.ToString());
                if (_Collection[index] == null) continue;
                if (_Collection[index].UID == UID)
                {
                    OnChanged(this, _Collection[index], true, false);
                    _Collection[index] = null;
                    _Count--;
                    if (index == _Current - 1)
                        _Current--;
                    // WARNING:
                    // POSSIBLE ERROR:
                    // i added this to kind of fix SExpUIMultiSelectContainer. i also changed index to start at _Capacity instead of _Current.
                    else if (index > _Current)
                        _Current = index;
                    return true;
                }
            }
            return false;
        }

        public virtual bool Remove_ByID(int id, int count = 1)
        {
            List<T> existingItems = FindAll_ByID(id);
            if (existingItems.Count == 0)
                return false;
            for (int index = existingItems.Count - 1; index >= 0; index--)
            {
                if (count <= 0)
                    break;
                T existingItem = existingItems[index];
                if (existingItem is IStackable)
                {
                    if ((existingItem as IStackable).StackSize <= count)
                    {
                        count -= (existingItem as IStackable).StackSize;
                        Remove_ByRef(existingItem);
                    }
                    else
                    {
                        (existingItem as IStackable).StackSize -= count;
                        count = 0;
                        OnChanged(this, existingItem, false, true);
                        break;
                    }
                }
                else
                {
                    Remove_ByRef(existingItem);
                    count -= 1;
                }
            }
            return count == 0;
        }

        public virtual void Clear()
        {
            Array.Clear(_Collection, 0, _Current);
            _Current = 0;
            _Count = 0;
            OnCleared(this);
        }

        public virtual List<T> FindAll_ByID(int id)
        {
            List<T> retCol = new List<T>();
            foreach (T item in _Collection)
            {
                if (item == null) continue;
                if (item.ID == id)
                    retCol.Add(item);
            }
            return retCol;
        }

        public virtual bool Find_ByUID(int UID, out T item)
        {
            foreach (T existingItem in _Collection)
            {
                if (existingItem == null) continue;
                if (existingItem.UID == UID)
                {
                    item = existingItem;
                    return true;
                }
            }
            item = default(T);
            return false;
        }

        public virtual bool Contains_ByRef(T item)
        {
            foreach (T existingItem in _Collection)
            {
                if (existingItem == null) continue;
                if (existingItem == item)
                    return true;
            }
            return false;
        }

        public virtual bool Contains_ByID(int id, int count = 1)
        {
            bool seen = false;
            int countSeen = 0;
            foreach (T item in _Collection)
            {
                if (item == null) continue;
                if (item.ID == id)
                {
                    seen = true;
                    if (count > 1)
                    {
                        if (item is IStackable)
                        {
                            if ((item as IStackable).StackSize >= count)
                            {
                                countSeen = count;
                                break;
                            }
                            else
                            {
                                countSeen += (item as IStackable).StackSize;
                            }
                        }
                        else
                            countSeen += 1;
                    }
                    else
                    {
                        countSeen = count;
                        break;
                    }
                }
            }
            return ((seen) && (countSeen >= count));
        }

        public virtual int Count_ByID(int id)
        {
            int countSeen = 0;
            foreach (T item in _Collection)
            {
                if (item == null) continue;
                if (item.ID == id)
                {
                    if (item is IStackable)
                        countSeen += (item as IStackable).StackSize;
                    else
                        countSeen += 1;
                }
            }
            return countSeen;
        }

        public virtual bool TransferTo(IBetterList<T> collection, int UID)
        {
            T item;
            if (!Find_ByUID(UID, out item))
                return false;
            collection.Add(item);
            Remove_ByRef(item);
            return true;
        }

        public virtual void Resize(int newSize)
        {
            int bufferSize;
            if (newSize > _Capacity)
                bufferSize = _Capacity;
            else
                bufferSize = newSize;
            T[] tempBuffer = new T[bufferSize];
            Array.Copy(_Collection, tempBuffer, bufferSize);
            _Collection = new T[newSize];
            _Capacity = newSize;
            Array.Copy(tempBuffer, _Collection, bufferSize);
        }

        public virtual bool TransferAllTo(IBetterList<T> collection)
        {
            foreach (T item in _Collection)
            {
                if (item == null) continue;
                collection.Add(item);
            }
            Clear();
            return true;
        }

        public virtual void Serialize(System.IO.BinaryWriter writer)
        {
            throw new NotSupportedException();
        }

        public virtual void Deserialize(System.IO.BinaryReader reader)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_Collection).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Collection.GetEnumerator();
        }

        /// <summary>
        /// WARNING: Allocates memory and uses LINQ
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <returns></returns>
        public List<T1> ToList<T1>() where T1 : class, IDisplayable
        {
            return (List<T1>)(from m in _Collection select (m as T1));
        }

        /// <summary>
        /// NOTE: Casts in-place, no memory allocated
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <returns></returns>
        public T1[] ToArray<T1>() where T1 : class, IDisplayable
        {
            return (T1[])((IDisplayable[])_Collection);
        }
    }
}