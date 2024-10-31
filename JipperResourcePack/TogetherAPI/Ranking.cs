using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JipperResourcePack.TogetherAPI;

public class Ranking : MonoBehaviour {

    public Image profileImage;
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI judgementText;
    public TextMeshProUGUI deathText;
    public new RectTransform transform;

    private void Awake() {
        transform = gameObject.AddComponent<RectTransform>();
        transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(0.5f, 1);
        transform.sizeDelta = new Vector2(300, 60);
        GameObject obj = new("Profile");
        RectTransform profileTransform = obj.GetComponent<RectTransform>();
        profileTransform.SetParent(transform);
        profileTransform.anchorMin = profileTransform.anchorMax = profileTransform.pivot = new Vector2(0, 0.5f);
        profileTransform.anchoredPosition = new Vector2(5, 0);
        profileTransform.sizeDelta = new Vector2(50, 50);
        Image image = obj.AddComponent<Image>();
        image.sprite = BundleLoader.KeyBackground;
        obj.AddComponent<Mask>();
        obj = new GameObject("Image");
        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.SetParent(profileTransform);
        rectTransform.sizeDelta = new Vector2(50, 50);
        profileImage = obj.AddComponent<Image>();
        obj = new GameObject("Username");
        rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.SetParent(transform);
        rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = new Vector2(65, 10);
        rectTransform.sizeDelta = new Vector2(45, 30);
        usernameText = obj.AddComponent<TextMeshProUGUI>();
        usernameText.font = BundleLoader.FontAsset;
        usernameText.fontSize = 20;
        usernameText.alignment = TextAlignmentOptions.Left;
        usernameText.color = Color.white;
        obj = new GameObject("JudgeImage");
        rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.SetParent(transform);
        rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = new Vector2(65, -15);
        rectTransform.sizeDelta = new Vector2(15, 15);
        image = obj.AddComponent<Image>();
        image.sprite = RDConstants.data.bullseyeSprites[1];
        obj = new GameObject("JudgeText");
        rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.SetParent(transform);
        rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = new Vector2(82, -15);
        rectTransform.sizeDelta = new Vector2(35, 15);
        judgementText = obj.AddComponent<TextMeshProUGUI>();
        judgementText.font = BundleLoader.FontAsset;
        judgementText.fontSize = 10;
        judgementText.alignment = TextAlignmentOptions.Left;
        judgementText.color = Color.white;
        obj = new GameObject("DeathImage");
        rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.SetParent(transform);
        rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = new Vector2(130, -15);
        rectTransform.sizeDelta = new Vector2(15, 15);
        image = obj.AddComponent<Image>();
        image.sprite = BundleLoader.Skull;
        obj = new GameObject("DeathText");
        rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.SetParent(transform);
        rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = new Vector2(147, -15);
        rectTransform.sizeDelta = new Vector2(50, 15);
        deathText = obj.AddComponent<TextMeshProUGUI>();
        deathText.font = BundleLoader.FontAsset;
        deathText.fontSize = 10;
        deathText.alignment = TextAlignmentOptions.Left;
        deathText.color = Color.white;
    }
}