namespace TypeSerialization.Asm;

public abstract class Pointer
{
    protected Pointer(long offset)
    {
        Offset = offset;
    }

    public long Offset { get; }
}
