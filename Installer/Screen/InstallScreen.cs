using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JipperResourcePack.Installer.Properties;
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
    public bool complete = false;
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
            Text = Resources.Install_Install,
            Font = new Font("Arial", 16),
            AutoSize = true,
            Location = new Point(30, 8)
        };
        ProgressBar = new ProgressBar {
            Minimum = 0,
            Maximum = 220 + 50 * GlobalSetting.Instance.SelectedMods.Count,
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
            await UnityModManagerCheck();
            Progress = 130;
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

    public async Task UnityModManagerCheck() {
        string path = GlobalSetting.Instance.InstallPath;
        Log("Founding UnityModManager...");
        if(Directory.Exists(Path.Combine(path, "A Dance of Fire and Ice_Data", "Managed", "UnityModManager"))) {
            Progress = 120;
            Log("UnityModManager already exists.");
            return;
        }
        Progress = 10;
        Log("UnityModManager not found.");
        await DownloadUMM();
    }

    public async Task DownloadUMM() {
        Log("Installing UnityModManager...");
        string ummPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "JipperResourcePack");
        if(Directory.Exists(ummPath)) Directory.Delete(ummPath, true);
        try {
            Application.ApplicationExit += RemoveDirectory;
            DownloadName = "UnityModManager";
            DownloadProgressStart = 10;
            DownloadProgressEnd = 60;
            await Download("https://www.dropbox.com/s/wz8x8e4onjdfdbm/UnityModManager.zip?dl=1", ummPath);
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UnityModManagerNet", "Params.xml"), $"""
                 <?xml version="1.0" encoding="utf-8"?>
                 <Param xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                   <LastSelectedGame>A Dance of Fire and Ice</LastSelectedGame>
                   <GameParams>
                     <GameParam Name="A Dance of Fire and Ice">
                       <Path>{GlobalSetting.Instance.InstallPath}</Path>
                       <InstallType>Assembly</InstallType>
                     </GameParam>
                   </GameParams>
                 </Param>
                 """);
            string installerPath = Path.Combine(ummPath, "UMMAutoInstaller.exe");
            DownloadName = "UnityModManager AutoInstaller";
            DownloadProgressStart = 60;
            DownloadProgressEnd = 110;
            await InstallerForm.WebClient.DownloadFileTaskAsync("https://github.com/Jongye0l/JipperResourcePack/raw/main/UMMAutoInstaller/UMMAutoInstaller.exe", installerPath);
            await ApplyUMM(installerPath);
            Progress = 120;
            Log("UnityModManager installed. Deleting temporary files...");
        } finally {
            Directory.Delete(ummPath, true);
            Application.ApplicationExit -= RemoveDirectory;
        }
        return;

        void RemoveDirectory(object sender, EventArgs e) {
            Directory.Delete(ummPath, true);
        }
    }

    public void Log(string text) => LogQueue.Enqueue(text);

    public async Task LogListner() {
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
                using FileStream fileStream = new(entryPath, FileMode.Create);
                entry.Open().CopyTo(fileStream);
            }
        }
    }

    public Task ApplyUMM(string path) {
        return Task.Run(() => {
            Log("Running UnityModManager AutoInstaller...");
            Process ummProcess = new();
            ProcessStartInfo ummStartInfo = ummProcess.StartInfo;
            ummStartInfo.FileName = path;
            ummStartInfo.Arguments = Process.GetCurrentProcess().Id.ToString();
            ummStartInfo.UseShellExecute = true;
            ummProcess.Start();
            ummProcess.WaitForExit();
        });
    }

    private void WebClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
        Progress = DownloadProgressStart + (int)((DownloadProgressEnd - DownloadProgressStart) * e.ProgressPercentage / 100f);
        Log($"Downloading {DownloadName}... {e.ProgressPercentage}%");
    }

    public async Task DownloadJALib() {
        DownloadName = "JALib Github Api";
        DownloadProgressStart = 130;
        DownloadProgressEnd = 150;
        Log("Check JALib Latest Version...");
        string json = await InstallerForm.WebClient.DownloadStringTaskAsync("https://api.github.com/repos/Jongye0l/JALib/releases/latest");
        string latestVersion = JObject.Parse(json)["tag_name"].Value<string>();
        DownloadName = "JALib";
        DownloadProgressStart = 150;
        DownloadProgressEnd = 200;
        Log("Downloading JALib...");
        await Download($"https://github.com/Jongye0l/JALib/releases/download/{latestVersion}/JALib.zip", Path.Combine(GlobalSetting.Instance.InstallPath, "Mods", "JALib"));
        Log("Download Complete JALib");
    }

    public async Task DownloadJipperResourcePack() {
        DownloadName = "JipperResourcePack Github Api";
        DownloadProgressStart = 200;
        DownloadProgressEnd = 220;
        Log("Check JipperResourcePack Latest Version...");
        string json = await InstallerForm.WebClient.DownloadStringTaskAsync("https://api.github.com/repos/Jongye0l/JipperResourcePack/releases/latest");
        string latestVersion = JObject.Parse(json)["tag_name"].Value<string>();
        DownloadName = "JipperResourcePack";
        DownloadProgressStart = 220;
        DownloadProgressEnd = 270;
        Log("Downloading JipperResourcePack...");
        await Download($"https://github.com/Jongye0l/JipperResourcePack/releases/download/{latestVersion}/JipperResourcePack.zip", Path.Combine(GlobalSetting.Instance.InstallPath, "Mods", "JipperResourcePack"));
        Log("Download Complete JipperResourcePack");
    }
}