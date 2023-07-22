﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Block_Grass : IBlockType
{
    public bool IsAir { get { return false; } }
    public bool IsFluid { get { return false; } }
    public bool IsTransparent { get { return false; } }
    public byte LightSourceIntensity { get { return 0; } }
    public byte BlockTypeID { get { return (byte)BlockID.Grass; } }

    public bool IsSolid(Direction blockFace)
    {
        return true;
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
        switch (blockFace)
        {
            case Direction.Down:
                {
                    return 8;
                }
            case Direction.Up:
                {
                    return 10;
                }
            case Direction.North:
            case Direction.East:
            case Direction.South:
            case Direction.West:
                {
                    return 9;
                }
            default:
                {
                    return 0xff;
                }
        }
    }
}