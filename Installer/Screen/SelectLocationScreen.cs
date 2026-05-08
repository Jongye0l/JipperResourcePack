using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JipperResourcePack.Installer.Resource;

namespace JipperResourcePack.Installer.Screen;

public class SelectLocationScreen : Screen {
    public TextBox LocationTextBox;
    public GroupBox NotifyBox;
    public CancellationTokenSource Cts;
    public string ErrorString;
    public RequirementStatus RequirementStatus = new();
    //public Button AdofaiFolderGuide;

    public SelectLocationScreen(Screen screen) {
        PrevScreen = screen;
        NextScreen = new SelectScreen(this, RequirementStatus);
    }

    public override void OnEnter() {
        GlobalSetting.Instance.InstallPath ??= Utility.GetAdofaiPath();
        TopPanelLabels[0].Font = new Font("Arial", 16, FontStyle.Bold);
        MainPanel.SuspendLayout();

        // Title
        Control control = new Label {
            Text = Resources.Current.SelectLocation_Title,
            Font = new Font("Arial", 16),
            AutoSize = true,
            Location = new Point(30, 62)
        };
        MainPanel.Controls.Add(control);

        // Description
        control = new Label {
            Text = Resources.Current.SelectLocation_Description,
            Font = new Font("Arial", 13),
            AutoSize = true,
            Location = new Point(32, 99)
        };
        MainPanel.Controls.Add(control);

        // Location
        control = new Label {
            Text = Resources.Current.SelectLocation_Location,
            Font = new Font("Arial", 13),
            AutoSize = true,
            Location = new Point(35, 230)
        };
        MainPanel.Controls.Add(control);

        control = LocationTextBox = new TextBox {
            Text = GlobalSetting.Instance.InstallPath,
            Location = new Point(38, 254),
            Font = new Font("Arial", 13),
            Size = new Size(770, 50)
        };
        LocationTextBox.TextChanged += (_, _) => UpdateInformation(false);
        MainPanel.Controls.Add(control);

        control = new Button {
            Text = Resources.Current.SelectLocation_Select,
            Location = new Point(818, 254),
            Font = new Font("Arial", 13),
            Size = new Size(130, 30)
        };
        control.Click += LocationSelectButton_Click;
        MainPanel.Controls.Add(control);

        control = NotifyBox = new GroupBox();
        MainPanel.Controls.Add(control);
        // AdofaiFolderGuide = new Button {
        //     Text = Resources.Current.SelectLocation_AdofaiFolderGuide,
        //     Location = new Point(30, 150),
        //     Font = new Font("Arial", 13),
        //     Size = new Size(150, 30)
        // };
        // AdofaiFolderGuide.Click += AdofaiFolderGuide_Click;
        // MainPanel.Controls.Add(AdofaiFolderGuide);
        MainPanel.ResumeLayout();
        UpdateInformation(true);
    }

    public override void OnLeave() {
        TopPanelLabels[0].Font = new Font("Arial", 16);
    }

    public override bool OnNext() {
        if(ErrorString == null) return true;
        MessageBox.Show(ErrorString, Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
    }

    public void UpdateInformation(bool init) {
        int count = 0;
        try {
            NotifyBox.SuspendLayout();
            if(Cts != null) {
                Cts.Cancel();
                NotifyBox.Controls.Clear();
            }
            Cts = new CancellationTokenSource();
            ErrorString = null;

            Resources resources = Resources.Current;
            NotifyBox.Controls.Add(new Label {
                Text = resources.SelectLocation_NotifyTitle,
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 16)
            });

            string path = LocationTextBox.Text.Replace("\\", "/");
            if(string.IsNullOrWhiteSpace(path)) {
                ErrorString = resources.SelectLocation_NoLocation;
                CreateNotify(1, resources.SelectLocation_NotifyEmpty, ref count);
                return;
            }

            if(!Directory.Exists(path)) {
                ErrorString = resources.SelectLocation_NoFolder;
                CreateNotify(1, resources.SelectLocation_NotifyFolderNotExist, ref count);
                return;
            }
            CreateNotify(0, resources.SelectLocation_NotifyFolderExist, ref count);

            if(!CheckAdofaiFolder(path)) {
                ErrorString = resources.SelectLocation_NoAdofai;
                CreateNotify(1, resources.SelectLocation_NotifyGameNotFound, ref count);
                return;
            }
            ParseAdofaiVersion(CreateNotify(0, resources.SelectLocation_NotifyGameFound, ref count), init);

            if(!File.Exists(Path.Combine(path, "A Dance of Fire and Ice_Data", "Managed", "UnityModManager", "UnityModManager.dll"))) return;
            RequirementStatus.IsExistUnityModManager = true;
            if(!File.Exists(Path.Combine(path, "winhttp.dll"))) {
                RequirementStatus.IsAssemblyInstalled = true;
                CreateNotify(2, resources.SelectLocation_NotifyUmmIsAssembly, resources.SelectLocation_NotifyUmmIsAssembly2, ref count);
                return;
            }

            FileVersionInfo doorstopVersion = FileVersionInfo.GetVersionInfo(Path.Combine(path, "winhttp.dll"));
            Version version = Version.Parse(doorstopVersion.FileVersion);
            if(version < new Version(4, 4, 0, 0)) {
                RequirementStatus.IsOldDoorStop = true;
                CreateNotify(2, resources.SelectLocation_NotifyDoorstopIsOld, resources.SelectLocation_NotifyDoorstopIsOld2, ref count);
            }
        } finally {
            int addition = 32 * count;
            NotifyBox.Location = new Point(38, 492 - addition);
            NotifyBox.Size = new Size(909, 50 + addition);
            NotifyBox.ResumeLayout();
        }
    }

