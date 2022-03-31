namespace TypeSerialization.Asm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public sealed class AssemblerStream
{
    private byte[] _buffer;
    private long _position;

    public AssemblerStream()
    {
        _buffer = Array.Empty<byte>();
    }

    public void Dispose()
    {
        _buffer = null;
        _position = 0;
    }

    public static AssemblerStream operator +(AssemblerStream left, AssemblerStream right)
    {
        var leftBytes = left._buffer;
        var rightBytes = right._buffer;
        return leftBytes.Concat(rightBytes).ToArray();
    }

    public static implicit operator AssemblerStream(byte[] source)
    {
        return new AssemblerStream
        {
            _buffer = source,
            _position = source.Length - 1
        };
    }

    public byte[] Buffer => _buffer;
    public bool AtEnd => _position >= _buffer.LongLength;
    public long Position
    {
        get => _position;
        set => _position = value;
    }

    public unsafe T Read<T>()
        where T : unmanaged
    {
        var b = _buffer[_position];
        _position += sizeof(T);
        var value = Unsafe.ReadUnaligned<T>(ref b);
        return value;
    }

    public unsafe T Read<T>(long offset)
        where T : unmanaged
    {
        if (offset < 0 ||
            offset > _buffer.LongLength)
        {
            throw new IndexOutOfRangeException();
        }

        _position = offset;
        var b = _buffer[offset];
        _position += sizeof(T);
        var value = Unsafe.ReadUnaligned<T>(ref b);
        return value;
    }

    public unsafe void Write<T>(T value)
        where T : unmanaged
    {
        var writePos = _position;

        if ((_position += sizeof(T)) < _buffer.Length)
        {
            Unsafe.WriteUnaligned(ref _buffer[writePos], value);
        }
        else
        {
            var diff = _position - writePos;
            var newArray = new byte[_buffer.Length + diff];
            _buffer.AsSpan().CopyTo(newArray);
            _buffer = newArray;
            Unsafe.WriteUnaligned(ref _buffer[writePos], value);
        }
    }

    public void Write(byte[] buffer)
    {
        Write(buffer, _buffer.Length);
    }

    public void Write(byte[] buffer, int offset)
    {
        if (buffer is null)
            throw new ArgumentNullException(nameof(buffer));

        _position += buffer.Length;
        var bottom = SplitArray(_buffer, 0, offset);
        var top = SplitArray(_buffer, offset, _buffer.Length);
        var newArr = bottom.Concat(buffer).Concat(top).ToArray();
        _buffer = newArr;
    }

    private static T[] SplitArray<T>(T[] source, int start, int end)
    {
        if (source is null ||
            source.Length == 0)
            return source;

        var list = new List<T>();
        for (var i = start; i < end; i++)
        {
            list.Add(source[i]);
        }

        return list.ToArray();
    }
}
