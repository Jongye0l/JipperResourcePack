using System;
using System.IO;
using Microsoft.Win32;

namespace JipperResourcePack.Installer;

public class Utility {
    public static string GetAdofaiPath() {
        string steamPath;
        using(RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\" + (Environment.Is64BitOperatingSystem ? "Wow6432Node\\" : "") + "Valve\\Steam"))
            steamPath = key?.GetValue("InstallPath") as string;
        if(steamPath != null) {
            string path = Path.Combine(steamPath, "steamapps", "common", "A Dance of Fire and Ice");
            if(Directory.Exists(path)) return path;
        }
        for(char c = 'A'; c <= 'Z'; c++) {
            string path = c + ":/SteamLibrary/steamapps/common/A Dance of Fire and Ice";
            if(Directory.Exists(path)) return path;
        }
        return null;
    }
}