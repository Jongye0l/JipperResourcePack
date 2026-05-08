using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JipperResourcePack.Installer.Resource;
using Newtonsoft.Json.Linq;

namespace JipperResourcePack.Installer.Screen;

public class InstallScreen : Screen {
    public ProgressBar ProgressBar;
    public Panel LogPanel;
    public Label LogLabel;
    public Label LastLogLabel = new() { Location = new Point(0, -10) };
    public ConcurrentQueue<string> LogQueue;
    
    public int Progress;
    public string DownloadName;
    public int DownloadProgressStart;
    public int DownloadProgressEnd;
    public bool Complete;
    public Exception Exception;

    public string TempPath;

    public InstallScreen(Screen screen) {
        PrevScreen = screen;
        NextScreen = new FinishScreen(this);
        Cancelable = false;
        Prevable = false;
        Nextable = false;
    }

    public override void OnEnter() {
        TopPanelLabels[2].Font = Constants.Arial16B;
        
        int maxValue = 0;
        if(GlobalSetting.Instance.IsUninstall) {
            maxValue = GlobalSetting.Instance.UninstallOption switch {
                0 => 4,
                1 => 3,
                _ => GlobalSetting.Instance.RemoveRequestMods.Count
            };
        } else {
            if(GlobalSetting.Instance.InstallUnityModManager) maxValue += 60;
            if(GlobalSetting.Instance.InstallDoorstop) maxValue += 80;
            if(GlobalSetting.Instance.InstallJalib) maxValue += 70;
            if(GlobalSetting.Instance.InstallJipperResourcePack) maxValue += 70;
            maxValue += 50 * GlobalSetting.Instance.SelectedMods.Count;
            maxValue += 10 * GlobalSetting.Instance.RemoveRequestMods.Count;
        }
        
        ProgressBar = new ProgressBar {
            Minimum = 0,
            Maximum = maxValue,
            Size = new Size(800, 20),
            Location = new Point(92, 120)
        };
        LogLabel = new Label {
            Font = Constants.Arial12,
            AutoSize = true,
            Location = new Point(92, 95)
        };
        LogPanel = new Panel {
            Location = new Point(92, 150),
            Size = new Size(800, 400),
            AutoScroll = true,
            BackColor = Color.Silver
        };
        
        MainPanel.SuspendLayout();
        MainPanel.Controls.Add(ProgressBar);
        MainPanel.Controls.Add(LogLabel);
        MainPanel.Controls.Add(LogPanel);
        MainPanel.ResumeLayout();
        
        LogQueue = new ConcurrentQueue<string>([
            "JipperResourcePack Installer v" + Application.ProductVersion,
            "Adofai Version: " + (GlobalSetting.Instance.AdofaiRevision == -1 ? "Unknown" : "r" + GlobalSetting.Instance.AdofaiRevision),
            "UnityModManager Installed: " + GlobalSetting.Instance.InstallUnityModManager,
            "Doorstop Installed: " + GlobalSetting.Instance.InstallDoorstop,
            "JALib Installed: " + GlobalSetting.Instance.InstallJalib,
            "JipperResourcePack Installed: " + GlobalSetting.Instance.InstallJipperResourcePack,
            "IsInstall: " + !GlobalSetting.Instance.IsUninstall,
            "Starting Work... Progress Maximum: " + maxValue
        ]);
        TempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "JipperResourcePack");
        
