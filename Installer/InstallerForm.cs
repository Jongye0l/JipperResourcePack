using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JipperResourcePack.Installer.Resource;
using JipperResourcePack.Installer.Screen;
using Newtonsoft.Json;

namespace JipperResourcePack.Installer;

public partial class InstallerForm : Form {

    public static WebClient WebClient;

    public InstallerForm() {
        InitializeComponent();
        Load += InstallerForm_Load;
    }

    private void InstallerForm_Load(object obj, EventArgs args) {
        try {
            if(CheckAlreadyRunning()) {
                Close();
                return;
            }
        } catch (Exception) {
            // ignored
        }
        ResetText();
        SetupScreenData();
        new MainScreen().Enter();
        WebClient = new WebClient();
        WebClient.Encoding = Encoding.UTF8;
        GlobalSetting.Instance.AdditionMods = GetMods();
    }


    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetForegroundWindow(IntPtr hwnd);

    private bool CheckAlreadyRunning() {
        int id = Process.GetCurrentProcess().Id;
        foreach (Process process in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName)) {
            if(process.Id == id) continue;
            SetForegroundWindow(process.MainWindowHandle);
            return true;
        }
        return false;
    }

    public override void ResetText() {
        Text = Resources.Current.Title;
        Cancel.Text = Resources.Current.Cancel;
        Prev.Text = Resources.Current.Previous;
        Next.Text = Resources.Current.Next;
    }

    public void SetupScreenData() {
        Screen.Screen.MainPanel = MainPanel;
        Screen.Screen.TopPanel = TopPanel;
        Screen.Screen.UnderPanel = UnderPanel;
        Screen.Screen.PrevButton = Prev;
        Screen.Screen.NextButton = Next;
        Screen.Screen.CancelButton = Cancel;
        Screen.Screen.SetupButton();
    }

    public async Task<ModData[]> GetMods() {
        string responseData = await WebClient.DownloadStringTaskAsync("https://github.com/Jongye0l/JipperResourcePack/raw/main/Installer/mods.json");
        return JsonConvert.DeserializeObject<ModData[]>(responseData);
    }
}