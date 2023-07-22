using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorCreateTextureArray
{
    [MenuItem("Scripts/Create Texture Array")]
    static void CreateTextureArray()
    {

        Texture2DArray m_TextureArray = new Texture2DArray(Settings.TERRAIN_TILE_WIDTH, Settings.TERRAIN_TILE_HEIGHT, GameManager.Instance.TexturePaths.Length, TextureFormat.RGBA32, true);
        m_TextureArray.wrapMode = TextureWrapMode.Repeat;
        m_TextureArray.filterMode = FilterMode.Point;

        for (int index = 0; index < GameManager.Instance.TexturePaths.Length; index++)
        {
            Texture2D loadedTex = Resources.Load<Texture2D>(Settings.TERRAIN_TILE_PATH + "/" + GameManager.Instance.TexturePaths[index]);
            if (loadedTex == null)
            {
                Debug.LogError("ERROR: Unable to load texture slice '" + Settings.TERRAIN_TILE_PATH + "/" + GameManager.Instance.TexturePaths[index] + "'");
                return;
            }
            m_TextureArray.SetPixels32(loadedTex.GetPixels32(), index);
        }
        m_TextureArray.Apply();
        string path = "Assets/TileTextureArray.asset";
        AssetDatabase.CreateAsset(m_TextureArray, path);
        Debug.Log("Saved asset to " + path);
    }
}