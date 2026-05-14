using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace JipperResourcePack.OverlayContents;

public class ProgressBar {
    public readonly RectTransform LineTransform;
    public readonly Image LineImage;
    public readonly Image BorderImage;
    public readonly Image BackgroundImage;

    public ProgressBar(RectTransform rectTransform) {
        LineTransform = rectTransform.Find("line") as RectTransform;
        LineImage = LineTransform!.GetComponent<Image>();
        BorderImage = rectTransform.Find("borderLine").GetComponent<Image>();
        BackgroundImage = rectTransform.Find("background").GetComponent<Image>();
    }
}