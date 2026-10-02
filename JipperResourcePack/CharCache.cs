using System;
using System.Text;

namespace JipperResourcePack;

public struct CharCache {
    public char[] Buffer;
    public int Length;

    public void Store(char[] source, int length) {
        EnsureCapacity(length);
        Array.Copy(source, 0, Buffer, 0, length);
        Length = length;
    }

    public void Store(StringBuilder sb) {
        int length = sb.Length;
        EnsureCapacity(length);
        sb.CopyTo(0, Buffer, 0, length);
        Length = length;
    }

    public readonly int WriteTo(char[] destination, int index) {
        if(Length == 0) return index;
        Array.Copy(Buffer, 0, destination, index, Length);
        return index + Length;
    }

    private void EnsureCapacity(int length) {
        if(Buffer != null && Buffer.Length >= length) return;
        int capacity = 64;
        while(capacity < length) capacity <<= 1;
        Buffer = new char[capacity];
    }
}
