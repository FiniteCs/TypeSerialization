namespace TypeSerialization;
using System;
using System.IO;
using TypeSerialization.Asm;

public static class Program
{
    public static void Main()
    {
        var type = new Test();
        var tAsm = new TypeAssembler();
        var asm = tAsm.Assemble(type);
        File.WriteAllBytes("Test.dat", asm.Buffer);
        var disassembler = new TypeDisassembler(File.ReadAllBytes("Test.dat"));
        var reference = disassembler.Disassemble();
        PrintReference(reference);
        Console.WriteLine();
    }

    public static void PrintReference(ClassReference reference)
    {
        Console.WriteLine($"Class Name: {reference.Name.Text}");
        Console.WriteLine($"Amount of Fields: {reference.Fields.Count}");
        Console.WriteLine();
        foreach (var field in reference.Fields)
        {
            Console.WriteLine($"Field Name: {field.Name.Text}");
            Console.WriteLine($"Field Value Offset: {field.Ptr.Offset}");
            Console.WriteLine($"Field Value Category: {field.Ptr.Value.Category}");
            Console.WriteLine($"Field Value: {field.Ptr.Value.ValueObj}");
            Console.WriteLine();
        }
    }
}

public sealed class Test
{
    public int A = 10;
    public int B = 20;
    public int C = 30;
    public Test2 Test2 = new();
}

public sealed class Test2
{
    public int A = 40;
    public int B = 50;
    public int C = 60;
}
