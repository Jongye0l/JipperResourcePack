using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JipperResourcePack.Installer.Resource;
using Newtonsoft.Json;

namespace JipperResourcePack.Installer.Screen;

public class SelectScreen : Screen {
    public RadioButton WorkInstallRadio;
    public RadioButton WorkUninstallRadio;

    public CheckBox RequirementUnityModManager;
    public CheckBox RequirementJalib;
    public CheckBox RequirementDoorstop;
    public CheckBox RequirementJipperResourcePack;

    public CheckBox[] AdditionMods;

    public List<string> ExistModsPath = [];
    public List<CheckBox> ExistMods = [];
    public CancellationTokenSource ExistModsCancellationTokenSource;

    public RequirementStatus RequirementStatus;

    public SelectScreen(Screen screen, RequirementStatus requirementStatus) {
        PrevScreen = screen;
        NextScreen = new InstallScreen(this);
        RequirementStatus = requirementStatus;
    }

    public override void OnEnter() {
        RequirementStatus.IsExistJALib = File.Exists(Path.Combine(GlobalSetting.Instance.InstallPath, "Mods", "JALib", "JALib.dll"));
        RequirementStatus.IsExistJipperResourcePack = File.Exists(Path.Combine(GlobalSetting.Instance.InstallPath, "Mods", "JipperResourcePack", "JipperResourcePack.dll"));
        TopPanelLabels[1].Font = Constants.Arial16B;
        MainPanel.SuspendLayout();
        Resources resources = Resources.Current;

        // Title
        Control control = new Label {
            Text = resources.Select_Title,
            Font = Constants.Arial16,
            AutoSize = true,
            Location = new Point(30, 62)
        };
        MainPanel.Controls.Add(control);

        // Work
        control = new Label {
            Text = resources.Select_Work,
            Font = Constants.Arial13,
            AutoSize = true,
            Location = new Point(34, 103)
        };
        MainPanel.Controls.Add(control);

        Panel panel = new() {
            Location = new Point(40, 125),
            Size = new Size(910, 22)
        };

        control = WorkInstallRadio = new RadioButton {
            Text = resources.Select_Work_Install,
            Font = Constants.Arial13,
            AutoSize = true,
            Location = new Point(0, 0),
            Checked = true
        };
        panel.Controls.Add(control);

        control = WorkUninstallRadio = new RadioButton {
            Text = resources.Select_Work_Uninstall,
            Font = Constants.Arial13,
            AutoSize = true,
            Location = new Point(80, 0)
        };
        WorkUninstallRadio.CheckedChanged += (_, _) => {
            if(!WorkUninstallRadio.Checked) return;
            // TODO: Implement this.
            MessageBox.Show("Not Implemented", Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            WorkInstallRadio.Checked = true;
        };
        panel.Controls.Add(control);

        MainPanel.Controls.Add(panel);

        // Requirement
        control = new Label {
            Text = resources.Select_Requirement,
            Font = Constants.Arial13,
            AutoSize = true,
            Location = new Point(34, 169)
        };
        MainPanel.Controls.Add(control);

        panel = new Panel {
            Location = new Point(40, 197),
            Size = new Size(910, 22)
        };

        panel.Click += RequirementPanelOnClick;

        control = RequirementUnityModManager = new CheckBox {
            Text = "UnityModManager",
            Font = Constants.Arial11,
            AutoSize = true,
            Location = new Point(0, 0),
            Checked = !RequirementStatus.IsExistUnityModManager,
            Enabled = RequirementStatus.IsExistUnityModManager
        };
        panel.Controls.Add(control);

        control = RequirementDoorstop = new CheckBox {
            Text = "Doorstop",
            Font = Constants.Arial11,
            AutoSize = true,
            Location = new Point(177, 0),
            Checked = RequirementUnityModManager.Checked || RequirementStatus.IsAssemblyInstalled || RequirementStatus.IsOldDoorStop,
            Enabled = !RequirementUnityModManager.Checked
        };
        RequirementDoorstop.CheckedChanged += RequirementDoorstopOnCheckedChanged;
        panel.Controls.Add(control);

        control = RequirementJalib = new CheckBox {
            Text = "JALib",
            Font = Constants.Arial11,
            AutoSize = true,
            Location = new Point(293, 0),
            Checked = !RequirementStatus.IsExistJALib,
            Enabled = RequirementStatus.IsExistJALib
        };
        panel.Controls.Add(control);

        control = RequirementJipperResourcePack = new CheckBox {
            Text = "JipperResourcePack",
            Font = Constants.Arial11,
            AutoSize = true,
            Location = new Point(384, 0),
            Checked = true,
            Enabled = RequirementStatus.IsExistJipperResourcePack
        };
        panel.Controls.Add(control);

        MainPanel.Controls.Add(panel);

        // Addition Mods
        control = new Label {
            Text = resources.Select_AdditionMods,
            Font = Constants.Arial13,
            AutoSize = true,
            Location = new Point(34, 240)
        };
        MainPanel.Controls.Add(control);
        MainPanel.ResumeLayout();
        EnterAsyncWork();
    }

    private async void EnterAsyncWork() {
        try {
            ExistMods.Clear();
            ExistModsPath.Clear();
            bool existMods = Directory.Exists(Path.Combine(GlobalSetting.Instance.InstallPath, "Mods"));
            ConcurrentQueue<string> modItemQueue;
            Task task;
            if(existMods) {
                ExistModsCancellationTokenSource = new CancellationTokenSource();
                modItemQueue = new ConcurrentQueue<string>();
                task = Task.Run(() => {
                    try {
                        CancellationToken token = ExistModsCancellationTokenSource.Token;
                        foreach(string file in Directory.GetDirectories(Path.Combine(GlobalSetting.Instance.InstallPath, "Mods"))) {
                            string infoLocation = Path.Combine(file, "Info.json");
                            if(!File.Exists(infoLocation)) infoLocation = Path.Combine(file, "info.json");
                            if(!File.Exists(infoLocation)) continue;

                            ModInfo modInfo = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(infoLocation));
                            modItemQueue.Enqueue(modInfo.Id + "(By " + modInfo.Author + ")");

                            if(token.IsCancellationRequested) return;
                            ExistModsPath.Add(file);
                        }
                    } finally {
                        modItemQueue.Enqueue(null);
                    }
                }, ExistModsCancellationTokenSource.Token);
            } else {
                modItemQueue = null;
                task = Task.CompletedTask;
            }

            int line = 0;
            try {
                ModData[] mods = await GlobalSetting.Instance.AdditionMods;
                line = (mods.Length + 2) / 3;
                if(line != 0) {
                    Panel panel = new() {
                        Location = new Point(40, 265),
                        Size = new Size(910, -4 + 25 * line)
                    };

                    AdditionMods = new CheckBox[mods.Length];
                    List<ModData> selectedMods = GlobalSetting.Instance.SelectedMods;
                    for(int i = 0; i < mods.Length; i++) {
                        AdditionMods[i] = new CheckBox {
                            Text = mods[i].DisplayName,
                            Font = Constants.Arial11,
                            Size = new Size(303, 25),
                            Location = new Point(i % 3 * 303, i / 3 * 25),
                            Checked = selectedMods?.Contains(mods[i]) == true
                        };
                        panel.Controls.Add(AdditionMods[i]);
                    }

                    MainPanel.Controls.Add(panel);
                }
            } catch (Exception e) {
                Console.WriteLine(e);
                MessageBox.Show(e.Message, Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Remove Mods
            if(!existMods) return;
            
            GroupBox groupBox = new() {
                Text = Resources.Current.Select_RemoveMods,
                Font = Constants.Arial13,
                Location = new Point(32, 287 + 25 * line)
            };
            groupBox.Size = new Size(920, 539 - groupBox.Location.Y);
            
            Panel modListPanel = new() {
                Location = new Point(10, 25),
                Size = groupBox.Size - new Size(11, 30),
                AutoScroll = true
            };

            while(true) {
                if(modItemQueue.TryDequeue(out string item)) {
                    if(item == null) break;
                    CheckBox checkBox = new() {
                        Text = item,
                        Font = Constants.Arial11,
                        Size = new Size(295, 25),
                        Location = new Point(ExistMods.Count % 3 * 295, ExistMods.Count / 3 * 25)
                    };
                    ExistMods.Add(checkBox);
                    modListPanel.Controls.Add(checkBox);
                } else await Task.Yield();
            }

            await task;
            
            int calY = 31 + (ExistMods.Count + 2) / 3 * 25;
            if(calY < groupBox.Size.Height) {
                groupBox.Size = groupBox.Size with {
                    Height = calY
                };
                modListPanel.Size = modListPanel.Size with {
                    Height = calY - 30
                };
            }
            
            groupBox.Controls.Add(modListPanel);
            MainPanel.Controls.Add(groupBox);
        } catch (Exception e) {
            Console.WriteLine(e);
            MessageBox.Show(e.Message, Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RequirementDoorstopOnCheckedChanged(object sender, EventArgs e) {
        if(RequirementDoorstop.Checked) return;
        if(RequirementStatus.IsAssemblyInstalled) {
            DialogResult result = MessageBox.Show(Resources.Current.Select_Requirement_Doorstop_Asm, Resources.Current.Warn, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if(result == DialogResult.No) RequirementDoorstop.Checked = true;
        } else if(RequirementStatus.IsOldDoorStop) {
            DialogResult result = MessageBox.Show(Resources.Current.Select_Requirement_Doorstop_Old, Resources.Current.Warn, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if(result == DialogResult.No) RequirementDoorstop.Checked = true;
        }
    }

    private void RequirementPanelOnClick(object sender, EventArgs e) {
        if(!RequirementUnityModManager.Enabled && RequirementUnityModManager.Bounds.Contains(((MouseEventArgs) e).Location))
            MessageBox.Show(Resources.Current.Select_Requirement_UnityModManager, Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        else if(!RequirementDoorstop.Enabled && RequirementDoorstop.Bounds.Contains(((MouseEventArgs) e).Location))
            MessageBox.Show(Resources.Current.Select_Requirement_Doorstop_Umm, Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        else if(!RequirementJalib.Enabled && RequirementJalib.Bounds.Contains(((MouseEventArgs) e).Location))
            MessageBox.Show(Resources.Current.Select_Requirement_JALib, Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        else if(!RequirementJipperResourcePack.Enabled && RequirementJipperResourcePack.Bounds.Contains(((MouseEventArgs) e).Location))
            MessageBox.Show(Resources.Current.Select_Requirement_JipperResourcePack, Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public override bool OnNext() {
        foreach(CheckBox checkBox in ExistMods) {
            if(!checkBox.Checked) continue;
            DialogResult result = MessageBox.Show(Resources.Current.Select_RemoveMods_Confirm, Resources.Current.Warn, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if(result == DialogResult.No) return false;
            break;
        }
        return true;
    }

    public override void OnLeave() {
        ExistModsCancellationTokenSource?.Cancel();
        GlobalSetting.Instance.InstallUnityModManager = RequirementUnityModManager.Checked;
        GlobalSetting.Instance.InstallDoorstop = RequirementDoorstop.Checked;
        GlobalSetting.Instance.InstallJalib = RequirementJalib.Checked;
        GlobalSetting.Instance.InstallJipperResourcePack = RequirementJipperResourcePack.Checked;
        List<ModData> selectedMods = [];
        ModData[] mods = GlobalSetting.Instance.AdditionMods.Result;
        for(int i = 0; i < AdditionMods.Length; i++)
            if(AdditionMods[i].Checked)
                selectedMods.Add(mods[i]);
        GlobalSetting.Instance.SelectedMods = selectedMods;
        
        List<string> removeRequestMods = [];
        for(int i = 0; i < ExistMods.Count; i++)
            if(ExistMods[i].Checked)
                removeRequestMods.Add(ExistModsPath[i]);
        GlobalSetting.Instance.RemoveRequestMods = removeRequestMods;
        
        TopPanelLabels[1].Font = Constants.Arial16;
    }
}