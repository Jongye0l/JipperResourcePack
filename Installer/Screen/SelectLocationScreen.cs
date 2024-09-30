using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using JipperResourcePack.Installer.Properties;
using JipperResourcePack.Installer.Resource;

namespace JipperResourcePack.Installer.Screen;

public class SelectLocationScreen : Screen {
    public Label TitleLabel;
    public Label DescriptionLabel;
    public GroupBox LocationGroup;
    public TextBox LocationTextBox;
    public Button LocationSelectButton;
    //public Button AdofaiFolderGuide;

    public SelectLocationScreen(Screen screen) {
        PrevScreen = screen;
        NextScreen = new SelectModScreen(this);
    }

    public override void OnEnter() {
        GlobalSetting.Instance.InstallPath ??= Utility.GetAdofaiPath();
        TitleLabel = new Label {
            Text = Resources.Current.SelectLocation_Title,
            Font = new Font("Arial", 16),
            AutoSize = true,
            Location = new Point(30, 8)
        };
        DescriptionLabel = new Label {
            Text = Resources.Current.SelectLocation_Description,
            Font = new Font("Arial", 13),
            AutoSize = true,
            Location = new Point(30, 65)
        };
        LocationGroup = new GroupBox {
            Text = Resources.Current.SelectLocation_Location,
            Font = new Font("Arial", 13),
            Location = new Point(60, 360),
            Size = new Size(850, 80)
        };
        LocationTextBox = new TextBox {
            Text = GlobalSetting.Instance.InstallPath,
            Location = new Point(10, 40),
            Font = new Font("Arial", 13),
            Size = new Size(690, 50)
        };
        LocationSelectButton = new Button {
            Text = Resources.Current.SelectLocation_Select,
            Location = new Point(714, 40),
            Font = new Font("Arial", 13),
            Size = new Size(130, 30)
        };
        LocationSelectButton.Click += LocationSelectButton_Click;
        //AdofaiFolderGuide = new Button {
        //    Text = Resources.Current.SelectLocation_AdofaiFolderGuide,
        //    Location = new Point(30, 150),
        //    Font = new Font("Arial", 13),
        //    Size = new Size(150, 30)
        //};
        //AdofaiFolderGuide.Click += AdofaiFolderGuide_Click;
        TopPanel.Controls.Add(TitleLabel);
        MainPanel.Controls.Add(DescriptionLabel);
        MainPanel.Controls.Add(LocationGroup);
        LocationGroup.Controls.Add(LocationTextBox);
        LocationGroup.Controls.Add(LocationSelectButton);
        //MainPanel.Controls.Add(AdofaiFolderGuide);
    }

    public override void OnLeave() {
    }

    public override bool OnNext() {
        GlobalSetting.Instance.InstallPath = LocationTextBox.Text;
        string path = GlobalSetting.Instance.InstallPath.Replace("\\", "/");
        if(string.IsNullOrEmpty(path)) {
            MessageBox.Show(Resources.Current.SelectLocation_NoLocation, Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        if(!Directory.Exists(path)) {
            MessageBox.Show(Resources.Current.SelectLocation_NoFolder, Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        if(!Directory.Exists(Path.Combine(path, "A Dance of Fire and Ice_Data"))) {
            if(!path.EndsWith("A Dance of Fire and Ice/A Dance of Fire and Ice_Data")) {
                if(!Directory.Exists(Path.Combine(path, "A Dance of Fire and Ice", "A Dance of Fire and Ice_Data"))) {
                    MessageBox.Show(Resources.Current.SelectLocation_NoAdofai, Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                GlobalSetting.Instance.InstallPath = Path.Combine(path, "A Dance of Fire and Ice");
            }
            GlobalSetting.Instance.InstallPath = Directory.GetParent(path).FullName;
        }
        return true;
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