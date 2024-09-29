using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JipperResourcePack.Installer.Properties;

namespace JipperResourcePack.Installer.Screen;

public class SelectModScreen : Screen {

    public Label TitleLabel;
    public Label DescriptionLabel;
    public GroupBox ModGroup;
    public CheckBox[] Mods;
    public ProgressBar ProgressBar;

    public SelectModScreen(Screen screen) {
        PrevScreen = screen;
        NextScreen = new InstallScreen(this);
    }

    public override async void OnEnter() {
        ProgressBar = new ProgressBar {
            MarqueeAnimationSpeed = 1,
            Style = ProgressBarStyle.Marquee,
            Size = new Size(800, 20),
            Location = new Point(92, 300)
        };
        MainPanel.Controls.Add(ProgressBar);
        ProgressBar.Dispose();
        TitleLabel = new Label {
            Text = Resources.SelectMod_Title,
            Font = new Font("Arial", 16),
            AutoSize = true,
            Location = new Point(30, 8)
        };
        DescriptionLabel = new Label {
            Text = Resources.SelectMod_Description,
            Font = new Font("Arial", 13),
            AutoSize = true,
            Location = new Point(30, 70)
        };
        ModGroup = new GroupBox {
            Text = Resources.SelectMod_Mod,
            Font = new Font("Arial", 13),
            Location = new Point(60, 120),
            Size = new Size(850, 400)
        };
        TopPanel.Controls.Add(TitleLabel);
        MainPanel.Controls.Add(DescriptionLabel);
        MainPanel.Controls.Add(ModGroup);
        ModData[] mods = await GlobalSetting.Instance.AdditionMods;
        Mods = new CheckBox[mods.Length];
        List<ModData> selectedMods = GlobalSetting.Instance.SelectedMods;
        for(int i = 0; i < mods.Length; i++) {
            Mods[i] = new CheckBox {
                Text = mods[i].DisplayName,
                Font = new Font("Arial", 12),
                AutoSize = true,
                Location = new Point(30 + i % 2 * 267, 30 + i / 2 * 30),
                Checked = selectedMods?.Contains(mods[i]) == true
            };
            ModGroup.Controls.Add(Mods[i]);
        }
    }

    public override void OnLeave() {
        List<ModData> selectedMods = [];
        ModData[] mods = GlobalSetting.Instance.AdditionMods.Result;
        for(int i = 0; i < Mods.Length; i++)
            if(Mods[i].Checked) selectedMods.Add(mods[i]);
        GlobalSetting.Instance.SelectedMods = selectedMods;
    }
}