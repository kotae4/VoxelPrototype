using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Block_Glass : IBlockType
{
    public bool IsAir { get { return false; } }
    public bool IsFluid { get { return false; } }
    public bool IsTransparent { get { return true; } }
    public byte LightSourceIntensity { get { return 0; } }
    public byte BlockTypeID { get { return (byte)BlockID.Glass; } }

    public bool IsSolid(Direction blockFace)
    {
        return true;
    }

    public bool Equals(IBlockType other)
    {
        if (other == null) return false;
        if (other == this) return true;
        if (BlockTypeID != other.BlockTypeID) return false;
        // TO-DO:
        // check lighting
        return true;
    }

    public byte GetTextureLayer(Direction blockFace)
    {
        return 11;
    }
}