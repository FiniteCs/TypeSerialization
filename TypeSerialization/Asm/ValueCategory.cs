namespace TypeSerialization.Asm;

public enum ValueCategory
    : byte
{
    Null = 0,
    Primitive = 1,
    ClassReference = 2,
    Array = 3,
}
