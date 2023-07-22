using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum Direction
{
    North = 1,
    South,
    East,
    West,
    Up,
    Down
};

public struct Block
{
    public byte BlockTypeID;
    public bool IsBlockingColumnSunlight;
    public byte SunlightValue;
    public byte VisibilityBitField;
    // EXTRA DATA USAGES:
    // === FLUID BLOCKS ===
    // bits 0-3 -> depth
    // bits 4-7 -> flow
    public byte ExtraData;

    public bool IsSolid(Direction faceDirection)
    {
        return BlockTypes.GetBlockType(BlockTypeID).IsSolid(faceDirection);
    }

    public void SetVisible(byte side, bool isVisible)
    {
        if (isVisible)
            VisibilityBitField |= (byte)(1 << side);
        else
            VisibilityBitField = (byte)(VisibilityBitField & ~(1 << side));
    }

    public bool IsVisible(byte side)
    {
        return (VisibilityBitField & (1 << side)) != 0;
    }

    public bool Equals(ref Block other)
    {
        if (SunlightValue != other.SunlightValue) return false;
        return BlockTypes.GetBlockType(BlockTypeID).Equals(BlockTypes.GetBlockType(other.BlockTypeID));
    }
}