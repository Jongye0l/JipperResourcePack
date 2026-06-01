using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace JipperResourcePack.KeyViewerContents;

public class KeyCountData {
    public static KeyCountData Instance;
    public readonly int[] Count = new int[KeyViewer.FootOutIndex];
    public int TotalCount;
    private int _saveDirtyVersion;

    public static void Load() {
        if(Instance != null) return;
        Instance = new KeyCountData();
        string path = Path.Combine(Main.Instance.Path, "KeyCount.dat");
        if(File.Exists(path) && Instance.Read(path)) return;
        path += ".bak";
        if(File.Exists(path)) Instance.Read(path);
    }

    private bool Read(string path) {
        byte[] data = new byte[KeyViewer.FootOutIndex * 4 + 8];
        using(FileStream fs = new(path, FileMode.Open)) 
            if(!ReadExactly(fs, data)) return false;
        TotalCount = Unsafe.ReadUnaligned<int>(ref data[4]);
        data.AsSpan(8).CopyTo(MemoryMarshal.AsBytes(Count.AsSpan()));
        return true;
    }

    private static bool ReadExactly(FileStream fs, byte[] buffer) {
        int offset = 0;
        while(offset < buffer.Length) 
            offset += fs.Read(buffer, offset, buffer.Length - offset);
        return true;
    }

    public void Save() {
        if(Interlocked.Increment(ref _saveDirtyVersion) == 1) Task.Run(SaveData);
    }

    private async void SaveData() {
        try {
            string path = Path.Combine(Main.Instance.Path, "KeyCount.dat");
            do {
                _saveDirtyVersion = 1;
                if(File.Exists(path)) {
                    File.Delete(path + ".bak");
                    File.Move(path, path + ".bak");
                }
                byte[] data = new byte[KeyViewer.FootOutIndex * 4 + 8];
                Unsafe.WriteUnaligned(ref data[4], TotalCount);
                MemoryMarshal.AsBytes(Count.AsSpan()).CopyTo(data.AsSpan(8));
                await using FileStream fs = new(path, FileMode.CreateNew);
                fs.Write(data, 0, data.Length);
                await Task.Delay(1000);
            } while(Interlocked.CompareExchange(ref _saveDirtyVersion, 0, 1) != 1);
        } catch (Exception e) {
            Main.Instance.LogException("Fail to save key count", e);
            _saveDirtyVersion = 0;
        }
    }
}