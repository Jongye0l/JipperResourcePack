using UnityEngine;
using UnityEngine.UI;

namespace JipperResourcePack.Async;

public class AsyncImage(Image image) {
    public readonly Image Image = image;
    public Color Color = image.color;
}