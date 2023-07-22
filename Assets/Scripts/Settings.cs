using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Settings
{
    public const byte NUM_CHUNKS = 16;
    public const byte WORLD_HEIGHT = 128;
    public const byte SEA_LEVEL = 20;
    public const byte SUNLIGHT_VALUE = 15;
    public const int ChunkSizeX = 8;
    public const int ChunkSizeY = 8;
    public const int ChunkSizeZ = 8;

    // temp? obsolete?
    public const float tileSize = 0.25f;

    // mostly for editor
    public const string TERRAIN_TILE_PATH = "Tiles";
    public const int TERRAIN_TILE_WIDTH = 32;
    public const int TERRAIN_TILE_HEIGHT = 32;
}