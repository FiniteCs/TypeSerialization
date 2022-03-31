namespace TypeSerialization.Asm;
using System.Collections.Generic;

public sealed class ClassReference
{
    public ClassReference(Name name, List<Field> fields)
    {
        Name = name;
        Fields = fields;
    }

    public Name Name { get; }
    public List<Field> Fields { get; }
    public int Size { get; }
}
