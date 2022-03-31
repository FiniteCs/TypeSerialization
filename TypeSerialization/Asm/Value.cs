namespace TypeSerialization.Asm;

public class Value
{
    public Value(ValueCategory category, object value)
    {
        Category = category;
        ValueObj = value;
    }

    public virtual ValueCategory Category { get; }
    public virtual object ValueObj { get; }
}
