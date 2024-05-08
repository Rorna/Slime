using UnityEngine;

public static class ResourceUtil
{
    public static GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Resources.Load<GameObject>($"Prefabs/{path}");
        if (original.IsNull())
        {
            Debug.LogError($"Failed to Load prefab : {path}");
            return null;
        } 

        GameObject go = Object.Instantiate(original, parent);

        return go;
    }

    public static GameObject Instantiate(string path, Vector2 position, Quaternion direction, Transform parent = null) //pos, dir
    {
        GameObject original = Resources.Load<GameObject>($"Prefabs/{path}");
        if (original.IsNull())
        {
            Debug.LogError($"Failed to Load prefab : {path}");
            return null;
        } 

        GameObject go = Object.Instantiate(original, position, direction);
        go.transform.SetParent(parent);

        return go;
    }

    public static GameObject Instantiate(string path, Vector3 position, Quaternion rotation)
    {
        GameObject original = Resources.Load<GameObject>($"Prefabs/{path}");

        if (original.IsNull())
        {
            Debug.LogError($"Failed to Load prefab : {path}");
            return null;
        }

        GameObject go = Object.Instantiate(original, position, rotation);
        return go;
    }

    public static void Destroy(GameObject go)
    {
        if (go.IsNull()) 
            return;

        Object.Destroy(go);
    }
}