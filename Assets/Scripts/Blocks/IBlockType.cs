using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public interface IBlockType
{
    bool IsSolid(Direction blockFace);
    bool IsAir { get; }
    bool IsFluid { get; }
    bool IsTransparent { get; }
    byte LightSourceIntensity { get; }
    byte BlockTypeID { get; }

    bool Equals(IBlockType other);
    byte GetTextureLayer(Direction blockFace);
}
