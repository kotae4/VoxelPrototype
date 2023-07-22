using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum BlockID : byte
{
    Dirt = 1,
    Grass,
    Stone,
    Diamond,
    Wood,
    Glass,
    Debug,
    Water,
    Air
};

public static class BlockTypes
{
    private static IBlockType[] types = new IBlockType[] { new Block_Air(), new Block_Dirt(), new Block_Grass(), new Block_Stone(), new Block_Diamond(), new Block_Wood(), new Block_Glass(), new Block_Debug(), new Block_Water(), new Block_Air() };

    public static IBlockType GetBlockType(byte blockTypeID)
    {
        return types[blockTypeID];
    }
}