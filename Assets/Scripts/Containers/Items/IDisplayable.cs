using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    public interface IDisplayable : IDisposable
    {
        // unique per item
        int ID { get; }
        // unique per instance of an item
        int UID { get; }
        int ContainerIndex { get; set; }
        object Data { get; }
        UnityEngine.Sprite UIIcon { get; }
    }
}
