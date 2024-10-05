using UnityEngine;
using UnityEngine.UI;

namespace JipperResourcePack;

public class ProgressBar : MonoBehaviour {
    public RectTransform LineTransform;
    public Image LineImage;
    public Image BorderImage;
    public Image BackgroundImage;

    private void Awake() {
        LineTransform = transform.Find("line") as RectTransform;
        LineImage = LineTransform.GetComponent<Image>();
        BorderImage = transform.Find("borderLine").GetComponent<Image>();
        BackgroundImage = transform.Find("background").GetComponent<Image>();
    }
}