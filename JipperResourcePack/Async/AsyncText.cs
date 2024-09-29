using TMPro;
using UnityEngine;

namespace JipperResourcePack.Async;

public class AsyncText : AsyncBehaviour {
    public TextMeshProUGUI tmp;
    private string _text;
    private Color? _color;
    
    public string text {
        get => _text ?? tmp.text;
        set {
            if(_text == tmp.text) return;
            _text = value;
        }
    }
    
    public Color color {
        get => _color ?? tmp.color;
        set {
            if(_color == tmp.color) return;
            _color = value;
        }
    }

    private void Awake() {
        tmp = GetComponent<TextMeshProUGUI>();
    }
    
    private void Update() {
        if(_text != null) {
            tmp.text = _text;
            _text = null;
        }
        if(_color != null) {
            tmp.color = _color.Value;
            _color = null;
        }
    }
}