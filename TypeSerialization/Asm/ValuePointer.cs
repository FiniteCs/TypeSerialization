namespace TypeSerialization.Asm;

public sealed class ValuePointer
    : Pointer
{
    public ValuePointer(long offset, Value value)
        : base(offset)
    {
        Value = value;
    }

    public Value Value { get; }
}
