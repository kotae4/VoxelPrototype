using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class ResourceManager
{
    public static Dictionary<string, UnityEngine.Object> LoadedResources = new Dictionary<string, UnityEngine.Object>();
    public static Dictionary<string, int> LoadedResourceUsers = new Dictionary<string, int>();

    public static UnityEngine.Object LoadResource(string pathToResource)
    {
        if (string.IsNullOrEmpty(pathToResource))
            return null;
        UnityEngine.Object cachedResource = null;
        if (LoadedResources.TryGetValue(pathToResource, out cachedResource))
        {
            if (LoadedResourceUsers.ContainsKey(pathToResource))
            {
                LoadedResourceUsers[pathToResource]++;
            }
            else
            {
                // should never happen
                LoadedResourceUsers.Add(pathToResource, 1);
            }
        }
        else
        {
            cachedResource = Resources.Load(pathToResource);
            LoadedResources.Add(pathToResource, cachedResource);
            LoadedResourceUsers.Add(pathToResource, 1);
        }
        return cachedResource;
    }

    public static T LoadResource<T>(string pathToResource) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(pathToResource))
            return default(T);
        UnityEngine.Object cachedResource = null;
        if (LoadedResources.TryGetValue(pathToResource, out cachedResource))
        {
            //Debug.LogError("LoadResource<T> already has " + pathToResource);
            if (LoadedResourceUsers.ContainsKey(pathToResource))
            {
                LoadedResourceUsers[pathToResource]++;
            }
            else
            {
                // should never happen
                LoadedResourceUsers.Add(pathToResource, 1);
            }
        }
        else
        {
            cachedResource = Resources.Load<T>(pathToResource);
            //Debug.LogError("LoadResource<T> loaded: " + pathToResource + (cachedResource == null ? "(null)" : cachedResource.name));
            LoadedResources.Add(pathToResource, cachedResource);
            LoadedResourceUsers.Add(pathToResource, 1);
        }
        return cachedResource as T;
    }

    public static void UnloadResource(string pathToResource)
    {
        return;
        /*
        if (string.IsNullOrEmpty(pathToResource))
            return;
        UnityEngine.Object loadedResource = null;
        if (LoadedResources.TryGetValue(pathToResource, out loadedResource))
        {
            LoadedResourceUsers[pathToResource] -= 1;
            if (LoadedResourceUsers[pathToResource] <= 0)
            {
                UnityEngine.Object.Destroy(loadedResource);
                // TO-DO:
                // DON'T DO THIS.
                if (!(loadedResource is GameObject))
                    Resources.UnloadAsset(loadedResource);
                LoadedResourceUsers.Remove(pathToResource);
                LoadedResources.Remove(pathToResource);
                loadedResource = null;
            }
        }
        Resources.UnloadUnusedAssets();
        */
    }

    public static void UnloadAllResources()
    {
        foreach (KeyValuePair<string, UnityEngine.Object> pair in LoadedResources)
        {
            UnityEngine.Object.Destroy(pair.Value);
            Resources.UnloadAsset(pair.Value);
        }
        LoadedResources.Clear();
        LoadedResourceUsers.Clear();
        Resources.UnloadUnusedAssets();
    }
}