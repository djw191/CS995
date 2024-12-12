using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;
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