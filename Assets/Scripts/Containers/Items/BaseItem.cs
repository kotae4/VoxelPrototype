using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
using Game.Equipment;

namespace Game
{
    public class BaseItem : BaseResource, IEquippable
    {
        protected int _ID;
        protected int _UID;
        protected string _PrefabPath = "";
        protected GameObject _WorldPrefab;
        protected string _IconPath = "";
        protected Sprite _UIIcon;
        protected string _Description = "";
        protected EquipSlot _Slot = EquipSlot.None;
        int _StackSize = 1;
        int _MaxStackSize = 1;
        protected int _ContainerIndex = -1;


        public int ID { get { return _ID; } private set { _ID = value; } }
        public int UID { get { return _UID; } }
        public GameObject WorldPrefab { get { if (_WorldPrefab == null) _WorldPrefab = (GameObject)ResourceManager.LoadResource<GameObject>(_PrefabPath); return _WorldPrefab; } }
        public Sprite UIIcon { get { if (_UIIcon == null) _UIIcon = (Sprite)ResourceManager.LoadResource<Sprite>(_IconPath); return _UIIcon; } }
        public string Name { get { return FriendlyName; } }
        public string Description { get { return _Description; } }
        public EquipSlot Slot { get { return _Slot; } }
        public int StackSize { get { return _StackSize; } set { _StackSize = value; } }
        public int MaxStackSize { get { return _MaxStackSize; } set { _MaxStackSize = value; } }
        public int ContainerIndex { get { return _ContainerIndex; } set { _ContainerIndex = value; } }
        public object Data { get { return this; } }

        public BaseItem()
        {
            _StackSize = 1;
            _MaxStackSize = 1;
            _ContainerIndex = -1;
            _Description = "";
            _UID = (int)Time.time + UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            if (_WorldPrefab == null)
            {
                _WorldPrefab = (GameObject)ResourceManager.LoadResource<GameObject>(_PrefabPath);
            }
            if (UIIcon == null)
            {
                _UIIcon = (Sprite)ResourceManager.LoadResource<Sprite>(_IconPath);
            }
        }

        protected BaseItem(BaseItem old, bool generateUID)
        {
            //this._PrefabID = old._PrefabID;
            this._PrefabPath = old._PrefabPath;
            this._WorldPrefab = old._WorldPrefab;
            if (this._WorldPrefab == null)
            {
                _WorldPrefab = (GameObject)ResourceManager.LoadResource<GameObject>(_PrefabPath);
            }
            this._IconPath = old._IconPath;
            this._UIIcon = old._UIIcon;
            if (this.UIIcon == null)
            {
                _UIIcon = (Sprite)ResourceManager.LoadResource<Sprite>(_IconPath);
            }
            this._ContainerIndex = old._ContainerIndex;

            this._ID = old._ID;
            if (generateUID)
                this._UID = (int)Time.time + UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            else
                this._UID = old._UID;
            this.FriendlyName = old.FriendlyName;
            this._Description = old._Description;
            this._Slot = old._Slot;
            this._StackSize = old._StackSize;
            this._MaxStackSize = old._MaxStackSize;
        }

        public virtual IEquippable GetCopy(bool generateUID = false)
        {
            return new BaseItem(this, generateUID);
        }

        /// <summary>
        /// When this object is on the player's quickbar and selected and the player right-clicks in the world (eg; to place a block or interact with a chest).
        /// If right-clicking with this item should do something differently place that logic here (eg; a potion heals the player or something)
        /// </summary>
        public virtual void OnRightClickWorld()
        {
            Debug.LogWarning("Empty OnRightClickWorld handler, probable malformed item");
        }



        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(_ID);

            //writer.Write(_PrefabID);
            writer.Write(_PrefabPath);
            writer.Write(_IconPath);
            writer.Write(_ContainerIndex);


            writer.Write(_UID);
            writer.Write(FriendlyName);
            writer.Write(_Description);
            writer.Write((byte)_Slot);
            writer.Write(_StackSize);
            writer.Write(_MaxStackSize);
        }

        public virtual void Deserialize(BinaryReader reader)
        {
            _ID = reader.ReadInt32();

            /*_PrefabID = reader.ReadInt32();
            if (GameManager.Instance.ItemManager.GetPrefab(_PrefabID) == null)
            {
                Debug.LogError("Tried initializing malformed item (Prefab ID out of range)");
                return;
            }
            */
            _PrefabPath = reader.ReadString();
            _IconPath = reader.ReadString();
            _UIIcon = (Sprite)ResourceManager.LoadResource<Sprite>(_IconPath);
            _ContainerIndex = reader.ReadInt32();


            _UID = reader.ReadInt32();
            if (_UID == -1)
                _UID = (int)Time.time + UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            FriendlyName = reader.ReadString();
            _Description = reader.ReadString();
            _Slot = (EquipSlot)reader.ReadByte();
            _StackSize = reader.ReadInt32();
            _MaxStackSize = reader.ReadInt32();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }
                if (_WorldPrefab != null)
                {
                    UnityEngine.Object.Destroy(_WorldPrefab);
                    _WorldPrefab = null;
                }
                if (_UIIcon != null)
                {
                    UnityEngine.Object.Destroy(_UIIcon);
                    _UIIcon = null;
                }
                ResourceManager.UnloadResource(_PrefabPath);
                ResourceManager.UnloadResource(_IconPath);
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BaseItem() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}