using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Game.Items
{
    public class ItemManager
    {
        private static readonly ItemManager instance = new ItemManager();
        public static ItemManager Instance
        {
            get
            {
                return instance;
            }
        }

        bool bLoadedItems = false;
        List<BaseItem> m_loadedItems = new List<BaseItem>();

        private ItemManager()
        {

        }

        public bool LoadItems(string filePath)
        {
            m_loadedItems.Clear();
            string[] fileNames = Directory.GetFiles(filePath, "*?.json", SearchOption.TopDirectoryOnly);
            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.Auto;
            serializer.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
            serializer.Formatting = Formatting.Indented;
            foreach (string fileName in fileNames)
            {
                //Debug.Log("Loading " + fileName);
                using (StreamReader reader = File.OpenText(fileName))
                {
                    BaseItem tempParsedItem = (BaseItem)serializer.Deserialize(reader, typeof(BaseItem));
                    //Debug.Log("Deserialized " + tempParsedItem.Name);
                    if (!m_loadedItems.Contains(tempParsedItem))
                        m_loadedItems.Add(tempParsedItem);
                }
            }
            Debug.Log("Done loading items");
            return true;
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<BaseItem> GetAllItems()
        {
            return m_loadedItems.AsReadOnly();
        }

        public BaseItem GetItem(int itemID)
        {
            foreach (BaseItem item in m_loadedItems)
            {
                if (item.ID == itemID)
                {
                    return (BaseItem)item.GetCopy(true);
                }
            }
            return null;
        }
    }
}