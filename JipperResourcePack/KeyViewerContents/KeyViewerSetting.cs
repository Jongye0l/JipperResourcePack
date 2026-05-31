using System.Threading.Tasks;
using JALib.Core;
using JALib.Core.Setting;
using JALib.Tools;
using JipperResourcePack.SettingTool;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents;

public class KeyViewerSetting : JASetting {
    public KeyviewerStyle KeyViewerStyle = KeyviewerStyle.Key16;
    public FootKeyviewerStyle FootKeyViewerStyle = FootKeyviewerStyle.Key4;

    // ReSharper disable InconsistentNaming
    public KeyCode[] key10 = [
        KeyCode.Tab, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.E, KeyCode.P, KeyCode.Equals, KeyCode.Backspace, KeyCode.Backslash,
        KeyCode.Space, KeyCode.Comma
    ];
    public string[] key10Text = new string[10];
    public KeyCode[] GhostKey10 = new KeyCode[10];

    public KeyCode[] key12 = [
        KeyCode.Tab, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.E, KeyCode.P, KeyCode.Equals, KeyCode.Backspace, KeyCode.Backslash,
        KeyCode.Space, KeyCode.C, KeyCode.Comma, KeyCode.Period
    ];
    public string[] key12Text = new string[12];
    public KeyCode[] GhostKey12 = new KeyCode[12];

    public KeyCode[] key16 = [
        KeyCode.Tab, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.E, KeyCode.P, KeyCode.Equals, KeyCode.Backspace, KeyCode.Backslash,
        KeyCode.Space, KeyCode.C, KeyCode.Comma, KeyCode.Period, KeyCode.CapsLock, KeyCode.LeftShift, KeyCode.Return, KeyCode.H
    ];
    public string[] key16Text = new string[16];
    public KeyCode[] GhostKey16 = new KeyCode[16];

    public KeyCode[] key20 = [
        KeyCode.Tab, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.E, KeyCode.P, KeyCode.Equals, KeyCode.Backspace, KeyCode.Backslash,
        KeyCode.Space, KeyCode.C, KeyCode.Comma, KeyCode.Period, KeyCode.CapsLock, KeyCode.LeftShift, KeyCode.Return, KeyCode.H,
        KeyCode.CapsLock, KeyCode.D, KeyCode.RightShift, KeyCode.Semicolon
    ];
    public string[] key20Text = new string[20];
    public KeyCode[] GhostKey20 = new KeyCode[20];

    public KeyCode[] footkey2 = [KeyCode.F8, KeyCode.F3];
    public KeyCode[] footkey4 = [KeyCode.F8, KeyCode.F3, KeyCode.F7, KeyCode.F2];
    public KeyCode[] footkey6 = [KeyCode.F8, KeyCode.F3, KeyCode.F7, KeyCode.F2, KeyCode.F6, KeyCode.F1];
    public KeyCode[] footkey8 = [KeyCode.F8, KeyCode.F4, KeyCode.F7, KeyCode.F3, KeyCode.F6, KeyCode.F2, KeyCode.F5, KeyCode.F1];
    public KeyCode[] footkey16 = [
        KeyCode.F8, KeyCode.F4, KeyCode.F7, KeyCode.F3, KeyCode.F6, KeyCode.F2, KeyCode.F5, KeyCode.F1,
        KeyCode.Alpha0, KeyCode.Alpha6, KeyCode.Alpha9, KeyCode.Alpha5, KeyCode.Alpha8, KeyCode.Alpha4, KeyCode.Alpha7, KeyCode.Alpha3
    ];

    public float YLocation = 200;
    public bool AutoSetupKeyLimit = true;
    public float Size = 1;
    public bool useRain = true;
    public bool useGhostRain;
    public float rainSpeed = 100;
    public float rainHeight = 200;
    // ReSharper restore InconsistentNaming

    public ColorCache Background = new(KeyViewer.Background);
    public ColorCache BackgroundClicked = new(KeyViewer.BackgroundClicked);
    public ColorCache Outline = new(KeyViewer.Outline);
    public ColorCache OutlineClicked = new(KeyViewer.OutlineClicked);
    public ColorCache Text = new(KeyViewer.Text);
    public ColorCache TextClicked = new(KeyViewer.TextClicked);
    public ColorCache RainColor = new(KeyViewer.RainColor);
    public ColorCache RainColor2 = new(KeyViewer.RainColor2);
    public ColorCache RainColor3 = new(KeyViewer.RainColor3);

    public KeyViewerSetting(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
        KeyViewer.Settings = this;
        if(jsonObject == null) return;
        if(jsonObject.TryGetValue("DownLocation", out JToken value)) {
            jsonObject.Remove("DownLocation");
            YLocation = value.Value<bool>() ? 0 : 200;
        }
        if(jsonObject.TryGetValue("Count", out JToken count)) {
            jsonObject.Remove("Count");
            JArray countArray = (JArray) count;
            KeyCountData.Instance ??= new KeyCountData();
            if(countArray.Count is KeyViewer.FootOutIndex or 24) {
                for(int i = 0; i < countArray.Count; i++) KeyCountData.Instance.Count[i] = countArray[i].Value<int>();
            } else {
                for(int i = 0; i < 16; i++) KeyCountData.Instance.Count[i] = countArray[i].Value<int>();
                for(int i = 16; i < 24; i++) KeyCountData.Instance.Count[i + 4] = countArray[i].Value<int>();
            }
        }
        if(jsonObject.TryGetValue("TotalCount", out JToken totalCount)) {
            jsonObject.Remove("TotalCount");
            KeyCountData.Instance ??= new KeyCountData();
            KeyCountData.Instance.TotalCount = totalCount.Value<int>();
        }
        if(KeyCountData.Instance != null) {
            KeyCountData.Instance.Save();
            Task.Yield().OnCompleted(Main.Instance.SaveSetting);
        }
    }
}