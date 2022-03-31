namespace TypeSerialization.Asm;

public abstract class Pointer
    : Value
{
    protected Pointer(long offset)
        : base(ValueCategory.ClassReference, offset)
    {
        Offset = offset;
    }

    public long Offset { get; }
    public sealed override ValueCategory Category => base.Category;
    public sealed override object ValueObj => base.ValueObj;
}
