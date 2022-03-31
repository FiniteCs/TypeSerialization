namespace TypeSerialization.Asm;

public sealed class Name
{
    public Name(string name)
    {
        Text = name;
    }

    public static implicit operator Name(string s)
    {
        return new Name(s);
    }

    public int Length => Text.Length;
    public string Text { get; }
}
