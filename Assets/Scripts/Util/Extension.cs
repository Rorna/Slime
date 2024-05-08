using UnityEngine;

public static class Extension
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        return UnityUtil.GetOrAddComponent<T>(go);
    }
}