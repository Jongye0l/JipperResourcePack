using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JipperResourcePack.Installer.Resource;
using Newtonsoft.Json.Linq;

namespace JipperResourcePack.Installer.Screen;

public class InstallScreen : Screen {

    public Label TitleLabel;
    public ProgressBar ProgressBar;
    public Panel LogPanel;
    public Label LogLabel;
    public int LogY = 10;
    public ConcurrentQueue<string> LogQueue;
    public int Progress;
    public string DownloadName;
    public int DownloadProgressStart;
    public int DownloadProgressEnd;
    public bool complete;
    public Exception exception;

    public InstallScreen(Screen screen) {
        PrevScreen = screen;
        NextScreen = new FinishScreen(this);
        Cancelable = false;
        Prevable = false;
        Nextable = false;
    }

    public override void OnEnter() {
        InstallerForm.WebClient.DownloadProgressChanged += WebClientOnDownloadProgressChanged;
        TitleLabel = new Label {
            Text = Resources.Current.Install_Install,
            Font = new Font("Arial", 16),
            AutoSize = true,
            Location = new Point(30, 8)
        };
        ProgressBar = new ProgressBar {
            Minimum = 0,
            Maximum = 110 + 50 * GlobalSetting.Instance.SelectedMods.Count,
            Size = new Size(800, 20),
            Location = new Point(92, 120)
        };
        LogLabel = new Label {
            Font = new Font("Arial", 12),
            AutoSize = true,
            Location = new Point(92, 95)
        };
        LogPanel = new Panel {
            Location = new Point(92, 150),
            Size = new Size(800, 400),
            AutoScroll = true,
            BackColor = Color.Silver
        };
        TopPanel.Controls.Add(TitleLabel);
        MainPanel.Controls.Add(ProgressBar);
        MainPanel.Controls.Add(LogLabel);
        MainPanel.Controls.Add(LogPanel);
        LogQueue = new ConcurrentQueue<string>();
        Task.Run(StartWork);
        LogListner();
    }

    public async void StartWork() {
        try {
            UnityModManagerCheck();
            await DownloadJALib();
            await DownloadJipperResourcePack();
            foreach(ModData selectedMod in GlobalSetting.Instance.SelectedMods) {
                Log("Downloading " + selectedMod.Name + "...");
                DownloadName = selectedMod.DisplayName;
                DownloadProgressStart = Progress;
                DownloadProgressEnd = Progress + 50;
                await Download(selectedMod.URL, Path.Combine(GlobalSetting.Instance.InstallPath, "Mods", selectedMod.Name));
                Progress = DownloadProgressEnd;
                Log("Download Complete " + selectedMod.Name);
            }
            complete = true;
        } catch (Exception e) {
            exception = e;
            Log("Error: " + e.Message);
        }
    }

    public void UnityModManagerCheck() {
        string path = GlobalSetting.Instance.InstallPath;
        Log("Founding UnityModManager...");
        if(File.Exists(Path.Combine(path, "A Dance of Fire and Ice_Data", "Managed", "UnityModManager", "UnityModManager.dll"))) {
            Log("UnityModManager already exists.");
            Progress = 20;
            return;
        }
        Progress = 10;
        Log("UnityModManager not found.");
        Log("Installing UnityModManager...");
        new UMMInstaller(this).Start();
        Log("UnityModManager Installed.");
        Progress = 20;
    }

    public void Log(string text) => LogQueue.Enqueue(text);

    public async void LogListner() {
        while(!complete && exception == null) {
            while(LogQueue.TryDequeue(out string log)) {
                LogLabel.Text = log;
                LogPanel.Controls.Add(new Label { Text = log, AutoSize = true, Location = new Point(0, LogY) });
                LogY += 20;
            }
            ProgressBar.Value = Progress;
            await Task.Delay(10);
        }
        while(LogQueue.TryDequeue(out string log)) {
            LogLabel.Text = log;
            LogPanel.Controls.Add(new Label { Text = log, AutoSize = true, Location = new Point(0, LogY) });
            LogY += 20;
        }
        try {
            Next();
        } catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
    }

