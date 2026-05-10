using System.Collections.Generic;
using UnityEngine;

namespace JipperResourcePack.Keyviewer;

public class RainManager : MonoBehaviour {
    public readonly List<Rain> RainList = [];

    private void Update() {
        if(RainList.Count == 0) return;
        long time = KeyViewer.Stopwatch.ElapsedMilliseconds;
        float speed = KeyViewer.Settings.rainSpeed;
        float height = KeyViewer.Settings.rainHeight;
        for(int i = 0; i < RainList.Count; i++) {
            Rain rain = RainList[i];
            RawRain rawRain = rain.RawRain;
            float y = (time - rawRain.StartTime) / 300f * speed;
            if(rawRain.FinishSize) {
                if(y > height) {
                    float sizeY = rawRain.FinalSizeY - y + height;
                    if(sizeY < 0) {
                        RainList.RemoveAt(i--);
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