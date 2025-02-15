using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using JipperResourcePack.Installer.Screen;

namespace JipperResourcePack.Installer;

public class UMMInstaller {
    public InstallScreen installScreen;
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
            Path.Combine(managerPath, "UnityModManager.xml"),
            Path.Combine(managerPath, "Config.xml"),
        ];
    }

    public void Start() {
        if(Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        try {
            Application.ApplicationExit += RemoveDirectory;
            installScreen.Log("=======================================");
            installScreen.Log("Create Directories...");
            Directory.CreateDirectory(tempPath);
            if(!Directory.Exists(managerPath))
                Directory.CreateDirectory(managerPath);
            installScreen.Log("Backup files...");
            string doorstopPath = Path.Combine(GlobalSetting.Instance.InstallPath, "winhttp.dll");
            string doorstopConfigPath = Path.Combine(GlobalSetting.Instance.InstallPath, "doorstop_config.ini");
            MakeBackup("winhttp.dll", doorstopPath);
            MakeBackup("doorstop_config.ini", doorstopConfigPath);
            foreach(string destPath in libraryDestPaths) MakeBackup(Path.GetFileName(destPath), destPath);
            MakeBackup(Path.Combine("UnityModManager", "Config.xml"));
            installScreen.Log("Deleting files from game...");
            installScreen.Log($"  '{doorstopPath}'");
            File.Delete(doorstopPath);
            installScreen.Log($"  '{doorstopConfigPath}'");
            File.Delete(doorstopConfigPath);
            installScreen.Log("Copying files to game...");
            string filename = UnmanagedDllIs64Bit(Path.Combine(GlobalSetting.Instance.InstallPath, "A Dance of Fire and Ice.exe")) ? "winhttp_x64.dll" : "winhttp_x86.dll";
            installScreen.Log($"  '{filename}'");
            using(Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Installer.Resource.UMM." + filename)) {
                using Stream file = File.Create(doorstopPath);
                stream.CopyTo(file);
            }
            installScreen.Log($"  '{doorstopConfigPath}'");
            string relativeManagerAssemblyPath = Path.Combine("A Dance of Fire and Ice_Data", "Managed", "UnityModManager", "UnityModManager.dll");
            File.WriteAllText(doorstopConfigPath, "[General]" + Environment.NewLine + "enabled = true" + Environment.NewLine + "target_assembly = " + relativeManagerAssemblyPath);
            DoactionLibraries();
        } finally {
            RemoveDirectory(null, null);
        }
    }

    public void MakeBackup(string fileName, string path = null) {
        try {
            path ??= Path.Combine(gamePath, fileName);
            if(File.Exists(path)) File.Copy(path, Path.Combine(tempPath, fileName));
        } catch (Exception e) {
            installScreen.Log(e.ToString());
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
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Installer.Resource.UMM." + Path.GetFileName(destpath));
            using Stream file = File.Create(destpath);
            stream.CopyTo(file);
        }
    }
}