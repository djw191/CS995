using System;
using System.Reflection;
using Object = System.Object;

public static class Attrs
{
    public static bool HasAttr<TAttribute>(Object obj) where TAttribute : Attribute
    {
        return obj?.GetType().GetCustomAttribute<TAttribute>() != null;
    }
}
public class NotPathable : Attribute
{
}