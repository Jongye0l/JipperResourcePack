using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ADOFAI;
using HarmonyLib;
using JALib.Tools.ByteTool;

namespace JipperResourcePack;

public class PlayCount {

    public static Dictionary<Hash, PlayData> datas;
    private static string filePath => Path.Combine(Main.Instance.Path, "Plays.dat");

    public static void Load() {
        string path = filePath;
        datas = new Dictionary<Hash, PlayData>();
        if(File.Exists(path)) {
            try {
                LoadFile(path);
                return;
            } catch (Exception e) {
                Main.Instance.LogException("Error On Load File", e);
            }
        }
        path += ".bak";
        if(!File.Exists(path)) return;
        try {
            LoadFile(path);
        } catch (Exception e) {
            Main.Instance.LogException("Error On Load Backup File", e);
        }
    }

    private static void LoadFile(string path) {
        FileStream fileStream = File.OpenRead(path);
        int count = fileStream.ReadInt();
        for(int i = 0; i < count; i++) {
            Hash key = fileStream.ReadBytes(16);
            PlayData value = fileStream.ReadObject<PlayData>();
            datas.Add(key, value);
        }
    }

    public static void Dispose() {
        datas = null;
    }

    public static void AddAttempts() {
        Hash hash = GetMapHash();
        if(!datas.ContainsKey(hash)) datas[hash] = new PlayData();
        datas[hash].AddAttempts(scrController.instance.percentComplete);
    }

    public static void SetBest(float start) {
        Hash hash = GetMapHash();
        if(!datas.ContainsKey(hash)) datas[hash] = new PlayData();
        datas[hash].SetBest(start, scrController.instance.percentComplete);
    }

    public static void Save() {
        string path = filePath;
        if(File.Exists(path)) {
            string backupPath = path + ".bak";
            if(File.Exists(backupPath)) File.Delete(backupPath);
            File.Move(path, backupPath);
        }
        using FileStream fileStream = new(path, FileMode.Create);
        fileStream.WriteInt(datas.Count);
        foreach(KeyValuePair<Hash, PlayData> pair in datas) {
            fileStream.Write(pair.Key.data);
            fileStream.WriteObject(pair.Value);
        }
    }

    public static PlayData GetData() {
        return datas.GetValueOrDefault(GetMapHash());
    }

    public class PlayData {
        public int totalAttempts;
        public Dictionary<float, int> attempts = new();
        public Dictionary<float, float> best = new();

        public int this[float progress] => attempts.GetValueOrDefault(progress, 0);

        public void AddAttempts(float progress) {
            if(!attempts.TryAdd(progress, 1)) attempts[progress]++;
            totalAttempts++;
            Save();
        }

        public void SetBest(float start, float cur) {
            if(best.TryAdd(start, cur)) return;
            if(best[start] < cur) best[start] = cur;
            Save();
        }

        public static implicit operator int(PlayData data) => data.totalAttempts;
    }

    #region BetterCalibration Hash Algorithm

    private static Hash GetMapHash() {
        using MD5 md5 = MD5.Create();
        return md5.ComputeHash(ADOBase.isOfficialLevel ? Encoding.UTF8.GetBytes(ADOBase.currentLevel) : GetHash());
    }

    private static byte[] GetHash() {
        using MemoryStream memoryStream = new();
        scrLevelMaker lm = ADOBase.lm;
        if(lm.isOldLevel) memoryStream.WriteUTF(lm.leveldata);
        else memoryStream.WriteObject(lm.floorAngles);
        foreach(LevelEvent levelEvent in ADOBase.customLevel.events) {
            switch(levelEvent.eventType) {
                case LevelEventType.SetSpeed:
                    memoryStream.WriteInt(levelEvent.floor);
                    memoryStream.WriteByte(0);
                    memoryStream.WriteByte((byte) levelEvent.Get<SpeedType>("speedType"));
                    if((SpeedType) levelEvent["speedType"] == SpeedType.Bpm) memoryStream.WriteFloat((float) levelEvent["beatsPerMinute"]);
                    else memoryStream.WriteFloat((float) levelEvent["bpmMultiplier"]);
                    break;
                case LevelEventType.Twirl:
                    memoryStream.WriteInt(levelEvent.floor);
                    memoryStream.WriteByte(1);
                    break;
                case LevelEventType.Hold:
                    memoryStream.WriteInt(levelEvent.floor);
                    memoryStream.WriteByte(2);
                    memoryStream.WriteFloat((float) levelEvent["duration"]);
                    break;
                case LevelEventType.MultiPlanet:
                    memoryStream.WriteInt(levelEvent.floor);
                    memoryStream.WriteByte(3);
                    memoryStream.WriteByte((byte) levelEvent.Get<PlanetCount>("planets"));
                    break;
                case LevelEventType.Pause:
                    memoryStream.WriteInt(levelEvent.floor);
                    memoryStream.WriteByte(4);
                    memoryStream.WriteFloat((float) levelEvent["duration"]);
                    break;
                case LevelEventType.AutoPlayTiles:
                    memoryStream.WriteInt(levelEvent.floor);
                    memoryStream.WriteByte(5);
                    memoryStream.WriteBoolean((bool) levelEvent["enabled"]);
                    break;
                case LevelEventType.ScaleMargin:
                    memoryStream.WriteInt(levelEvent.floor);
                    memoryStream.WriteByte(6);
                    memoryStream.WriteFloat((float) levelEvent["scale"]);
                    break;
                case LevelEventType.Multitap:
                    memoryStream.WriteInt(levelEvent.floor);
                    memoryStream.WriteByte(7);
                    memoryStream.WriteFloat((float) levelEvent["taps"]);
                    break;
                case LevelEventType.KillPlayer:
                    memoryStream.WriteInt(levelEvent.floor);
                    memoryStream.WriteByte(8);
                    break;
            }
        }
        return memoryStream.ToArray();
    }

    public readonly struct Hash(byte[] data) : IEquatable<Hash> {
        public readonly byte[] data = data;

        public override bool Equals(object obj) => obj is Hash hash ? Equals(hash) : obj is byte[] bytes && Equals(bytes);
        public bool Equals(Hash other) => Equals(other.data);
        public bool Equals(byte[] hash) {
            if(data.Length != hash.Length) return false;
            return data.Length == hash.Length && !data.Where((t, i) => t != hash[i]).Any();
        }
        public override int GetHashCode() => data != null ? ToString().GetHashCode() : 0;

        public static bool operator ==(Hash left, Hash right) => left.Equals(right);
        public static bool operator !=(Hash left, Hash right) => !(left == right);

        public static implicit operator Hash(byte[] hash) => new(hash);
        public static implicit operator byte[](Hash hash) => hash.data;

        public override string ToString() => data.Join(b => b.ToString("x2"), "");
    }

    #endregion
}