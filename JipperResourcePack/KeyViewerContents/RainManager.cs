using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents;

public class RainManager : MonoBehaviour {
    public readonly List<Rain> RainList = [];
    public readonly ConcurrentQueue<RawRain> RawRainQueue = new();

    private void Update() {
        while(RawRainQueue.TryDequeue(out RawRain rawRain)) {
            Rain rainComponent = rawRain.Key.RainPool.GetOrNewRain(rawRain.IsGhost);
            rainComponent.Image.color = rawRain.Key.Color switch {
                1 => KeyViewer.Settings.RainColor,
                3 => KeyViewer.Settings.RainColor3,
                _ => KeyViewer.Settings.RainColor2
            };
            rainComponent.RawRain = rawRain;
            rainComponent.Transform.SetSiblingIndex(rawRain.IsGhost ? rawRain.Key.SiblingIndex + 1 : rawRain.Key.SiblingIndex);
            RainList.Add(rainComponent);
        }
        if(RainList.Count == 0) return;
        long time = KeyViewer.Stopwatch.ElapsedMilliseconds;
        float speed = KeyViewer.Settings.rainSpeed;
        float height = KeyViewer.Settings.rainHeight;
        for(int i = 0; i < RainList.Count; i++) {
            Rain rain = RainList[i];
            RawRain rawRain = rain.RawRain;
            if(!rain.Transform) {
                Main.Instance.Warning("Rain transform is null, this should not happen");
                rain = rain.Pool.GetOrNewRain(rain.IsGhost);
                rain.RawRain = rawRain;
                rain.Transform.SetSiblingIndex(rawRain.IsGhost ? rawRain.Key.SiblingIndex + 1 : rawRain.Key.SiblingIndex);
                if(rawRain.FinishSize) rain.Transform.sizeDelta = new Vector2(rawRain.XSize, rawRain.FinalSizeY);
                rawRain.SizeOver = false;
            }
            float y = (time - rawRain.StartTime) / 300f * speed;
            if(rawRain.FinishSize) {
                if(y > height) {
                    float sizeY = rawRain.FinalSizeY - y + height;
                    if(sizeY < 0) {
                        RainList.RemoveAt(i--);
                        RawRain.AddPool(rawRain);
                        rain.RawRain = null;
                        rain.Pool.AddPool(rain, rain.IsGhost);
                        continue;
                    }
                    rain.Transform.sizeDelta = new Vector2(rawRain.XSize, sizeY);
                    if(rawRain.SizeOver) continue;
                    rain.Transform.anchoredPosition = new Vector2(0, height);
                    rawRain.SizeOver = true;
                } else {
                    rain.Transform.anchoredPosition = new Vector2(0, y);
                    if(rawRain.FinishSizeSetup) continue;
                    rain.Transform.sizeDelta = new Vector2(rawRain.XSize, rawRain.FinalSizeY);
                    rawRain.FinishSizeSetup = true;
                }
            } else {
                if(y > height) {
                    if(rawRain.SizeOver) continue;
                    rain.Transform.sizeDelta = new Vector2(rawRain.XSize, height);
                    rain.Transform.anchoredPosition = new Vector2(0, height);
                    rawRain.SizeOver = true;
                } else {
                    rain.Transform.sizeDelta = new Vector2(rawRain.XSize, y);
                    rain.Transform.anchoredPosition = new Vector2(0, y);
                }
            }
        }
    }
}