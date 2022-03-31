namespace TypeSerialization.Asm;
using System.Collections.Generic;

public static class StackExtensions
{
    public static void PushRange<T>(this Stack<T> stack, Stack<T> value)
    {
        foreach (var item in value)
        {
            stack.Push(item);
        }
    }
}
