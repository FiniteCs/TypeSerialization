namespace TypeSerialization.Asm;
using System.Collections.Generic;
using System.Text;

/*
 * -Field:
 *      Field Name
 *      Value : Pointer to value if value is a reference to a class
 * 
 * -Class Reference:
 *      Class Name
 *      Fields
 * 
 * [Class References]
 * [Values]
 */
public sealed class TypeAssembler
{
    private readonly Stack<AssemblerStream> _referenceStack;
    private readonly AssemblerStream _nameStream;
    private readonly AssemblerStream _valueStream;
    private static readonly byte[] REF_STREAM_DECL = new byte[] { 230, 231, 232 };
    private static readonly byte[] END_REF_STREAM_DECL = new byte[] { 232, 231, 230 };
    private static readonly byte[] VALUE_STREAM_DECL = new byte[] { 240, 241, 242 };
    private static readonly byte[] NAME_STREAM_DECL = new byte[] { 250, 251, 252 };
    private static readonly byte[] EOF = new byte[] { 0, 255, 0, 255, 0 };

    public TypeAssembler()
    {
        _referenceStack = new Stack<AssemblerStream>();
        _nameStream = new AssemblerStream();
        _valueStream = new AssemblerStream();
    }

    public AssemblerStream Assemble(object obj)
    {
        var asm = new AssemblerStream();
        var reference = Serializer.SerializeReference(obj);
        AssembleClassReference(reference);
        asm.Write(_referenceStack.Count);
        do
        {
            asm += REF_STREAM_DECL;
            asm += _referenceStack.Pop();
            asm += END_REF_STREAM_DECL;
        }
        while (_referenceStack.Count > 0);
        asm += VALUE_STREAM_DECL;
        asm += _valueStream;
        asm += NAME_STREAM_DECL;
        asm += _nameStream;
        asm += EOF;
        return asm;
    }

    public ClassPointer AssembleClassReference(ClassReference reference)
    {
        var referenceStream = new AssemblerStream();
        _referenceStack.Push(referenceStream);
        var offset = _referenceStack.Count - 1;
        var namePtr = AssembleName(reference.Name);
        referenceStream.Write(namePtr.Offset);
        referenceStream.Write(reference.Fields.Count);
        foreach (var field in reference.Fields)
        {
            AssembleField(field, referenceStream);
        }

        return new ClassPointer(offset, reference);
    }

    public void AssembleField(Field field, AssemblerStream referenceStream)
    {
        var namePtr = AssembleName(field.Name);
        referenceStream.Write(namePtr.Offset);
        var valuePtr = AssembleValue(field.Ptr.Value);
        referenceStream.Write(valuePtr.Offset);
    }

    public NamePointer AssembleName(Name name)
    {
        var nameOffset = _nameStream.Position;
        _nameStream.Write(name.Length);
        _nameStream.Write(Encoding.ASCII.GetBytes(name.Text));
        return new NamePointer(nameOffset, name);
    }

    public ValuePointer AssembleValue(Value value)
    {
        var valueOffset = _valueStream.Position;
        _valueStream.Write(value.Category);
        AssembleValue(value.Category, value.ValueObj);
        return new ValuePointer(valueOffset, value);
    }

    public void AssembleValue(ValueCategory category, object value)
    {
        switch (category)
        {
            case ValueCategory.Primitive:
                AssemblePrimitiveValue(value);
                break;
            case ValueCategory.ClassReference:
                AssembleReferenceValue(value);
                break;
        }
    }

    public void AssembleReferenceValue(object value)
    {
        var reference = Serializer.SerializeReference(value);
        var referencePtr = AssembleClassReference(reference);
        _valueStream.Write(referencePtr.Offset);
    }

    public void AssemblePrimitiveValue(object value)
    {
        switch (value)
        {
            case bool b:
                _valueStream.Write(ValueType.Bool);
                _valueStream.Write(b);
                break;
            case byte b:
                _valueStream.Write(ValueType.Byte);
                _valueStream.Write(b);
                break;
            case sbyte b:
                _valueStream.Write(ValueType.SByte);
                _valueStream.Write(b);
                break;
            case char c:
                _valueStream.Write(ValueType.Char);
                _valueStream.Write(c);
                break;
            case decimal d:
                _valueStream.Write(ValueType.Decimal);
                _valueStream.Write(d);
                break;
            case double doub:
                _valueStream.Write(ValueType.Double);
                _valueStream.Write(doub);
                break;
            case float f:
                _valueStream.Write(ValueType.Float);
                _valueStream.Write(f);
                break;
            case int i:
                _valueStream.Write(ValueType.Int);
                _valueStream.Write(i);
                break;
            case uint ui:
                _valueStream.Write(ValueType.UInt);
                _valueStream.Write(ui);
                break;
            case long l:
                _valueStream.Write(ValueType.Long);
                _valueStream.Write(l);
                break;
            case ulong ul:
                _valueStream.Write(ValueType.ULong);
                _valueStream.Write(ul);
                break;
            case short s:
                _valueStream.Write(ValueType.Short);
                _valueStream.Write(s);
                break;
            case ushort us:
                _valueStream.Write(ValueType.UShort);
                _valueStream.Write(us);
                break;
        }
    }
}
