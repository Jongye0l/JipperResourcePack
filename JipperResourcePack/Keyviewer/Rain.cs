using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace JipperResourcePack.Keyviewer;

public class Rain : MonoBehaviour {
    public RainPool pool;
    public Graphic image;
    public new RectTransform transform;
    public RawRain RawRain;
    public bool isGhost;

    public void Setup(bool isGhost) {
        if(isGhost) {
            Image img = gameObject.AddComponent<Image>();
            img.sprite = BundleLoader.GhostRain;
            img.type = Image.Type.Tiled;
            image = img;
        } else image = gameObject.AddComponent<RawImage>();
        this.isGhost = isGhost;
    }

    public void Update() {
        if(RawRain.Removed) {
            RawRain = null;
            pool.AddPool(this, isGhost);
            return;
        }
        if(RawRain.SizeDelta.HasValue) {
            transform.sizeDelta = RawRain.SizeDelta.Value;
            RawRain.SizeDelta = null;
        }
        if(RawRain.AnchoredPosition.HasValue) {
            transform.anchoredPosition = RawRain.AnchoredPosition.Value;
            RawRain.AnchoredPosition = null;
        }
    }
}