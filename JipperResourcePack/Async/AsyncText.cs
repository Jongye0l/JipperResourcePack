using TMPro;
using UnityEngine;

namespace JipperResourcePack.Async;

public class AsyncText : MonoBehaviour {
    public TextMeshProUGUI tmp;
    private string _text;
    
    public string text {
        get => _text ?? tmp.text;
        set {
            if(_text == tmp.text) return;
            _text = value;
        }
    }

    private void Awake() {
        tmp = GetComponent<TextMeshProUGUI>();
    }
    
    private void Update() {
        if(_text == null) return;
        tmp.text = _text;
        _text = null;
    }
}