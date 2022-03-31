namespace TypeSerialization.Asm;
using System;
using System.Collections.Generic;
using System.Reflection;

public static class Serializer
{
    /*
        Say we have a class that we want to serialize.
        The class has a two fields that are ints.
        The class also has one field that is a custom class.
            The custom class has two fields that are longs.

        In order to serialize class1, we need to serialize class2.
    */

    public static ClassReference SerializeReference(object value)
    {
        var type = value.GetType();
        var name = new Name(type.Name);
        var sFields = new List<Field>();
        var fields = type.GetFields();
        var properties = type.GetProperties();
        foreach (var field in fields)
        {
            var sField = SerializeField(field, value);
            sFields.Add(sField);
        }
        foreach (var p in properties)
        {
            var sField = SerializeProperty(p, value);
            sFields.Add(sField);
        }

        return new ClassReference(name, sFields);
    }

    public static Field SerializeProperty(PropertyInfo property, object value)
    {
        var propertyType = property.PropertyType;
        var propertyName = SerializeName(property.Name);
        var type = GetFieldType(propertyType);
        var propertyValue = SerializeValue(type, property, value);
        var ptr = new ValuePointer(0, propertyValue);
        return new Field(propertyName, ptr);
    }

    public static Field SerializeField(FieldInfo field, object value)
    {
        var fieldType = field.FieldType;
        var fieldName = SerializeName(field.Name);
        var type = GetFieldType(fieldType);
        var fieldValue = SerializeValue(type, field, value);
        var ptr = new ValuePointer(0, fieldValue);
        return new Field(fieldName, ptr);
    }

    public static Value SerializeValue(ValueCategory sType, object field, object origin)
    {
        object v = null;
        if (field is FieldInfo fi)
        {
            v = fi.GetValue(origin);
        }
        else if (field is PropertyInfo pi)
        {
            v = pi.GetValue(origin);
        }

        switch (sType)
        {
            case ValueCategory.Primitive:
            case ValueCategory.ClassReference:
                return new Value(sType, v);
            default:
                return new Value(sType, new byte[sizeof(long)]);
        }
    }

    public static ValueCategory GetFieldType(Type type)
    {
        if (type.IsPrimitive)
        {
            return ValueCategory.Primitive;
        }
        else if (type.IsClass)
        {
            return ValueCategory.ClassReference;
        }
        else if (type.IsArray)
        {
            return ValueCategory.Array;
        }
        else
        {
            return ValueCategory.Null;
        }
    }

    public static Name SerializeName(string name)
    {
        return new Name(name);
    }
}
