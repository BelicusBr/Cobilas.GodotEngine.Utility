using Godot;
using System;
using System.Reflection;
using Cobilas.Collections;
using System.Collections.Generic;
using Cobilas.GodotEngine.Utility.Runtime;

using IOPath = System.IO.Path;
using IOFile = System.IO.File;
using IODirectory = System.IO.Directory;
using SYSEnvironment = System.Environment;

namespace Cobilas.GodotEngine.Utility.EditorSerialization;
/// <summary>Class allows to build a serialization list of properties of a node class.</summary>
public static class BuildSerialization {
    private static bool _copyToPlayer = false;
    private const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly List<SerializedNode> serializeds = [];
    private static readonly Dictionary<Type, PropertyCustom> propertyCustom = [];
    /// <summary>The method constructs a serialization list of properties for the node.</summary>
    /// <param name="obj">The <see cref="Godot.Object"/> to use.</param>
    /// <returns>Returns a serialization representation of the node.</returns>
    public static SerializedNode? Build(Godot.Object obj)
        => obj switch {
            Node nd => Build(nd),
            Resource rc => Build(rc),
            _ => null
        };


    private static SerializedNode? Build(Resource node) {
        string id = GetID(node);
        CopyToPlayer();
        if (TryGetSerializedObject(id, out SerializedNode? result)) return result;
        else serializeds.Add(result = new(id));
        result.Add(Build(node, node.GetType(), SONull.Null, id));
        return result;
    }

    private static SerializedNode? Build(Node node) {
        string id = GetID(node);
        CopyToPlayer();
        if (TryGetSerializedObject(id, out SerializedNode? result)) return result;
        else serializeds.Add(result = new(id));
        result.Add(Build(node, node.GetType(), SONull.Null, id));
        return result;
    }

    private static List<SerializedObject> Build(object obj, Type type, SerializedObject root, string id) {
        List<SerializedObject> result = [];
        foreach (MemberInfo? item in GetMembers(type))
            if (IsSerialized(item)) {
                if (PrimitiveTypeCustom.IsPrimitiveType(GetMemberType(item))) {
                    SerializedProperty property = new(item.Name, root, id) {
                        Member = new MemberItem() {
                            Parent = obj,
                            Menber = item
                        },
                        Custom = new PrimitiveTypeCustom()
                    };
                    result.Add(property);
                } else if (IsPropertyCustom(GetMemberType(item))) {
                    SerializedProperty property = new(item.Name, root, id) {
                        Member = new MemberItem() {
                            Parent = obj,
                            Menber = item
                        },
                        Custom = propertyCustom[GetMemberType(item)]
                    };
                    result.Add(property);
                } else {
                    NoSerializedProperty property = new(item.Name, root, id) {
                        Member = new MemberItem() {
                            Parent = obj,
                            Menber = item
                        }
                    };
                    result.Add(property);
                    KeyValuePair<object, Type> pair = GetValue(obj, item);
                    property.Add(Build(pair.Key, pair.Value, property, id));
                }
            }
        return result;
    }
    /// <summary>Checks if the type has a <seealso cref="PropertyCustom"/>.</summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Returns <c>true</c> when the type has a <seealso cref="PropertyCustom"/>.</returns>
    public static bool IsPropertyCustom(Type type) {
        if (propertyCustom.ContainsKey(type)) return true;
        Type[] types = TypeUtilitarian.GetTypes();
        foreach (Type item in types) {
            PropertyCustomAttribute attribute = item.GetAttribute<PropertyCustomAttribute>();
            if (attribute is null) continue;
            else if (!attribute.UseForChildren && attribute.Target != type) continue;
            else if (attribute.UseForChildren && !type.CompareTypeAndSubType(attribute.Target)) continue;
            propertyCustom.Add(type, item.Activator<PropertyCustom>());
            return true;
        }
        return false;
    }

    private static string GetID(Node node) => node.GetPathTo(node).StringHash();

    private static string GetID(Resource resource) => resource.ResourcePath.StringHash();

    private static void CopyToPlayer() {
        if (_copyToPlayer && GDFeature.HasEditor) return;
        _copyToPlayer = true;
        string dir = IOPath.Combine(SYSEnvironment.CurrentDirectory, "cache");
        string dir2 = IOPath.Combine(dir, "player");
        string dir3 = IOPath.Combine(dir, "editor");

        string[] files = IODirectory.GetFiles(dir3);

        if (!IODirectory.Exists(dir2))
            IODirectory.CreateDirectory(dir2);

        foreach (string item in files) {
            string fileName = IOPath.GetFileName(item);
            IOFile.Copy(item, IOPath.Combine(dir2, fileName), true);
        }
    }

    private static KeyValuePair<object, Type> GetValue(object parent, MemberInfo member)
        => member switch {
            FieldInfo fdi => new(fdi.GetValue(parent), fdi.FieldType),
            PropertyInfo pti => new(pti.GetValue(parent), pti.PropertyType),
            _ => new()
        };

    private static MemberInfo[] GetMembers(Type? type) {
        if (type is null) return Array.Empty<MemberInfo>();
        return ArrayManipulation.Add(type.GetMembers(flags), GetMembers(type.BaseType));
    }

    private static bool TryGetSerializedObject(string id, out SerializedNode? result) {
        for (int I = 0; I < serializeds.Count; I++)
            if (id == serializeds[I].Id) {
                result = serializeds[I];
                return true;
            }
        result = null;
        return false;
    }

    private static Type GetMemberType(MemberInfo member) 
        => member switch {
            FieldInfo fdi => fdi.FieldType,
            PropertyInfo pti => pti.PropertyType,
            _ => NullObject.Null.GetType()
        };

    private static bool IsSerialized(MemberInfo member) 
        => member switch {
                FieldInfo fdi => IsSerialized(fdi.FieldType) && (fdi.GetCustomAttribute<ShowPropertyAttribute>() is not null || fdi.GetCustomAttribute<HidePropertyAttribute>() is not null),
                PropertyInfo pti => IsSerialized(pti.PropertyType) && (pti.GetCustomAttribute<ShowPropertyAttribute>() is not null || pti.GetCustomAttribute<HidePropertyAttribute>() is not null),
                _ => false,
            };

    private static bool IsSerialized(Type type) 
        => type.CompareType(typeof(string), typeof(bool), typeof(sbyte), typeof(byte), typeof(ushort), typeof(short),
            typeof(uint), typeof(int), typeof(ulong), typeof(long), typeof(float), typeof(double)) || 
            type.CompareTypeAndSubType<Enum>() || type.GetAttribute<SerializableAttribute>() is not null;
}
