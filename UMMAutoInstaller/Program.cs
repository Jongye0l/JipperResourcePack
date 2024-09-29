using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UMMAutoInstaller {
    class Program {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void Main(string[] args) {
            int id = int.Parse(args[0]);
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "JipperResourcePack", "UnityModManagerInstaller", "Console.exe");
            Process ummProcess = new();
            ProcessStartInfo ummStartInfo = ummProcess.StartInfo;
            ummStartInfo.FileName = path;
            ummStartInfo.UseShellExecute = false;
            ummStartInfo.RedirectStandardOutput = true;
            ummStartInfo.CreateNoWindow = false;
            ummProcess.Start();
            using StreamReader reader = ummProcess.StandardOutput;
            SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
            SendKeys.SendWait("{Enter}I{Enter}{Enter}{Enter}{Enter}");
            ShowWindow(Process.GetCurrentProcess().MainWindowHandle, 2);
            SetForegroundWindow(Process.GetProcessById(id).MainWindowHandle);
            bool install = true;
            while(!ummProcess.HasExited) {
                string line = reader.ReadLine();
                if(line == null) continue;
                if(line.Trim().EndsWith("I. Install")) install = true;
                else if(line.Trim().EndsWith("D. Delete")) install = false;
                else if(line.Trim().EndsWith("P. Path") && !install) break;
            }
            if(!ummProcess.HasExited) ummProcess.Kill();
            ummProcess.WaitForExit();
        }
    }
}