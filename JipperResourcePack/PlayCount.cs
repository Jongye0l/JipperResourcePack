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
    private static string FilePath => Path.Combine(Main.Instance.Path, "Plays.dat");

    public static float Multiplier => (float) (ADOBase.conductor.song.pitch * ADOBase.controller.speed);

    public static void Load() {
        string path = FilePath;
        datas = new Dictionary<Hash, PlayData>();
        if(File.Exists(path)) {
            try {
                LoadFile(path);
                return;
            } catch (Exception e) {
                Main.Instance.LogException("Error On Load File", e);
                datas.Clear();
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
        fileStream.ReadByte();
        int count = fileStream.ReadInt();
        for(int i = 0; i < count; i++) {
            Hash key = fileStream.ReadBytes(16);
            PlayData value = fileStream.ReadObject<PlayData>(nullable: false);
            datas[key] = value;
        }
    }

    public static void Dispose() {
        datas = null;
    }

    public static void AddAttempts(float progress) => GetData().AddAttempts(progress, Multiplier);


    public static void RemoveAttempts(float progress) => GetData().RemoveAttempts(progress, Multiplier);


    public static void SetBest(float start, float cur) => GetData().SetBest(start, cur);


    public static void Save() {
        try {
            string path = FilePath;
            if(File.Exists(path)) File.Copy(path, path + ".bak", true);
            using FileStream fileStream = File.OpenWrite(path);
            fileStream.WriteByte(0);
            fileStream.WriteInt(datas.Count);
            foreach(KeyValuePair<Hash, PlayData> pair in datas) {
                if(pair.Value == null) continue;
                fileStream.Write(pair.Key.data);
                fileStream.WriteObject(pair.Value, nullable: false);
            }
        } catch (Exception e) {
            Main.Instance.LogException("Error On Save File", e);
        }
    }

    public static PlayData GetData() {
        Hash hash = GetMapHash();
        if(!datas.ContainsKey(hash)) datas[hash] = new PlayData();
        return datas[hash];
    }

    public class PlayData {
        public int totalAttempts;
        public Dictionary<(float, float), int> attempts = new();
        public Dictionary<(float, float), float> best = new();

        public void AddAttempts(float progress, float multiplier) {
            if(!attempts.TryAdd((progress, multiplier), 1)) attempts[(progress, multiplier)]++;
            totalAttempts++;
            Save();
        }

        public void RemoveAttempts(float progress, float multiplier) {
            if(!attempts.TryGetValue((progress, multiplier), out int value)) return;
            if(value == 1) attempts.Remove((progress, multiplier));
            else attempts[(progress, multiplier)]--;
            totalAttempts--;
            Save();
        }

        public void SetBest(float start, float cur) {
            (float, float) key = (start, Multiplier);
            if(best.TryAdd(key, cur)) return;
            if(best[key] < cur) best[key] = cur;
            Save();
        }

        public float GetBest(float start) => best.GetValueOrDefault((start, Multiplier), 0);

        public int GetAttempts(float progress) => attempts.GetValueOrDefault((progress, Multiplier), 0);

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
                    memoryStream.WriteFloat((float) levelEvent[levelEvent.Get<SpeedType>("speedType") == SpeedType.Bpm ? "beatsPerMinute" : "bpmMultiplier"]);
                    break;
                case LevelEventType.Twirl:
                    memoryStream.WriteInt(levelEvent.floor);
                    memoryStream.WriteByte(1);
                    break;
                case LevelEventType.Hold:
                    memoryStream.WriteInt(levelEvent.floor);
                    memoryStream.WriteByte(2);
                    memoryStream.WriteInt((int) levelEvent["duration"]);
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