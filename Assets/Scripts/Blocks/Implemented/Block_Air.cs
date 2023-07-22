using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Block_Air : IBlockType
{
    public bool IsAir { get { return true; } }
    public bool IsFluid { get { return false; } }
    public bool IsTransparent { get { return true; } }
    public byte LightSourceIntensity { get { return 0; } }
    public byte BlockTypeID { get { return (byte)BlockID.Air; } }

    public bool IsSolid(Direction blockFace)
    {
        return false;
    }

    public bool Equals(IBlockType other)
    {
        if (other == null) return false;
        if (other == this) return true;
        if (BlockTypeID == other.BlockTypeID) return true;
        return false;
    }

    public byte GetTextureLayer(Direction blockFace)
    {
        return 0xff;
    }
}