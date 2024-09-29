using UnityEngine;
using UnityEngine.UI;

namespace JipperResourcePack.Keyviewer;

public class Rain : MonoBehaviour {
    public Image image;
    public new RectTransform transform;
    public RawRain rawRain;
    
    private void Awake() {
        transform = GetComponent<RectTransform>();
        image = gameObject.AddComponent<Image>();
    }

    public void Update() {
        if(rawRain.removed) {
            Destroy(gameObject);
            return;
        }
        if(rawRain.sizeDelta != null) {
            transform.sizeDelta = rawRain.sizeDelta.Value;
            rawRain.sizeDelta = null;
        }
        if(rawRain.anchoredPosition != null) {
            transform.anchoredPosition = rawRain.anchoredPosition.Value;
            rawRain.anchoredPosition = null;
        }
    }
}