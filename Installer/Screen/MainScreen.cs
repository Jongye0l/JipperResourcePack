using System;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JipperResourcePack.Installer.Resource;

namespace JipperResourcePack.Installer.Screen;

public class MainScreen : Screen {
    public MainScreen() {
        NextScreen = new SelectLocationScreen(this);
        UpPanelVisible = false;
    }

    public override void OnEnter() {
        Font descriptionFont = new("Arial", 12);
        MainPanel.Controls.AddRange([
            // JipperResourceSide
            new PictureBox {
                Image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("JipperResourcePack.Installer.Resource.JipperResourceSide.png")!),
                Size = new Size(403, 561)
            },
            
            // Title
            new Label {
                Text = Resources.Current.MainScreen_Title,
                Font = new Font("Arial", 24, FontStyle.Bold),
                Size = new Size(392, 32),
                Location = new Point(435, 48)
            },
            
            // Descriptions
            new Label {
                Text = Resources.Current.MainScreen_Description1,
                Font = descriptionFont,
                Size = new Size(517, 18),
                Location = new Point(435, 140)
            },
            new Label {
                Text = Resources.Current.MainScreen_Description2,
                Font = descriptionFont,
                Size = new Size(517, 18),
                Location = new Point(435, 166)
            },
            new Label {
                Text = Resources.Current.MainScreen_Description3,
                Font = descriptionFont,
                Size = new Size(517, 18),
                Location = new Point(435, 192)
            },
            new Label {
                Text = Resources.Current.MainScreen_Description4,
                Font = descriptionFont,
                Size = new Size(517, 36),
                Location = new Point(435, 218)
            },
            new Label {
                Text = Resources.Current.MainScreen_Description5,
                Font = descriptionFont,
                Size = new Size(517, 18),
                Location = new Point(435, 260)
            },
            new Label {
                Text = Resources.Current.MainScreen_Description6,
                Font = descriptionFont,
                Size = new Size(517, 18),
                Location = new Point(435, 286)
            },
            new Label {
                Text = Resources.Current.MainScreen_Description7,
                Font = descriptionFont,
                Size = new Size(517, 18),
                Location = new Point(435, 312)
            },
            
            // Bug Report
            new Label {
                Text = Resources.Current.MainScreen_BugReport,
                Font = descriptionFont,
                Size = new Size(64, 18),
                Location = new Point(435, 444)
            },
            new LinkedImageButton("https://discord.gg/qTbnPhY7YA", "JipperResourcePack.Installer.Resource.Discord-Symbol-Blurple.png") {
                Size = new Size(32, 24),
                Location = new Point(440, 474)
            },
            new LinkedImageButton("https://github.com/Jongye0l/JipperResourcePack/issues", "JipperResourcePack.Installer.Resource.GitHub_Invertocat_Black.png") {
                Size = new Size(32, 31),
                Location = new Point(480, 470)
            },
            new LinkedImageButton("mailto:bcpjy1233@gmail.com", "JipperResourcePack.Installer.Resource.mail_24dp_E3E3E3_FILL0_wght400_GRAD0_opsz24.png") {
                Size = new Size(32, 32),
                Location = new Point(520, 470)
            },
            
            // Donate
            new Label {
                Text = Resources.Current.MainScreen_Donate,
                Font = descriptionFont,
                Size = new Size(34, 16),
                Location = new Point(600, 444)
            },
            new LinkedImageButton(null, "JipperResourcePack.Installer.Resource.payment_icon_yellow_small.png") {
                Size = new Size(64, 27),
                Location = new Point(605, 470)
            },
            new LinkedImageButton("https://ko-fi.com/jongyeol", "JipperResourcePack.Installer.Resource.kofi_logo.png") {
                Size = new Size(87, 24),
                Location = new Point(677, 470)
            }
        ]);
    }

    public override void OnLeave() {
    }

    public override bool OnNext() {
        if(IsNetworkAvailable()) return true;
        MessageBox.Show(Resources.Current.MainScreen_NoInternet, Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
    }

    public static bool IsNetworkAvailable() {
        return NetworkInterface.GetAllNetworkInterfaces().Any(ni => ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                                                    ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel);
    }

    private class LinkedImageButton : PictureBox {
        public string Url;
        public LinkedImageButton(string url, string imageUrl) {
            Url = url;
            Image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(imageUrl)!);
            Cursor = Cursors.Hand;
            Click += OnClick;
        }
        private void OnClick(object sender, EventArgs e) {
            if(Url == null) {
                using KakaoPayQrForm form = new();
                form.ShowDialog();
            } else {
                try {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
                        FileName = Url,
                        UseShellExecute = true
                    }); 
                } catch (Exception) {
                    MessageBox.Show(Resources.Current.MainScreen_OpenBrowserFailed, Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // copy clipboard url
                    Clipboard.SetText(Url);
                }
            }
        }
    }

    
    private class KakaoPayQrForm : Form {
        private const int ImageWidth = 540;
        private const int ImageHeight = 693;
        
        public KakaoPayQrForm() {
            int widthOffset = Width - ClientSize.Width;
            int heightOffset = Height - ClientSize.Height;
            PictureBox qrPictureBox = new() {
                Location = new Point(0, 0),
                Size = new Size(ImageWidth, ImageHeight),
                Image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("JipperResourcePack.Installer.Resource.KakaoPay_QR.png")!),
            };
            Size = new Size(ImageWidth + widthOffset, ImageHeight + heightOffset);
            Controls.Add(qrPictureBox);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            Name = nameof(KakaoPayQrForm);
            Text = Resources.Current.MainScreen_KakaoPayQrForm;
        }
    }
}