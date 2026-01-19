using UnityEngine;
using UnityEngine.UI;

namespace JipperResourcePack.Keyviewer;

public class Rain : MonoBehaviour {
    public Key key;
    public Image image;
    public new RectTransform transform;
    public RawRain rawRain;
    
    private void Awake() {
        image = gameObject.AddComponent<Image>();
    }

    public void Update() {
        if(rawRain.removed) {
            rawRain = null;
            key.AddPool(this);
            return;
        }
        if(rawRain.sizeDelta.HasValue) {
            transform.sizeDelta = rawRain.sizeDelta.Value;
            rawRain.sizeDelta = null;
        }
        if(rawRain.anchoredPosition.HasValue) {
            transform.anchoredPosition = rawRain.anchoredPosition.Value;
            rawRain.anchoredPosition = null;
        }
    }
}