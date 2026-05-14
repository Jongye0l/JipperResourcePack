using System.Threading;
using JALib.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace JipperResourcePack.Async;

public class AsyncImage {
    private readonly Image _image;
    private Color _color;
    private int _colorChangeRequested;

    public AsyncImage(Image image) {
        _image = image;
        _color = image.color;
    }
    
    public Color Color {
        get => _color;
        set {
            if(EqualColor(_color, value)) return;
            _color = value;
            if(MainThread.IsMainThread()) _image.color = _color;
            else if(Interlocked.Increment(ref _colorChangeRequested) == 1) MainThread.Run(Main.Instance, ApplyColor);
        }
    }

    private static bool EqualColor(in Color a, in Color b) {
        // ReSharper disable CompareOfFloatsByEqualityOperator
        return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        // ReSharper restore CompareOfFloatsByEqualityOperator
    }

    private void ApplyColor() {
        int current;
        do {
            current = Volatile.Read(ref _colorChangeRequested);
            if(current == 0) return;
            _image.color = _color;
        } while(Interlocked.CompareExchange(ref _colorChangeRequested, 0, current) != current);
    }
}