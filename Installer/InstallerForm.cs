using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using JipperResourcePack.Installer.Resource;
using JipperResourcePack.Installer.Screen;
using Newtonsoft.Json;

namespace JipperResourcePack.Installer;

public partial class InstallerForm : Form {
    public static HttpClient HttpClient;

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
        //new MainScreen().Enter();
        new FinishScreen(null).Enter();
        HttpClient = new HttpClient();
        GlobalSetting.Instance.AdditionMods = GetMods();
    }


    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetForegroundWindow(IntPtr hwnd);

    private static bool CheckAlreadyRunning() {
        int id = Process.GetCurrentProcess().Id;
        foreach (Process process in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName)) {
            if(process.Id == id) continue;
            SetForegroundWindow(process.MainWindowHandle);
            return true;
        }
        return false;
    }

    public override void ResetText() {
        Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("JipperResourcePack.Installer.Resource.JipperProfile.ico")!);
        Text = Resources.Current.Title;
        Cancel.Text = Resources.Current.Cancel;
        Prev.Text = Resources.Current.Previous;
        Next.Text = Resources.Current.Next;
        VersionLabel.Text = Application.ProductVersion;
        TopPanel.Controls.Clear();
        Label oldLabel = null;
        Label[] labels = new Label[4];
        for(int i = 0; i < 7; i++) {
            Label label = new() {
                Text = i switch {
                    0 => Resources.Current.Title1,
                    2 => Resources.Current.Title2,
                    4 => Resources.Current.Title3,
                    6 => Resources.Current.Title4,
                    _ => "→"
                },
                AutoSize = true,
                Font = Constants.Arial16,
                Location = oldLabel == null ? new Point(32, 9) : new Point(oldLabel.Location.X + oldLabel.Size.Width, 9)
            };
            if((i & 1) == 0) labels[i >> 1] = label;
            TopPanel.Controls.Add(label);
            oldLabel = label;
        }
        Screen.Screen.TopPanelLabels = labels;
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
        string responseData = await HttpClient.GetStringAsync("https://github.com/Jongye0l/JipperResourcePack/raw/main/Installer/mods.json");
        return JsonConvert.DeserializeObject<ModData[]>(responseData);
    }
}