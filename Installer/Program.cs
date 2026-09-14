using System;
using System.Threading;
using System.Windows.Forms;

namespace JipperResourcePack.Installer;

static class Program {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
        using Mutex instanceMutex = new(false, @"Local\JipperResourcePack.Installer", out bool createdNew);
        if(!createdNew) return;

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new InstallerForm());
    }
}