        Task.Run(StartWork);
        LogListener();
    }

    public async void StartWork() {
        try {
            if(GlobalSetting.Instance.IsUninstall) {
                string targetPath;
                switch(GlobalSetting.Instance.UninstallOption) {
                    case 0:
                        targetPath = Path.Combine(GlobalSetting.Instance.InstallPath, "Mods");
                        Log($"Deleting {targetPath}...");
                        Directory.Delete(targetPath, true);
                        Progress = 1;
                        goto case 1;
                    case 1:
                        targetPath = Path.Combine(GlobalSetting.Instance.InstallPath, "A Dance of Fire and Ice_Data", "Managed", "UnityModManager");
                        Log($"Deleting {targetPath}...");
                        Directory.Delete(targetPath, true);
                        Progress++;
                        targetPath = Path.Combine(GlobalSetting.Instance.InstallPath, "winhttp.dll");
                        Log($"Deleting {targetPath}...");
                        File.Delete(targetPath);
                        Progress++;
                        targetPath = Path.Combine(GlobalSetting.Instance.InstallPath, "doorstop_config.ini");
                        Log($"Deleting {targetPath}...");
                        File.Delete(targetPath);
                        Progress++;
                        break;
                    default:
                        foreach(string path in GlobalSetting.Instance.RemoveRequestMods) {
                            Directory.Delete(path, true);
                            Log($"Deleting {path}...");
                            Progress++;
                        }
                        break;
                }
            } else {
                if(GlobalSetting.Instance.InstallUnityModManager) await InstallUnityModManager();
                if(GlobalSetting.Instance.InstallDoorstop) await InstallDoorstop();
            
                // Remove Before Install
                foreach(string path in GlobalSetting.Instance.RemoveRequestMods) {
                    Log($"Deleting {path}...");
                    Directory.Delete(path, true);
                    Progress += 10;
                }
            
                if(GlobalSetting.Instance.InstallJalib) await DownloadJALib();
                if(GlobalSetting.Instance.InstallJipperResourcePack) await DownloadJipperResourcePack();
                foreach(ModData selectedMod in GlobalSetting.Instance.SelectedMods) {
                    Log("Downloading " + selectedMod.Name + "...");
                    DownloadName = selectedMod.Name;
                    DownloadProgressStart = Progress;
                    DownloadProgressEnd = Progress + 50;
                    await Download(selectedMod.URL, Path.Combine(GlobalSetting.Instance.InstallPath, "Mods", selectedMod.Name), true);
                    Progress = DownloadProgressEnd;
                    Log("Download Complete " + selectedMod.Name);
                }
            }
            Complete = true;
        } catch (Exception e) {
            Exception = e;
            Log("Error: " + e.Message);
        }
    }

    public async Task InstallUnityModManager() {
        string managerPath = Path.Combine(GlobalSetting.Instance.InstallPath, "A Dance of Fire and Ice_Data", "Managed", "UnityModManager");
        if(Directory.Exists(TempPath)) Directory.Delete(TempPath, true);
        Directory.CreateDirectory(TempPath);
        try {
            Application.ApplicationExit += RemoveDirectory;
            Log("Downloading UnityModManager...");
            DownloadName = "UnityModManager";
            DownloadProgressStart = 0;
            DownloadProgressEnd = 50;
            await Download("https://www.dropbox.com/s/wz8x8e4onjdfdbm/UnityModManager.zip?dl=1", TempPath, false);
            Log("Download Complete UnityModManager");
            Progress = 50;

            if(!Directory.Exists(managerPath))
                Directory.CreateDirectory(managerPath);
            foreach(string destFileName in (string[]) ["0Harmony.dll", "dnlib.dll", "UnityModManager.dll", "UnityModManager.xml"]) {
                Log($"Coping {destFileName}...");
                File.Copy(Path.Combine(TempPath, "UnityModManagerInstaller", destFileName), Path.Combine(managerPath, destFileName), true);
                Progress += 2;
            }
            
            Log("Coping Confix.xml...");
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("JipperResourcePack.Installer.Resource.UMM.Config.xml");
            using Stream file = File.Create(Path.Combine(managerPath, "Config.xml"));
            await stream!.CopyToAsync(file);
            Progress = 60;
            Log("Apply Complete UnityModManager");
        } finally {
            RemoveDirectory(null, null);
        }
    }

    public void RemoveDirectory(object obj, EventArgs args) {
        Directory.Delete(TempPath, true);
        Application.ApplicationExit -= RemoveDirectory;
    }

    public async Task InstallDoorstop() {
        string doorstopPath = Path.Combine(GlobalSetting.Instance.InstallPath, "winhttp.dll");
        string doorstopConfigPath = Path.Combine(GlobalSetting.Instance.InstallPath, "doorstop_config.ini");
        int startProgress = Progress;
        
        Log("Check Doorstop Latest Version...");
        string latestVersion;
        using HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("JipperResourcePack");
        try {
            HttpResponseMessage response = await httpClient.GetAsync("https://api.github.com/repos/NeighTools/UnityDoorstop/releases/latest");
            Progress = startProgress + 7;
            response.EnsureSuccessStatusCode();
            Progress = startProgress + 14;
            string json = await response.Content.ReadAsStringAsync();
            latestVersion = JObject.Parse(json)["tag_name"]!.Value<string>();
        } catch (Exception) {
            HttpResponseMessage response = await httpClient.GetAsync("https://api.github.com/repos/NeighTools/UnityDoorstop/releases");
            Progress = startProgress + 10;
            response.EnsureSuccessStatusCode();
            Progress = startProgress + 16;
            string json = await response.Content.ReadAsStringAsync();
            latestVersion = JArray.Parse(json)[0]["tag_name"]!.Value<string>();
        }
        Progress = startProgress + 20;
        Log("Latest Version is " + latestVersion);
        
        if(Directory.Exists(TempPath)) Directory.Delete(TempPath, true);
        Directory.CreateDirectory(TempPath);
        try {
            Application.ApplicationExit += RemoveDirectory;
            DownloadName = "Doorstop";
            DownloadProgressStart = Progress;
            DownloadProgressEnd = Progress + 50;
            Log("Downloading Doorstop...");
            await Download($"https://github.com/NeighTools/UnityDoorstop/releases/download/{latestVersion}/doorstop_win_release_{latestVersion.TrimStart('v')}.zip", TempPath, false);
            Log("Download Complete Doorstop");
            
            Log("Checking program bits...");
            bool is64Bit = UnmanagedDllIs64Bit(Path.Combine(GlobalSetting.Instance.InstallPath, "A Dance of Fire and Ice.exe"));
            Progress += 2;
            Log($"Program is {(is64Bit ? "64" : "32")} bit");

            if(File.Exists(doorstopPath)) {
                Log($"Deleting {doorstopPath}...");
                File.Delete(doorstopPath);
            }
            Progress += 2;
        
            if(File.Exists(doorstopConfigPath)) {
                Log($"Deleting {doorstopConfigPath}...");
                File.Delete(doorstopConfigPath);
            }
            Progress += 2;
        
            Log("Coping winhttp.dll...");
            File.Copy(Path.Combine(TempPath, is64Bit ? "x64" : "x86", "winhttp.dll"), doorstopPath);
            Progress += 2;
            
            using(Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("JipperResourcePack.Installer.Resource.UMM.doorstop_config.ini")) {
                using Stream file = File.Create(doorstopConfigPath);
                await stream!.CopyToAsync(file);
            }
            Progress = startProgress + 80;
            Log("Apply Complete Doorstop");
        } finally {
            RemoveDirectory(null, null);
        }
    }

    public static ushort GetDllMachineType(string dllPath) {
        using FileStream input = new(dllPath, FileMode.Open, FileAccess.Read);
        using BinaryReader binaryReader = new(input);
        input.Seek(60L, SeekOrigin.Begin);
        input.Seek(binaryReader.ReadInt32(), SeekOrigin.Begin);
        return binaryReader.ReadUInt32() == 17744U ? binaryReader.ReadUInt16() : throw new Exception("Can't find PE header");
    }

    public bool UnmanagedDllIs64Bit(string dllPath) {
        try {
            switch(GetDllMachineType(dllPath)) {
                case 512: // IMAGE_FILE_MACHINE_IA64
                case 34404: // IMAGE_FILE_MACHINE_AMD64
                    return true;
                default:
                    return false;
            }
        } catch (Exception ex) {
            Log("Unable to determine the bitness of " + dllPath);
            Log(ex.ToString());
            return false;
        }
    }

    public void Log(string text) => LogQueue.Enqueue(text);

    public async void LogListener() {
        try {
            while(!Complete && Exception == null) {
                WorkLog();
                ProgressBar.Value = Progress;
                await Task.Delay(1);
            }
            WorkLog();
            Next();
        } catch (Exception e) {
            MessageBox.Show(e.Message, Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void WorkLog() {
        if(InstallStream.Instance?.NeedUpdate(out double value) == true) {
            value = 0.2 + value * 0.8;
            int realStart = DownloadProgressStart + 10;
            Progress = realStart + (int)((DownloadProgressEnd - realStart) * value);
            Log($"Downloading {DownloadName}... {(int)(value * 100)}%");
        } else if(LogQueue.IsEmpty) return;

        LogPanel.SuspendLayout();
        Label label = LastLogLabel;
        while(LogQueue.TryDequeue(out string log)) {
            LogLabel.Text = log;
            LogPanel.Controls.Add(label = new Label { Text = log, AutoSize = true, Location = new Point(0, label.Location.Y + 20) });
            Console.WriteLine(log);
        }

        LastLogLabel = label;
        LogPanel.VerticalScroll.Value = LogPanel.VerticalScroll.Maximum;
        LogPanel.ResumeLayout();
    }

    public override void OnLeave() {
        TopPanelLabels[2].Font = Constants.Arial16;
    }

    public async Task Download(string url, string path, bool checkInfo) {
        if(!Directory.Exists(path)) Directory.CreateDirectory(path);
        HttpResponseMessage message = await InstallerForm.HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        Log($"Downloading {DownloadName}... 16%");
        Progress = DownloadProgressStart + 8;
        long contentLength = message.Content.Headers.ContentLength ?? -1;
        using Stream stream = new InstallStream(await message.Content.ReadAsStreamAsync(), contentLength);
        Progress = DownloadProgressStart + 2;
        Log($"Downloading {DownloadName}... 20%");
        Unzip(stream, path, checkInfo);
    }

    public static void Unzip(Stream stream, string path, bool checkInfo) {
        using ZipArchive archive = new(stream, ZipArchiveMode.Read, false, Encoding.UTF8);
        foreach(ZipArchiveEntry entry in archive.Entries) {
            string entryPath = Path.Combine(path, entry.FullName);
            if(entryPath.EndsWith("/")) Directory.CreateDirectory(entryPath);
            else {
                string directory = Path.GetDirectoryName(entryPath)!;
                if(!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                using FileStream fileStream = new(entryPath, FileMode.Create);
                entry.Open().CopyTo(fileStream);
            }
        }
        if(!checkInfo || File.Exists(Path.Combine(path, "Info.json"))) return;
        string folder = Directory.GetDirectories(path)[0];
        foreach(string file in Directory.GetFiles(folder)) {
            string fileName = Path.GetFileName(file);
            File.Move(file, Path.Combine(path, fileName));
        }
        foreach(string folder2 in Directory.GetDirectories(folder)) {
            string folderName = Path.GetFileName(folder2);
            Directory.Move(folder2, Path.Combine(path, folderName));
        }
    }

    public async Task DownloadJALib() {
        int startProgress = Progress;
        Log("Check JALib Latest Version...");
        string latestVersion;
        using HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("JipperResourcePack");
        try {
            HttpResponseMessage response = await httpClient.GetAsync("https://api.github.com/repos/Jongye0l/JALib/releases/latest");
            Progress = startProgress + 7;
            response.EnsureSuccessStatusCode();
            Progress = startProgress + 14;
            string json = await response.Content.ReadAsStringAsync();
            latestVersion = JObject.Parse(json)["tag_name"]!.Value<string>();
        } catch (Exception) {
            HttpResponseMessage response = await httpClient.GetAsync("https://api.github.com/repos/Jongye0l/JALib/releases");
            Progress = startProgress + 10;
            response.EnsureSuccessStatusCode();
            Progress = startProgress + 16;
            string json = await response.Content.ReadAsStringAsync();
            latestVersion = JArray.Parse(json)[0]["tag_name"]!.Value<string>();
        }
        Progress = startProgress + 20;
        Log("Latest Version is " + latestVersion);
        
        DownloadName = "JALib";
        DownloadProgressStart = Progress;
        DownloadProgressEnd = Progress + 50;
        Log("Downloading JALib...");
        await Download($"https://github.com/Jongye0l/JALib/releases/download/{latestVersion}/JALib.zip", Path.Combine(GlobalSetting.Instance.InstallPath, "Mods", "JALib"), true);
        Log("Download Complete JALib");
    }

    public async Task DownloadJipperResourcePack() {
        int startProgress = Progress;
        Log("Check JipperResourcePack Latest Version...");
        string latestVersion;
        using HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("JipperResourcePack");
        try {
            HttpResponseMessage response = await httpClient.GetAsync("https://api.github.com/repos/Jongye0l/JipperResourcePack/releases/latest");
            Progress = startProgress + 7;
            response.EnsureSuccessStatusCode();
            Progress = startProgress + 14;
            string json = await response.Content.ReadAsStringAsync();
            latestVersion = JObject.Parse(json)["tag_name"]!.Value<string>();
        } catch (Exception) {
            HttpResponseMessage response = await httpClient.GetAsync("https://api.github.com/repos/Jongye0l/JipperResourcePack/releases");
            Progress = startProgress + 10;
            response.EnsureSuccessStatusCode();
            Progress = startProgress + 16;
            string json = await response.Content.ReadAsStringAsync();
            latestVersion = JArray.Parse(json)[0]["tag_name"]!.Value<string>();
        }
        Progress = startProgress + 20;
        Log("Latest Version is " + latestVersion);
        
        DownloadName = "JipperResourcePack";
        DownloadProgressStart = Progress;
        DownloadProgressEnd = Progress + 50;
        Log("Downloading JipperResourcePack...");
        await Download($"https://github.com/Jongye0l/JipperResourcePack/releases/download/{latestVersion}/JipperResourcePack.zip", Path.Combine(GlobalSetting.Instance.InstallPath, "Mods", "JipperResourcePack"), true);
        Log("Download Complete JipperResourcePack");
    }

    private class InstallStream : Stream {
        public static InstallStream Instance;
        private readonly Stream _baseStream;
        private long _position;
        private long _lastCheckedPosition;
        
        public InstallStream(Stream baseStream, long length) {
            _baseStream = baseStream;
            Length = length;
            Instance = this;
        }

        public bool NeedUpdate(out double value) {
            if(Length == -1) {
                value = 0;
                return false;
            }
            long currentPosition = _position;
            if(currentPosition == _lastCheckedPosition) {
                value = 0;
                return false;
            }
            value = (double) currentPosition / Length;
            _lastCheckedPosition = currentPosition;
            return true;
        }

        public override void Flush() => _baseStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        
        public override int Read(byte[] buffer, int offset, int count) {
            int read = _baseStream.Read(buffer, offset, count);
            _position += read;
            return read;
        }

        public override int ReadByte() {
            int read = _baseStream.ReadByte();
            if(read != -1) _position++;
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            int read = await _baseStream.ReadAsync(buffer, offset, count, cancellationToken);
            _position += read;
            return read;
        }

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length { get; }

        public override long Position {
            get => _position;
            set => throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing) {
            if(!disposing) return;
            _baseStream.Dispose();
            Instance = null;
        }
    }
}