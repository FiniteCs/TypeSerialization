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
        var references = disassembler.Disassemble();
        foreach (var reference in references)
        {
            PrintReference(reference);
        }
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
    [AsmSerializable]
    public int A = 10;
    
    [AsmSerializable]
    public int B = 20;
    
    [AsmSerializable]
    public int C = 30;

    [AsmSerializable]
    public Test2 Test2 = new();
}

public sealed class Test2
{
    [AsmSerializable]
    public int A = 40;

    [AsmSerializable]
    public int B = 50;

    [AsmSerializable]
    public int C = 60;
}