    public override void OnLeave() {
        InstallerForm.WebClient.DownloadProgressChanged -= WebClientOnDownloadProgressChanged;
    }

    public async Task Download(string url, string path) {
        if(!Directory.Exists(path)) Directory.CreateDirectory(path);
        using Stream stream = await InstallerForm.WebClient.OpenReadTaskAsync(url);
        Unzip(stream, path);
    }

    public static void Unzip(Stream stream, string path) {
        using ZipArchive archive = new(stream, ZipArchiveMode.Read, false, Encoding.UTF8);
        foreach(ZipArchiveEntry entry in archive.Entries) {
            string entryPath = Path.Combine(path, entry.FullName);
            if(entryPath.EndsWith("/")) Directory.CreateDirectory(entryPath);
            else {
                string directory = Path.GetDirectoryName(entryPath);
                if(!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                using FileStream fileStream = new(entryPath, FileMode.Create);
                entry.Open().CopyTo(fileStream);
            }
        }
        if(File.Exists(Path.Combine(path, "Info.json"))) return;
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

    private void WebClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
        Progress = DownloadProgressStart + (int)((DownloadProgressEnd - DownloadProgressStart) * e.ProgressPercentage / 100f);
        Log($"Downloading {DownloadName}... {e.ProgressPercentage}%");
    }

    public async Task DownloadJALib() {
        DownloadName = "JALib Github Api";
        Log("Check JALib Latest Version...");
        string latestVersion;
        using HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("JipperResourcePack");
        try {
            HttpResponseMessage response = await httpClient.GetAsync("https://api.github.com/repos/Jongye0l/JALib/releases/latest");
            Progress = 27;
            response.EnsureSuccessStatusCode();
            Progress = 34;
            string json = await response.Content.ReadAsStringAsync();
            latestVersion = JObject.Parse(json)["tag_name"].Value<string>();
        } catch (Exception) {
            HttpResponseMessage response = await httpClient.GetAsync("https://api.github.com/repos/Jongye0l/JALib/releases");
            Progress = 30;
            response.EnsureSuccessStatusCode();
            Progress = 36;
            string json = await response.Content.ReadAsStringAsync();
            latestVersion = JArray.Parse(json)[0]["tag_name"].Value<string>();
        }
        Progress = 40;
        DownloadName = "JALib";
        DownloadProgressStart = Progress;
        DownloadProgressEnd = 90;
        Log("Downloading JALib...");
        await Download($"https://github.com/Jongye0l/JALib/releases/download/{latestVersion}/JALib.zip", Path.Combine(GlobalSetting.Instance.InstallPath, "Mods", "JALib"));
        Log("Download Complete JALib");
    }

    public async Task DownloadJipperResourcePack() {
        DownloadName = "JipperResourcePack Github Api";
        Log("Check JipperResourcePack Latest Version...");
        string latestVersion;
        using HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("JipperResourcePack");
        try {
            HttpResponseMessage response = await httpClient.GetAsync("https://api.github.com/repos/Jongye0l/JipperResourcePack/releases/latest");
            Progress = 97;
            response.EnsureSuccessStatusCode();
            Progress = 104;
            string json = await response.Content.ReadAsStringAsync();
            latestVersion = JObject.Parse(json)["tag_name"].Value<string>();
        } catch (Exception) {
            HttpResponseMessage response = await httpClient.GetAsync("https://api.github.com/repos/Jongye0l/JipperResourcePack/releases");
            Progress = 100;
            response.EnsureSuccessStatusCode();
            Progress = 106;
            string json = await response.Content.ReadAsStringAsync();
            latestVersion = JArray.Parse(json)[0]["tag_name"].Value<string>();
        }
        Progress = 110;
        DownloadName = "JipperResourcePack";
        DownloadProgressStart = Progress;
        DownloadProgressEnd = 160;
        Log("Downloading JipperResourcePack...");
        await Download($"https://github.com/Jongye0l/JipperResourcePack/releases/download/{latestVersion}/JipperResourcePack.zip", Path.Combine(GlobalSetting.Instance.InstallPath, "Mods", "JipperResourcePack"));
        Log("Download Complete JipperResourcePack");
    }
}