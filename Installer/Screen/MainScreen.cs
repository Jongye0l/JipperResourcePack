using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Windows.Forms;
using JipperResourcePack.Installer.Properties;

namespace JipperResourcePack.Installer.Screen;

public class MainScreen : Screen {
    public Label TitleLabel;
    public Label DescriptionLabel;
    public PictureBox PictureBox;

    public MainScreen() {
        NextScreen = new SelectLocationScreen(this);
        UpPanelVisible = false;
    }

    public override void OnEnter() {
        TitleLabel = new Label {
            Text = Resources.MainScreen_Title,
            Font = new Font("Arial", 22, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(420, 50)
        };
        DescriptionLabel = new Label {
            Text = Resources.MainScreen_Description,
            Font = new Font("Arial", 15),
            AutoSize = true,
            Location = new Point(420, 150)
        };
        PictureBox = new PictureBox {
            Image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Installer.Resource.JipperResourceSide.png")),
            Size = new Size(403, 561)
        };
        MainPanel.Controls.Add(TitleLabel);
        MainPanel.Controls.Add(DescriptionLabel);
        MainPanel.Controls.Add(PictureBox);
    }

    public override void OnLeave() {
    }

    public override bool OnNext() {
        if(IsNetworkAvailable()) return true;
        MessageBox.Show(Resources.MainScreen_NoInternet, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
    }

    public static bool IsNetworkAvailable() {
        return NetworkInterface.GetAllNetworkInterfaces().Any(ni => ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                                                    ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel);
    }
}