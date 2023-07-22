using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    public interface IBetterList<T> : IEnumerable<T>
    {
        int Capacity { get; }
        int Current { get; }
        T this[int index] { get; set; }
        bool Contains_ByRef(T item);
        bool Add(T item);
        /*
        bool Add(T item);
        bool Delete(int id, int count = 1);
        bool Has(int id, int count = 1);
        int HowMany(int id);
        bool Get(int UID, out T item);
        List<T> GetAll(int id);
        bool TransferTo(IBetterList<T> collection, int UID);
        bool TransferAllTo(IBetterList<T> collection);
        */
    }
}
