using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using JipperResourcePack.Installer.Screen;

namespace JipperResourcePack.Installer;

public class UMMInstaller {
    public InstallScreen installScreen;
    public TaskCompletionSource<bool> tcs;
    public string[] libraryDestPaths;
    public string tempPath;
    public string gamePath;
    public string managerPath;

    public UMMInstaller(InstallScreen installScreen) {
        this.installScreen = installScreen;
        tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "JipperResourcePack");
        gamePath = Path.Combine(GlobalSetting.Instance.InstallPath, "A Dance of Fire and Ice_Data", "Managed");
        managerPath = Path.Combine(gamePath, "UnityModManager");
        libraryDestPaths = [
            Path.Combine(managerPath, "0Harmony.dll"),
            Path.Combine(managerPath, "dnlib.dll"),
            Path.Combine(managerPath, "UnityModManager.dll"),
            Path.Combine(managerPath, "UnityModManager.xml")
        ];
    }

    public Task Start() {
        tcs = new TaskCompletionSource<bool>();
        if(Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        Directory.CreateDirectory(tempPath);
        try {
            Application.ApplicationExit += RemoveDirectory;
            installScreen.Log("Downloading UnityModManager...");
            installScreen.DownloadName = "UnityModManager";
            installScreen.DownloadProgressStart = 10;
            installScreen.DownloadProgressEnd = 60;
            installScreen.Download("https://www.dropbox.com/s/wz8x8e4onjdfdbm/UnityModManager.zip?dl=1", tempPath)
                .GetAwaiter().UnsafeOnCompleted(OnDownloadComplete);
        } catch (Exception e) {
            tcs.SetException(e);
        } finally {
            RemoveDirectory(null, null);
        }
        return tcs.Task;
    }

    public void OnDownloadComplete() {
        try {
            if(!Directory.Exists(managerPath))
                Directory.CreateDirectory(managerPath);
            installScreen.Log("Backup files...");
            string doorstopPath = Path.Combine(GlobalSetting.Instance.InstallPath, "winhttp.dll");
            string doorstopConfigPath = Path.Combine(GlobalSetting.Instance.InstallPath, "doorstop_config.ini");
            installScreen.Log("Deleting files from game...");
            installScreen.Log($"  '{doorstopPath}'");
            File.Delete(doorstopPath);
            installScreen.Log($"  '{doorstopConfigPath}'");
            File.Delete(doorstopConfigPath);
            installScreen.Log("Copying files to game...");
            string filename = UnmanagedDllIs64Bit(Path.Combine(GlobalSetting.Instance.InstallPath, "A Dance of Fire and Ice.exe")) ? "winhttp_x64.dll" : "winhttp_x86.dll";
            installScreen.Log($"  '{filename}'");
            File.Copy(Path.Combine(tempPath, filename), doorstopPath);
            installScreen.Log($"  '{doorstopConfigPath}'");
            using(Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Installer.Resource.UMM.doorstop_config.ini")) {
                using Stream file = File.Create(doorstopConfigPath);
                stream.CopyTo(file);
            }
            DoactionLibraries();
            DoactionGameConfig();
            tcs.SetResult(true);
        } catch (Exception e) {
            tcs.SetException(e);
        } finally {
            RemoveDirectory(null, null);
        }
    }

    public void RemoveDirectory(object obj, EventArgs args) {
        Directory.Delete(tempPath, true);
        Application.ApplicationExit -= RemoveDirectory;
    }

    public ushort GetDllMachineType(string dllPath) {
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
            installScreen.Log("Unable to determine the bitness of " + dllPath);
            installScreen.Log(ex.ToString());
            return false;
        }
    }

    public void DoactionLibraries() {
        installScreen.Log("Copying files to game...");
        foreach(string destpath in libraryDestPaths) {
            installScreen.Log($"  {destpath}");
            File.Copy(Path.Combine(tempPath, Path.GetFileName(destpath)), destpath, true);
        }
    }

    public void DoactionGameConfig() {
        installScreen.Log("Creating configs...");
        installScreen.Log("  Config.xml");
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Installer.Resource.UMM.Config.xml");
        using Stream file = File.Create(Path.Combine(managerPath, "Config.xml"));
        stream.CopyTo(file);
    }
}