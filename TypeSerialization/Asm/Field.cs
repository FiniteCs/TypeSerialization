namespace TypeSerialization.Asm;

public sealed class Field
{
    public Field(Name name, ValuePointer ptr)
    {
        Name = name;
        Ptr = ptr;
    }

    public Name Name { get; }
    public ValuePointer Ptr { get; set; }
}
