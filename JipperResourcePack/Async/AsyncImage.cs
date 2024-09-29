using UnityEngine;
using UnityEngine.UI;

namespace JipperResourcePack.Async;

public class AsyncImage : AsyncBehaviour {
    public Image image;
    private Color? _color;
    
    public Color color {
        get => _color ?? image.color;
        set {
            if(_color == image.color) return;
            _color = value;
        }
    }
    
    private void Awake() {
        image = GetComponent<Image>();
    }
    
    private void Update() {
        if(_color != null) {
            image.color = _color.Value;
            _color = null;
        }
    }
}