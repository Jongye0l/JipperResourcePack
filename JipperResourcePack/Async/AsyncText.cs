using System.Threading;
using JALib.Tools;
using TMPro;
using UnityEngine;

namespace JipperResourcePack.Async;

public class AsyncText(TextMeshProUGUI tmp) {
    public readonly TextMeshProUGUI TMP = tmp;
    private string _text;
    private int _textChangeRequested;

    public string Text {
        get => _text;
        set {
            if(_text == value) return;
            _text = value;
            if(Interlocked.Increment(ref _textChangeRequested) == 1) MainThread.Run(Main.Instance, ApplyText);
        }
    }

    private void ApplyText() {
        int current;
        do {
            current = Volatile.Read(ref _textChangeRequested);
            if(current == 0) return;
            TMP.text = _text;
        } while(Interlocked.CompareExchange(ref _textChangeRequested, 0, current) != current);
    }

    public void SetTextForce(string text) {
        _text = text;
        TMP.text = _text;
    }
}