    private async void ParseAdofaiVersion(Label label, bool init) {
        try {
            CancellationToken token = Cts.Token;
            label.AutoSize = true;
            int adofaiVersion;

            if(!init || GlobalSetting.Instance.AdofaiRevision == -1) {
                GlobalSetting.Instance.AdofaiRevision = -1;
                string path = GlobalSetting.Instance.InstallPath;
                string dllPath = Path.Combine(path, "A Dance of Fire and Ice_Data", "Managed", "Assembly-CSharp.dll");
                adofaiVersion = await Task.Run(() => GetAdofaiRevision(dllPath), token); // TODO: Memory Leak. add AppDomain System.
                if(token.IsCancellationRequested) return;
                GlobalSetting.Instance.AdofaiRevision = adofaiVersion;
            } else {
                adofaiVersion = GlobalSetting.Instance.AdofaiRevision;
                await Task.Yield(); // Waiting Resume to update label location
            }

            Label versionLabel = new() {
                Text = $"(r{adofaiVersion})",
                Font = new Font("Arial", 10),
                ForeColor = Color.FromArgb(109, 109, 109),
                AutoSize = true,
                Location = new Point(label.Location.X + label.Size.Width - 4, label.Location.Y + 2)
            };
            NotifyBox.Controls.Add(versionLabel);
        } catch (Exception e) {
            MessageBox.Show(e.ToString(), Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static int GetAdofaiRevision(string dllPath) {
        Assembly assembly = Assembly.Load(File.ReadAllBytes(dllPath));
        Type type = assembly.GetType("GCNS");
        FieldInfo fieldInfo = type.GetField("releaseNumber", BindingFlags.Public | BindingFlags.Static);
        return (int) fieldInfo!.GetRawConstantValue();
    }

    private static bool CheckAdofaiFolder(string path) {
        string installPath;
        if(!Directory.Exists(Path.Combine(path, "A Dance of Fire and Ice_Data"))) {
            if(!path.TrimEnd('/', '\\').EndsWith("A Dance of Fire and Ice_Data")) {
                if(!Directory.Exists(Path.Combine(path, "A Dance of Fire and Ice", "A Dance of Fire and Ice_Data"))) return false;
                installPath = Path.Combine(path, "A Dance of Fire and Ice");
            } else installPath = Directory.GetParent(path)!.FullName;
        } else installPath = path;
        if(!File.Exists(Path.Combine(installPath, "A Dance of Fire and Ice.exe"))) return false;
        GlobalSetting.Instance.InstallPath = installPath;
        return true;
    }

    private void CreateNotify(int level, string title, string subTitle, ref int count) {
        Label label = CreateNotify(level, title, ref count);
        label.AutoSize = true;
        Task.Yield().GetAwaiter().UnsafeOnCompleted(() => {
            try {
                Label label2 = new() {
                    Text = subTitle,
                    Font = new Font("Arial", 10),
                    ForeColor = Color.FromArgb(109, 109, 109),
                    AutoSize = true,
                    Location = new Point(label.Location.X + label.Size.Width - 4, label.Location.Y + 2)
                };
                NotifyBox.Controls.Add(label2);
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        });
    }

    private Label CreateNotify(int level, string title, ref int count) {
        string iconLocation = "JipperResourcePack.Installer.Resource." + level switch {
            0 => "check",
            1 => "close",
            _ => "warning"
        } + ".png";
        PictureBox icon = new() {
            Image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(iconLocation)!),
            Size = new Size(24, 24),
            Location = new Point(18, 50 + count * 32)
        };
        NotifyBox.Controls.Add(icon);
        Label label = new() {
            Text = title,
            Font = new Font("Arial", 13),
            Size = new Size(845, 20),
            Location = new Point(46, 51 + count * 32)
        };
        NotifyBox.Controls.Add(label);
        count++;
        return label;
    }

    public void LocationSelectButton_Click(object sender, EventArgs e) {
        FolderBrowserDialog folderBrowserDialog = new();
        if(folderBrowserDialog.ShowDialog() == DialogResult.OK)
            LocationTextBox.Text = GlobalSetting.Instance.InstallPath = folderBrowserDialog.SelectedPath;
    }

    //public void AdofaiFolderGuide_Click(object sender, EventArgs e) {
    //    Process.Start(new ProcessStartInfo {
    //        FileName = "https://jongyeol.kr/guide/adofai-folder",
    //        UseShellExecute = true
    //    });
    //}
}