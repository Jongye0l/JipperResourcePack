using System.Threading;
using JALib.Tools;
using TMPro;
using UnityEngine;

namespace JipperResourcePack.Async;

public class AsyncText(TextMeshProUGUI tmp) {
    public readonly TextMeshProUGUI TMP = tmp;
    private string _text;
    private Color _color;
    private int _textChangeRequested;
    private int _colorChangeRequested;

    public string Text {
        get => _text;
        set {
            if(_text == value) return;
            _text = value;
            if(MainThread.IsMainThread()) TMP.text = _text;
            else if(Interlocked.Increment(ref _textChangeRequested) == 1) MainThread.Run(Main.Instance, ApplyText);
        }
    }
    
    public Color Color {
        get => _color;
        set {
            if(EqualColor(_color, value)) return;
            _color = value;
            if(MainThread.IsMainThread()) TMP.color = _color;
            else if(Interlocked.Increment(ref _colorChangeRequested) == 1) MainThread.Run(Main.Instance, ApplyColor);
        }
    }

    private static bool EqualColor(in Color a, in Color b) {
        // ReSharper disable CompareOfFloatsByEqualityOperator
        return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        // ReSharper restore CompareOfFloatsByEqualityOperator
    }

    private void ApplyText() {
        int current;
        do {
            current = Volatile.Read(ref _textChangeRequested);
            if(current == 0) return;
            TMP.text = _text;
        } while(Interlocked.CompareExchange(ref _textChangeRequested, 0, current) != current);
    }

    private void ApplyColor() {
        int current;
        do {
            current = Volatile.Read(ref _colorChangeRequested);
            if(current == 0) return;
            TMP.color = _color;
        } while(Interlocked.CompareExchange(ref _colorChangeRequested, 0, current) != current);
    }

    public void SetTextForce(string text) {
        _text = text;
        TMP.text = _text;
    }
}