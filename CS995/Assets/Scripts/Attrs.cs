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
    public static Dictionary<Guid, GameObject> Registry = new();
    public static bool HasAttr<TAttribute>(Object obj) where TAttribute : Attribute
    {
        return obj?.GetType().GetCustomAttribute<TAttribute>() != null;
    }
    public static bool Register(GameObject obj, Guid id)
    {
        if (Registry.ContainsKey(id)) return false;
        Registry.Add(id, obj);
        return true;
    }
    public static bool Register(GameObject obj)
    {
        Registerable registerable = obj.GetComponent<Registerable>();
        if (registerable == null) return false;
        return Register(obj, Guid.Parse(registerable.guid) );
    }

    public static void Save()
    {
        //TODO iterate through every script on gameobject, looking for save
        List<SerializedObject> objs = new();
        
        foreach (var entry in Registry)
        {
            Dictionary<Type, Dictionary<String, Object>> members = new();
            
            Component[] components = entry.Value.GetComponents<Component>();
            if (components.GroupBy(c => c.GetType()).Any(g => g.Count() > 1))
            {
                Debug.LogError($"GameObject {entry.Value} has more than one component of the same type, this is not allowed.");
                return;
            }
            foreach (Component component in components)
            {
                if (component is MonoBehaviour)
                {
                    MonoBehaviour script = (MonoBehaviour)component;
                    foreach (MemberInfo member in script.GetType()
                                 .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        Object value = null;
                        if (member.GetCustomAttribute<Save>() != null)
                        {
                            value = (member as FieldInfo)?.GetValue(script);
                            if(value == null) value = (member as PropertyInfo)?.GetValue(script);
                        }
                        if(value != null)
                        {
                            if (!members.ContainsKey(script.GetType()))
                            {
                                members.Add(script.GetType(), new Dictionary<String, Object>());
                            }
                            if (!members[script.GetType()].ContainsKey(member.Name))
                            {
                                members[script.GetType()].Add(member.Name, value);
                            }
                        }
                    }
                }
            }
            
            
            PrefabUtility.SaveAsPrefabAssetAndConnect(entry.Value.gameObject, $"Assets/Prefabs/Generated/{entry.Key}.prefab", InteractionMode.AutomatedAction, out _);

            var obj = new SerializedObject()
            {
                prefabLocation = $"Assets/Prefabs/{entry.Key}.prefab",
                ID = entry.Key,
                members = members
            };
            objs.Add(obj);
        }
        File.WriteAllText("save.json", JsonConvert.SerializeObject(objs));
    }

    public static void Load()
    {
        string input = File.ReadAllText("save.json");
        SerializedObject[] objs = JsonConvert.DeserializeObject<SerializedObject[]>(input, new JsonSerializerSettings
        {
            Error = delegate(object sender, ErrorEventArgs args)
            {
                Debug.LogError(args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            }
        }); //Deserialization crashes...
        foreach (var serializedObject in objs)
        {
            if (Registry.ContainsKey(serializedObject.ID))
            {
                FillObject(serializedObject);
            }
            else
            { //Not robust enough, needs sytstem of prefabs?
                Register(UnityEngine.Object.Instantiate(PrefabUtility.LoadPrefabContents($"Assets/Prefabs/{serializedObject.ID}.prefab")), serializedObject.ID);
                FillObject(serializedObject);
            }
        }
        
    }

    private static void FillObject(SerializedObject serializedObject)
    {
        //Set transform
        GameObject obj = Registry[serializedObject.ID];
                
        Component[] components = obj.GetComponents<Component>();
        if (components.GroupBy(c => c.GetType()).Any(g => g.Count() > 1))
        {
            Debug.LogError($"GameObject {obj} has more than one component of the same type, this is not allowed.");
            return;
        }

        foreach (Component component in components)
        {
            //find all saved properties and fields, and set them
            foreach (MemberInfo member in component.GetType()
                         .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                // For each found field/property:
                if (member.GetCustomAttribute<Save>() != null)
                {
                    var currentType = serializedObject.members[component.GetType()];
                    if (currentType.ContainsKey(member.Name))
                    {
                        object value = currentType[member.Name] is double
                            ? Convert.ToSingle(currentType[member.Name])
                            : currentType[member.Name];
                        if (member is FieldInfo fieldInfo)
                        {
                            fieldInfo.SetValue(component, value);
                        }
                        else if (member is PropertyInfo propertyInfo)
                        {
                            propertyInfo.SetValue(component, value);
                        }
                    }
                }
            }
        }
    }


    private class SerializedObject
    {
        public string prefabLocation;
        public Guid ID;
        public Dictionary<Type, Dictionary<String, Object>> members = new();
    }
}
public class NotPathable : Attribute
{
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class Save : Attribute
{
}