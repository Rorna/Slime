using System;

public static class UnityObjectUtil
{
    public static bool IsNull(this Object o)
    {
        return Object.ReferenceEquals(o, null);
    }

    public static bool IsNotNull(this Object o)
    {
        return !Object.ReferenceEquals(o, null);
    }

    public static bool IsSameObject(this Object a, Object b)
    {
        return Object.ReferenceEquals(a, b);
    }
}