using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ADOFAI;
using HarmonyLib;
using JALib.Tools.ByteTool;

namespace JipperResourcePack.OverlayContents;

public static class PlayCount {
    private static Dictionary<Hash, PlayData> _datas;
    private static string FilePath => Path.Combine(Main.Instance.Path, "Plays.dat");
    private static float Multiplier => (float) (ADOBase.conductor.song.pitch * VersionSafe.GetPlanetSpeed(scrController.instance));

    public static void Load() {
        string path = FilePath;
        _datas = new Dictionary<Hash, PlayData>();
        if(File.Exists(path)) {
            try {
                LoadFile(path);
                return;
            } catch (Exception e) {
                Main.Instance.LogException("Error On Load File", e);
                _datas.Clear();
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
        int version = fileStream.ReadByte();
        int count = fileStream.ReadInt();
        for(int i = 0; i < count; i++) {
            Hash key = fileStream.ReadBytes(16);
            (_datas[key] = new PlayData()).Read(fileStream, version);
        }
    }

    public static void Dispose() {
        _datas = null;
    }

    public static void AddAttempts(Hash hash, float progress) => GetData(hash).AddAttempts(progress, Multiplier);


    public static void RemoveAttempts(Hash hash, float progress) => GetData(hash).RemoveAttempts(progress, Multiplier);


    public static void SetBest(Hash hash, float start, float cur, float multiplier) {
        GetData(hash).SetBest(start, cur, multiplier);
    }


    public static void Save() {
        try {
            string path = FilePath;
            if(File.Exists(path)) File.Copy(path, path + ".bak", true);
            using FileStream fileStream = File.OpenWrite(path);
            using MemoryStream memoryStream = new();
            memoryStream.WriteByte(1);
            memoryStream.WriteInt(_datas.Count);
            foreach(KeyValuePair<Hash, PlayData> pair in _datas) {
                if(pair.Value == null) continue;
                memoryStream.Write(pair.Key.Data);
                pair.Value.Write(memoryStream);
            }
            memoryStream.WriteTo(fileStream);
        } catch (Exception e) {
            Main.Instance.LogException("Error On Save File", e);
        }
    }

    public static PlayData GetData(Hash hash) {
        if(!_datas.ContainsKey(hash)) _datas[hash] = new PlayData();
        return _datas[hash];
    }

    public class PlayData {
        private int _totalAttempts;
        private readonly Dictionary<(float, float), int> _attempts = new();
        private readonly Dictionary<(float, float), float> _best = new();

        public void AddAttempts(float progress, float multiplier) {
            if(!_attempts.TryAdd((progress, multiplier), 1)) _attempts[(progress, multiplier)]++;
            _totalAttempts++;
            Save();
        }

        public void RemoveAttempts(float progress, float multiplier) {
            if(!_attempts.TryGetValue((progress, multiplier), out int value)) return;
            if(value == 1) _attempts.Remove((progress, multiplier));
            else _attempts[(progress, multiplier)]--;
            _totalAttempts--;
            Save();
        }

        public void SetBest(float start, float cur, float multiplier) {
            (float, float) key = (start, multiplier);
            if(_best.TryAdd(key, cur)) return;
            if(!(_best[key] < cur)) return;
            _best[key] = cur;
            Save();
        }

        public void Write(Stream stream) {
            stream.WriteInt(_totalAttempts);
            stream.WriteInt(_attempts.Count);
            foreach(KeyValuePair<(float, float), int> pair in _attempts) {
                stream.WriteFloat(pair.Key.Item1);
                stream.WriteFloat(pair.Key.Item2);
                stream.WriteInt(pair.Value);
            }
            stream.WriteInt(_best.Count);
            foreach(KeyValuePair<(float, float), float> pair in _best) {
                stream.WriteFloat(pair.Key.Item1);
                stream.WriteFloat(pair.Key.Item2);
                stream.WriteFloat(pair.Value);
            }
        }

        public void Read(Stream stream, int version) {
            _totalAttempts = stream.ReadInt();
            int size = stream.ReadInt();
            _attempts.EnsureCapacity(size);
            for(int i = 0; i < size; i++) {
                if(version == 0) stream.ReadByte();
                _attempts[(stream.ReadFloat(), stream.ReadFloat())] = stream.ReadInt();
            }
            size = stream.ReadInt();
            _best.EnsureCapacity(size);
            for(int i = 0; i < size; i++) {
                if(version == 0) stream.ReadByte();
                _best[(stream.ReadFloat(), stream.ReadFloat())] = stream.ReadFloat();
            }
        }

        public float GetBest(float start, float multiplier) => _best.GetValueOrDefault((start, multiplier), 0);

        public int GetAttempts(float progress) => _attempts.GetValueOrDefault((progress, Multiplier), 0);
        
        public int GetAttempts() => _attempts.Values.Sum();

        public static implicit operator int(PlayData data) => data._totalAttempts;
    }

    #region BetterCalibration Hash Algorithm

    public static Hash GetMapHash() {
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
                    memoryStream.WriteByte((byte) (SpeedType) levelEvent["speedType"]);
                    // ReSharper disable once PossibleInvalidCastException
                    memoryStream.WriteFloat((float) levelEvent[(SpeedType) levelEvent["speedType"] == SpeedType.Bpm ? "beatsPerMinute" : "bpmMultiplier"]);
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
                    memoryStream.WriteByte((byte) (PlanetCount) levelEvent["planets"]);
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
        public readonly byte[] Data = data;

        public override bool Equals(object obj) => obj is Hash hash ? Equals(hash) : obj is byte[] bytes && Equals(bytes);
        public bool Equals(Hash other) => Equals(other.Data);
        public bool Equals(byte[] hash) {
            if(Data == hash) return true;
            if(Data == null || hash == null || Data.Length != hash.Length) return false;
            return Data.Length == hash.Length && !Data.Where((t, i) => t != hash[i]).Any();
        }
        public override int GetHashCode() => Data != null ? ToString().GetHashCode() : 0;

        public static bool operator ==(Hash left, Hash right) => left.Equals(right);
        public static bool operator !=(Hash left, Hash right) => !(left == right);

        public static implicit operator Hash(byte[] hash) => new(hash);
        public static implicit operator byte[](Hash hash) => hash.Data;

        public override string ToString() => Data.Join(b => b.ToString("x2"), "");
    }

    #endregion
}