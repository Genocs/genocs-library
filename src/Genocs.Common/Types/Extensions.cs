using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Genocs.Common.Types;

/// <summary>
/// Type extensions.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// GetDefaultInstance.
    /// </summary>
    /// <param name="type">The type for which to get the default instance.</param>
    /// <returns>The default instance of the specified type, or null if it cannot be created.</returns>
    public static object? GetDefaultInstance(this Type type)
    {
        if (type == typeof(string))
        {
            return string.Empty;
        }

        var defaultValueCache = new Dictionary<Type, object?>();

        if (TryGetDefaultValue(type, out object? instance, defaultValueCache))
        {
            return instance;
        }

        return null;
    }

    /// <summary>
    /// It sets the default values for all properties of the given instance. It uses reflection to iterate through
    /// the properties and assigns default values based on their types.
    /// </summary>
    /// <param name="instance">The instance for which to set the default values.</param>
    /// <returns>The instance with its properties set to their default values.</returns>
    public static object SetDefaultInstanceProperties(this object instance)
        => SetDefaultInstanceProperties(instance, []);

    private static object SetDefaultInstanceProperties(object instance, Dictionary<Type, object?> defaultValueCache)
    {
        defaultValueCache ??= [];

        var type = instance.GetType();

        foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance))
        {
            if (TryGetDefaultValue(propertyInfo.PropertyType, out object? defaultValue, defaultValueCache))
            {
                SetValue(propertyInfo, instance, defaultValue);
            }
        }

        return instance;
    }

    private static bool TryGetDefaultValue(Type type, out object? defaultValue, Dictionary<Type, object?> defaultValueCache)
    {
        if (defaultValueCache.TryGetValue(type, out defaultValue))
        {
            return true;
        }

        if (type == typeof(string))
        {
            defaultValue = string.Empty;

            defaultValueCache[type] = defaultValue;

            return true;
        }

        if (type.Name == "IDictionary`2")
        {
            defaultValue = null;

            return false;
        }

        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            if (TryGetCollectionDefaultValue(type, out defaultValue))
            {
                defaultValueCache[type] = defaultValue;

                return true;
            }

            defaultValue = null;

            return false;
        }

        if (type.IsInterface || type.IsAbstract)
        {
            defaultValue = null;

            return false;
        }

        if (type.IsArray)
        {
            if (TryGetCollectionDefaultValue(type, out defaultValue))
            {
                defaultValueCache[type] = defaultValue;

                return true;
            }

            defaultValue = null;

            return false;
        }

        if (!type.IsClass)
        {
            defaultValue = null;

            return false;
        }

        // Create instance without calling constructor
        defaultValue = RuntimeHelpers.GetUninitializedObject(type);

        defaultValueCache[type] = defaultValue;

        SetDefaultInstanceProperties(defaultValue, defaultValueCache);

        return true;
    }

    private static bool TryGetCollectionDefaultValue(Type type, out object? defaultValue)
    {
        var elementType =
            type.IsGenericType
                ? type.GenericTypeArguments[0]
                : type.GetElementType();

        if (elementType is null)
        {
            defaultValue = null;

            return false;
        }

        if (typeof(IEnumerable).IsAssignableFrom(elementType))
        {
            if (elementType == typeof(string))
            {
                defaultValue = Array.Empty<string>();

                return true;
            }

            defaultValue = null;

            return false;
        }

        defaultValue = Array.CreateInstance(elementType, 0);

        return true;
    }

    private static void SetValue(PropertyInfo propertyInfo, object instance, object? value)
    {
        if (propertyInfo.CanWrite)
        {
            propertyInfo.SetValue(instance, value);
            return;
        }

        var fieldInfo = instance
                        .GetType()
                        .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                        .SingleOrDefault(x => x.Name.StartsWith($"<{propertyInfo.Name}>"));

        fieldInfo?.SetValue(instance, value);
    }
}
