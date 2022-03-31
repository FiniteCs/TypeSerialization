namespace TypeSerialization.Asm;

public sealed class NamePointer
    : Pointer
{
    public NamePointer(long offset, Name name)
        : base(offset)
    {
        Name = name;
    }

    public Name Name { get; }
}
