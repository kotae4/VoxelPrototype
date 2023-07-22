using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    public interface IStackable
    {
        int StackSize { get; set; }
        int MaxStackSize { get; }
    }
}
