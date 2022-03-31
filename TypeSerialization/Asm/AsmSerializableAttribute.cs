namespace TypeSerialization.Asm;
using System;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public sealed class AsmSerializableAttribute : Attribute
{
    public AsmSerializableAttribute()
    {
    }
}
