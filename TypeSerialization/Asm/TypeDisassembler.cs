namespace TypeSerialization.Asm;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class TypeDisassembler
{
#pragma warning disable CA1822
    private readonly byte[] _input;
    private static readonly byte[] REF_STREAM_DECL = new byte[] { 230, 231, 232 };
    private static readonly byte[] END_REF_STREAM_DECL = new byte[] { 232, 231, 230 };
    private static readonly byte[] VALUE_STREAM_DECL = new byte[] { 240, 241, 242 };
    private static readonly byte[] NAME_STREAM_DECL = new byte[] { 250, 251, 252 };

    public TypeDisassembler(byte[] input)
    {
        _input = input;
    }

    public List<ClassReference> Disassemble()
    {
        var referenceStack = ParseReferenceStack();
        var classReferences = new List<ClassReference>();
        var valueStream = ParseValueStream();
        var nameStream = ParseNameStream();
        valueStream.Position = 0;
        nameStream.Position = 0;
        var offset = 0;
        do
        {
            var referenceStream = referenceStack.Pop();
            referenceStream.Position = 0;
            classReferences.Add(ParseClassReference(referenceStream, valueStream, nameStream, offset).Reference);
            offset++;
        }
        while (referenceStack.Count > 0);
        foreach (var reference in classReferences)
        {
            foreach (var field in reference.Fields)
            {
                if (field.Ptr.Value.Category == ValueCategory.ClassReference)
                {
                    var index = Convert.ToInt32(field.Ptr.Value.ValueObj);
                    field.Ptr.Value.ValueObj = classReferences[index];
                }
            }
        }

        return classReferences;
    }

    public ClassPointer ParseClassReference(AssemblerStream referenceStream,
                                            AssemblerStream valueStream,
                                            AssemblerStream nameStream,
                                            int offset = 0)
    {
        if (referenceStream.AtEnd)
        {
            return null;
        }

        var namePtr = ParseNamePointer(referenceStream, nameStream);
        var fieldCount = referenceStream.Read<int>();
        var fields = new List<Field>();
        for (var i = 0; i < fieldCount; i++)
        {
            fields.Add(ParseField(referenceStream, valueStream, nameStream));
        }

        return new ClassPointer(offset, new ClassReference(namePtr.Name, fields));
    }

    public Field ParseField(AssemblerStream referenceStream,
                            AssemblerStream valueStream,
                            AssemblerStream nameStream)
    {
        var namePtr = ParseNamePointer(referenceStream, nameStream);
        var valuePtr = ParseValuePointer(referenceStream, valueStream);
        return new Field(namePtr.Name, valuePtr);
    }

    public NamePointer ParseNamePointer(AssemblerStream referenceStream,
                                        AssemblerStream nameStream)
    {
        var namePtrOffset = referenceStream.Read<long>();
        var nameLength = nameStream.Read<int>(namePtrOffset);
        var name = new char[nameLength];
        for (long i = 0; i < nameLength; i++)
        {
            name[i] = (char)nameStream.Read<byte>();
        }

        return new NamePointer(namePtrOffset, new string(name));
    }

    public ValuePointer ParseValuePointer(AssemblerStream referenceStream,
                                          AssemblerStream valueStream)
    {
        var valuePtrOffset = referenceStream.Read<long>();
        var category = valueStream.Read<ValueCategory>(valuePtrOffset);
        object value = null;
        switch (category)
        {
            case ValueCategory.ClassReference:
                value = valueStream.Read<long>();
                break;
            case ValueCategory.Primitive:
                value = ParsePrimitiveType(valueStream);
                break;
        }

        return new ValuePointer(valuePtrOffset, new Value(category, value));
    }

    public object ParsePrimitiveType(AssemblerStream valueStream)
    {
        var type = valueStream.Read<ValueType>();
        object value = null;
        switch (type)
        {
            case ValueType.Bool:
                value = valueStream.Read<bool>();
                break;
            case ValueType.Byte:
                value = valueStream.Read<byte>();
                break;
            case ValueType.SByte:
                value = valueStream.Read<sbyte>();
                break;
            case ValueType.Char:
                value = valueStream.Read<char>();
                break;
            case ValueType.Decimal:
                value = valueStream.Read<decimal>();
                break;
            case ValueType.Double:
                value = valueStream.Read<double>();
                break;
            case ValueType.Float:
                value = valueStream.Read<float>();
                break;
            case ValueType.Int:
                value = valueStream.Read<int>();
                break;
            case ValueType.UInt:
                value = valueStream.Read<uint>();
                break;
            case ValueType.Long:
                value = valueStream.Read<long>();
                break;
            case ValueType.ULong:
                value = valueStream.Read<ulong>();
                break;
            case ValueType.Short:
                value = valueStream.Read<short>();
                break;
            case ValueType.UShort:
                value = valueStream.Read<ushort>();
                break;
        }

        return value;
    }

    public Stack<AssemblerStream> ParseReferenceStack()
    {
        var referenceStack = new Stack<AssemblerStream>();
        AssemblerStream asm = _input;
        asm.Position = 0;
        var stackCount = asm.Read<int>();
        asm.Dispose();
        for (int i = 0; i < stackCount; i++)
        {
            referenceStack.Push(ParseReferenceStream(i));
        }

        return referenceStack;
    }
    
    public AssemblerStream ParseReferenceStream(int index)
    {
        var start = PatternAt(_input, REF_STREAM_DECL).ToArray()[index] + 3;
        var end = PatternAt(_input, END_REF_STREAM_DECL).ToArray()[index];
        var byteList = new List<byte>();
        for (var i = start; i < end; i++)
        {
            byteList.Add(_input[i]);
        }

        return byteList.ToArray();
    }

    public AssemblerStream ParseValueStream()
    {
        var start = IndexOf(_input, VALUE_STREAM_DECL) + 3;
        var end = IndexOf(_input, NAME_STREAM_DECL);
        var byteList = new List<byte>();
        for (var i = start; i < end; i++)
        {
            byteList.Add(_input[i]);
        }

        return byteList.ToArray();
    }

    public AssemblerStream ParseNameStream()
    {
        var start = IndexOf(_input, NAME_STREAM_DECL) + 3;
        var end = _input.Length - 5;
        var byteList = new List<byte>();
        for (var i = start; i < end; i++)
        {
            byteList.Add(_input[i]);
        }

        return byteList.ToArray();
    }

    public static int IndexOf(byte[] source, byte[] patternToFind)
    {
        if (patternToFind.Length > source.Length)
        {
            return -1;
        }

        for (var i = 0; i < source.Length - patternToFind.Length; i++)
        {
            var found = true;
            for (var j = 0; j < patternToFind.Length; j++)
            {
                if (source[i + j] != patternToFind[j])
                {
                    found = false;
                    break;
                }
            }
            if (found)
            {
                return i;
            }
        }

        return -1;
    }

    public static IEnumerable<int> PatternAt(byte[] source, byte[] pattern)
    {
        for (int i = 0; i < source.Length; i++)
        {
            if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
            {
                yield return i;
            }
        }
    }
#pragma warning restore CA1822
}
