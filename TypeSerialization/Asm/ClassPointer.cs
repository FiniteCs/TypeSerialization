namespace TypeSerialization.Asm;

public sealed class ClassPointer
    : Pointer
{
    public ClassPointer(long offset, ClassReference reference)
        : base(offset)
    {
        Reference = reference;
    }

    public ClassReference Reference { get; }
}
