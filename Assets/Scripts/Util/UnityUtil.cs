using System.Text.RegularExpressions;
using UnityEngine;

//Util Class
public class UnityUtil
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static GameObject FindChildObject(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChildComponent<Transform>(go, name, recursive);
        if (transform.IsNull())
            return null;

        return transform.gameObject; 
    }

    public static T FindChildComponent<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go.IsNull()) 
            return null;

        if (recursive)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    //get component
                    T component = transform.GetComponent<T>();
                    if (component.IsNotNull())
                        return component; 
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }
        return null;
    }

    public static string RemoveGuid(string objName)
    {
        var nameArr = objName.Split('_');
        return nameArr[0];
    }

    public static string RemoveObjectNumber(string objName) //use Regular Expression
    {
        //Regular Expression: "_number"
        string pattern = @"_\d+";

        string refinedName = Regex.Replace(objName, pattern, "");

        return refinedName;
    }

    public static string SetPathByType(FieldObjectTypeEnum type, string objName)
    {
        string path = string.Empty;
        switch (type)
        {
            case FieldObjectTypeEnum.Player:
            case FieldObjectTypeEnum.Enemy:
            case FieldObjectTypeEnum.BossEnemy:
                {
                    path = "Person/" + objName;
                }
                break;

            case FieldObjectTypeEnum.Item:
                {
                    path = "Item/" + objName;
                }
                break;

            case FieldObjectTypeEnum.Effect:
                {
                    path = "Effect/" + objName;
                }
                break;

            default:
                return objName;
                break;
        }

        return path;
    }

    public static string GetObjectName(GameObject go)
    {
        if (go.name == DefineStrings.Body)
        {
            return go.transform.parent.name;
        }
        else
        {
            return go.name;
        }
    }
}