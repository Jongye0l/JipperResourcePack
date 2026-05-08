using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using JipperResourcePack.Installer.Resource;

namespace JipperResourcePack.Installer.Screen;

public class FinishScreen : Screen {
    public CheckBox CheckBox;
    public string Folder;

    public FinishScreen(Screen screen) {
        PrevScreen = screen;
        Cancelable = false;
        Prevable = false;
        UpPanelVisible = false;
    }

    public override void OnEnter() {
        NextButton.Text = Resources.Current.FinishScreen_Finish;
        Exception exception = ((InstallScreen) PrevScreen)?.Exception;
        MainPanel.SuspendLayout();
        MainPanel.Controls.AddRange([
            // JipperResourceSide
            new PictureBox {
                Image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("JipperResourcePack.Installer.Resource.JipperResourceSide.png")!),
                Size = new Size(403, 561)
            },
            
            // Title
            new Label {
                Text = exception != null ? Resources.Current.FinishScreen_Title_Error :
                       GlobalSetting.Instance.IsUninstall ? Resources.Current.FinishScreen_Title_Uninstall : Resources.Current.FinishScreen_Title_Install,
                Font = Constants.Arial24B,
                AutoSize = true,
                Location = new Point(430, 48)
            },
            
            // Description
            new Label {
                Text = exception != null ? Resources.Current.FinishScreen_Description_Error :
                           GlobalSetting.Instance.IsUninstall ? Resources.Current.FinishScreen_Description_Uninstall : Resources.Current.FinishScreen_Description_Install,
                Font = Constants.Arial12,
                AutoSize = true,
                Location = new Point(435, 128)
            },
            
            // Bug Report
            new Label {
                Text = Resources.Current.MainScreen_BugReport,
                Font = Constants.Arial12,
                Size = new Size(64, 18),
                Location = new Point(440, 444)
            },
            new LinkedImageButton("https://discord.gg/qTbnPhY7YA", "JipperResourcePack.Installer.Resource.Discord-Symbol-Blurple.png") {
                Size = new Size(32, 24),
                Location = new Point(445, 474)
            },
            new LinkedImageButton("https://github.com/Jongye0l/JipperResourcePack/issues", "JipperResourcePack.Installer.Resource.GitHub_Invertocat_Black.png") {
                Size = new Size(32, 31),
                Location = new Point(485, 470)
            },
            new LinkedImageButton("mailto:bcpjy1233@gmail.com", "JipperResourcePack.Installer.Resource.mail.png") {
                Size = new Size(32, 32),
                Location = new Point(525, 470)
            },
            
            // Donate
            new Label {
                Text = Resources.Current.MainScreen_Donate,
                Font = Constants.Arial12,
                Size = new Size(34, 16),
                Location = new Point(605, 444)
            },
            new LinkedImageButton(null, "JipperResourcePack.Installer.Resource.payment_icon_yellow_small.png") {
                Size = new Size(64, 27),
                Location = new Point(610, 470)
            },
            new LinkedImageButton("https://ko-fi.com/jongyeol", "JipperResourcePack.Installer.Resource.kofi_logo.png") {
                Size = new Size(87, 24),
                Location = new Point(682, 470)
            }
        ]);
        if(exception == null) {
            CheckBox = new CheckBox {
                Text = Resources.Current.FinishScreen_RunAdofai,
                Font = Constants.Arial12,
                AutoSize = true,
                Location = new Point(440, 216),
                Checked = true
            };
            MainPanel.Controls.Add(CheckBox);
        } else {
            Button checkLog = new() {
                Text = Resources.Current.FinishScreen_CheckLog,
                Font = Constants.Arial12,
                AutoSize = true,
                Location = new Point(440, 216)
            };
            checkLog.Click += CheckLog_Click;
            MainPanel.Controls.Add(checkLog);
        }
        MainPanel.ResumeLayout();
    }

    private void CheckLog_Click(object sender, EventArgs e) {
        string logPath;
        if(Folder == null) {
            Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "JipperResourcePack");
            Directory.CreateDirectory(Folder);
            try {
                Application.ApplicationExit += DeleteFolder;
                logPath = Path.Combine(Folder, "log.txt");
                StringBuilder builder = new();
                InstallScreen installScreen = (InstallScreen) PrevScreen;
                foreach(Control control in installScreen.LogPanel.Controls) builder.AppendLine(control.Text);
                builder.Append(installScreen.Exception);
                File.WriteAllText(logPath, builder.ToString());
            } catch {
                DeleteFolder(null, null);
                throw;
            }
        } else logPath = Path.Combine(Folder, "log.txt");
        Process.Start(logPath);
    }

    private void DeleteFolder(object obj, EventArgs args) {
        Directory.Delete(Folder, true);
        Application.ApplicationExit -= DeleteFolder;
    }

    public override void OnLeave() {
        if(CheckBox is { Checked: true }) {
            if(GlobalSetting.Instance.InstallPath == Utility.GetAdofaiPath()) Process.Start("steam://rungameid/977950");
            else Process.Start(new ProcessStartInfo {
                FileName = Path.Combine(GlobalSetting.Instance.InstallPath, "A Dance of Fire and Ice.exe"),
                WorkingDirectory = GlobalSetting.Instance.InstallPath
            });
        }
        if(Folder != null) DeleteFolder(null, null);
    }
}