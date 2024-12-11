using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

public static class Attrs
{
    public static bool HasAttr<TAttribute>(Object obj) where TAttribute : Attribute
    {
        return obj?.GetType().GetCustomAttribute<TAttribute>() != null;
    }
    public static void ProcessAssembly()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            foreach (Type type in assembly.GetTypes())
            {
                foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    var serializeFieldAttribute = fieldInfo.GetCustomAttribute<Save>();
                    if (serializeFieldAttribute != null)
                    {
                        // Serialize the field
                        var instance = Activator.CreateInstance(type);
                        var fieldValue = fieldInfo.GetValue(instance);
                        var serializedData = JsonConvert.SerializeObject(fieldValue);

                        // Write the serialized data to a file
                        File.WriteAllText("serialized_data.json", serializedData);

                        // // Deserialize the data
                        // var deserializedValue = JsonConvert.DeserializeObject(serializedData, fieldValue.GetType());
                        // fieldInfo.SetValue(instance, deserializedValue);
                    }
                }
            }
        }
    }
}
public class NotPathable : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
public class Save : Attribute
{
}