using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JipperResourcePack.KeyViewerContents;

public class KeyCountData {
    public static KeyCountData Instance;
    public int[] Count = new int[KeyViewer.FootOutIndex];
    public int TotalCount;

    public static void Load() {
        if(Instance != null) return;
        Instance = new KeyCountData();
        string path = Path.Combine(Main.Instance.Path, "KeyCount.dat");
        if(File.Exists(path) && Instance.Read(path)) return;
        path += ".bak";
        if(File.Exists(path)) Instance.Read(path);
    }

    private unsafe bool Read(string path) {
        FileStream fs = new(path, FileMode.Open);
        Span<byte> data = stackalloc byte[KeyViewer.FootOutIndex * 4 + 8];
        if(!ReadExactly(fs, data)) return false;
        TotalCount = Unsafe.ReadUnaligned<int>(ref data[4]);
        data[8..].CopyTo(MemoryMarshal.AsBytes(Count.AsSpan()));
        return true;
    }

    private static bool ReadExactly(FileStream fs, Span<byte> buffer) {
        while(!buffer.IsEmpty) {
            int read = fs.Read(buffer);
            if(read == 0) return false;
            buffer = buffer[read..];
        }
        return true;
    }

    public void Save() {
        string path = Path.Combine(Main.Instance.Path, "KeyCount.dat");
        if(File.Exists(path)) {
            File.Delete(path + ".bak");
            File.Move(path, path + ".bak");
        }
        Span<byte> data = stackalloc byte[KeyViewer.FootOutIndex * 4 + 8];
        Unsafe.WriteUnaligned(ref data[4], TotalCount);
        MemoryMarshal.AsBytes(Count.AsSpan()).CopyTo(data[8..]);
        using FileStream fs = new(path, FileMode.CreateNew);
        fs.Write(data);
    }
}