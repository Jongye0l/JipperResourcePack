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

    public Panel ContentsPanel;

    // Install
    public CheckBox RequirementUnityModManager;
    public CheckBox RequirementJalib;
    public CheckBox RequirementDoorstop;
    public CheckBox RequirementJipperResourcePack;

    public GroupBox ModListGroup;
    public Panel ModListPanel;

    public CheckBox[] AdditionMods;
    
    // Uninstall
    public RadioButton UninstallAll;
    public RadioButton UninstallManager;
    public RadioButton UninstallMod;
    
    // All
    public List<string> ExistModsPath = [];
    public List<CheckBox> ExistMods = [];
    public int ExistModHeight;
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
        RequirementStatus.ExistMods = Directory.Exists(Path.Combine(GlobalSetting.Instance.InstallPath, "Mods"));
        TopPanelLabels[1].Font = Constants.Arial16B;
        if(!GlobalSetting.Instance.SelectSaved) { 
            GlobalSetting.Instance.InstallDoorstop = !RequirementStatus.IsExistUnityModManager || RequirementStatus.IsAssemblyInstalled || RequirementStatus.IsOldDoorStop;
            GlobalSetting.Instance.InstallJipperResourcePack = true;
        }
        
        Resources resources = Resources.Current;
        MainPanel.SuspendLayout();
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
            Location = new Point(80, 0),
            Enabled = RequirementStatus.IsExistUnityModManager || RequirementStatus.ExistMods
        };
        WorkUninstallRadio.CheckedChanged += (_, _) => {
            SaveData(WorkUninstallRadio.Checked);
            UpdateUI(!WorkUninstallRadio.Checked, false);
        };
        panel.Controls.Add(control);
        
        MainPanel.Controls.Add(panel);

        panel = ContentsPanel = new Panel {
            Location = new Point(34, 169),
            Size = new Size(916, 360)
        };
        MainPanel.Controls.Add(panel);
        
        MainPanel.ResumeLayout();
        UpdateUI(!GlobalSetting.Instance.IsUninstall, true);
    }

    private void UpdateUI(bool install, bool init) {
        ContentsPanel.SuspendLayout();
        ContentsPanel.Controls.Clear();
        Resources resources = Resources.Current;

        if(install) {
            // Requirement
            Control control = new Label {
                Text = resources.Select_Requirement,
                Font = Constants.Arial13,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            ContentsPanel.Controls.Add(control);

            Panel panel = new() {
                Location = new Point(6, 28),
                Size = new Size(910, 22)
            };

            panel.Click += RequirementPanelOnClick;
            control = RequirementUnityModManager = new CheckBox {
                Text = "UnityModManager",
                Font = Constants.Arial11,
                AutoSize = true,
                Location = new Point(0, 0),
                Checked = !RequirementStatus.IsExistUnityModManager || GlobalSetting.Instance.InstallUnityModManager,
                Enabled = RequirementStatus.IsExistUnityModManager
            };
            panel.Controls.Add(control);

            control = RequirementDoorstop = new CheckBox {
                Text = "Doorstop",
                Font = Constants.Arial11,
                AutoSize = true,
                Location = new Point(177, 0),
                Checked = RequirementUnityModManager.Checked || GlobalSetting.Instance.InstallDoorstop,
                Enabled = RequirementStatus.IsExistUnityModManager
            };
            RequirementDoorstop.CheckedChanged += RequirementDoorstopOnCheckedChanged;
            panel.Controls.Add(control);

            control = RequirementJalib = new CheckBox {
                Text = "JALib",
                Font = Constants.Arial11,
                AutoSize = true,
                Location = new Point(293, 0),
                Checked = !RequirementStatus.IsExistJALib || GlobalSetting.Instance.InstallJalib,
                Enabled = RequirementStatus.IsExistJALib
            };
            panel.Controls.Add(control);

            control = RequirementJipperResourcePack = new CheckBox {
                Text = "JipperResourcePack",
                Font = Constants.Arial11,
                AutoSize = true,
                Location = new Point(384, 0),
                Checked = GlobalSetting.Instance.InstallJipperResourcePack || !RequirementStatus.IsExistJipperResourcePack,
                Enabled = RequirementStatus.IsExistJipperResourcePack
            };
            panel.Controls.Add(control);

            ContentsPanel.Controls.Add(panel);

            // Addition Mods
            control = new Label {
                Text = resources.Select_AdditionMods,
                Font = Constants.Arial13,
                AutoSize = true,
                Location = new Point(0, 71)
            };
            ContentsPanel.Controls.Add(control);
        } else {
            // Uninstall Option
            Control control = new Label {
                Text = resources.Select_UninstallOption,
                Font = Constants.Arial13,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            ContentsPanel.Controls.Add(control);

            Panel panel = new() {
                Location = new Point(6, 28),
                Size = new Size(910, 82)
            };
            
            control = UninstallAll = new RadioButton {
                Text = resources.Select_UninstallOption_All,
                Font = Constants.Arial13,
                Size = new Size(85, 22),
                Location = new Point(0, 0),
                Checked = GlobalSetting.Instance.UninstallOption == 0
            };
            panel.Controls.Add(control);

            control = new Label {
                Text = resources.Select_UninstallOption_AllDescription,
                Font = Constants.Arial11,
                ForeColor = Constants.SubColor,
                AutoSize = true,
                Location = new Point(82, 5)
            };
            panel.Controls.Add(control);
            
            control = UninstallManager = new RadioButton {
                Text = resources.Select_UninstallOption_OnlyManager,
                Font = Constants.Arial13,
                Size = new Size(143, 22),
                Location = new Point(0, 30),
                Checked = GlobalSetting.Instance.UninstallOption == 1,
                Enabled = RequirementStatus.IsExistUnityModManager
            };
            panel.Controls.Add(control);
            
            control = new Label {
                Text = resources.Select_UninstallOption_OnlyManagerDescription,
                Font = Constants.Arial11,
                ForeColor = Constants.SubColor,
                AutoSize = true,
                Location = new Point(140, 35)
            };
            panel.Controls.Add(control);

            control = UninstallMod = new RadioButton {
                Text = resources.Select_UninstallOption_OnlyMod,
                Font = Constants.Arial13,
                Size = new Size(129, 22),
                Location = new Point(0, 60),
                Checked = GlobalSetting.Instance.UninstallOption == 2,
                Enabled = RequirementStatus.ExistMods
            };
            UninstallMod.CheckedChanged += (_, _) => {
                ModListGroup.Visible = UninstallMod.Checked || WorkInstallRadio.Checked;
            };
            panel.Controls.Add(control);

            control = new Label {
                Text = resources.Select_UninstallOption_OnlyModDescription,
                Font = Constants.Arial11,
                ForeColor = Constants.SubColor,
                AutoSize = true,
                Location = new Point(126, 65)
            };
            panel.Controls.Add(control);

            ContentsPanel.Controls.Add(panel);
            ContentsPanel.Size = new Size(916, 134);
        }
        EnterAsyncWork(install, init);
        ContentsPanel.ResumeLayout();
    }

    private async void EnterAsyncWork(bool install, bool init) {
        try {
            ConcurrentQueue<string> modItemQueue;
            Task task;
            if(init && RequirementStatus.ExistMods) {
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
                            if(token.IsCancellationRequested) return;
                            ExistModsPath.Add(file);
                            modItemQueue.Enqueue(modInfo.Id + "(By " + modInfo.Author.Replace(" & ", ", ").Replace("&", ", ") + ")");
                        }
                    } finally {
                        modItemQueue.Enqueue(null);
                    }
                }, ExistModsCancellationTokenSource.Token);
            } else {
                modItemQueue = null;
                task = Task.CompletedTask;
            }

            if(install) {
                int line = 0;
                try {
                    ModData[] mods = await GlobalSetting.Instance.AdditionMods;
                    line = (mods.Length + 2) / 3;
                    if(line != 0) {
                        Panel panel = new() {
                            Location = new Point(6, 96),
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

                        ContentsPanel.Controls.Add(panel);
                    }
                } catch (Exception e) {
                    Console.WriteLine(e);
                    MessageBox.Show(e.ToString(), Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                ContentsPanel.Size = new Size(916, 114 + 25 * line);
            }

            // Remove Mods
            if(!RequirementStatus.ExistMods) return;

            if(init) {
                ModListGroup = new GroupBox {
                    Text = Resources.Current.Select_RemoveMods,
                    Font = Constants.Arial13,
                    Location = new Point(32, 169 + ContentsPanel.Size.Height)
                };
                ModListGroup.Size = new Size(920, 539 - ModListGroup.Location.Y);
            
                ModListPanel = new Panel {
                    Location = new Point(10, 25),
                    Size = ModListGroup.Size - new Size(11, 30),
                    AutoScroll = true
                };

                List<string> removeMods = GlobalSetting.Instance.RemoveRequestMods;

                while(true) {
                    if(modItemQueue!.TryDequeue(out string item)) {
                        if(item == null) break;
                        CheckBox checkBox = new() {
                            Text = item,
                            Font = Constants.Arial11,
                            Size = new Size(295, 25),
                            Location = new Point(ExistMods.Count % 3 * 295, ExistMods.Count / 3 * 25),
                            Checked = removeMods?.Contains(ExistModsPath[ExistMods.Count]) == true
                        };
                        ExistMods.Add(checkBox);
                        ModListPanel.Controls.Add(checkBox);
                    } else await Task.Yield();
                }

                await task;
            
                int calY = 31 + (ExistMods.Count + 2) / 3 * 25;
                ExistModHeight = calY;
                if(calY < ModListGroup.Size.Height) {
                    ModListGroup.Size = ModListGroup.Size with {
                        Height = calY
                    };
                    ModListPanel.Size = ModListPanel.Size with {
                        Height = calY - 30
                    };
                }
            
                ModListGroup.Controls.Add(ModListPanel);
                MainPanel.Controls.Add(ModListGroup);
            } else {
                ModListGroup.Visible = install || UninstallMod.Checked;
                ModListGroup.Location = new Point(32, 169 + ContentsPanel.Size.Height);
                int calY = 539 - ModListGroup.Location.Y;
                ModListGroup.Size = new Size(920, Math.Min(calY, ExistModHeight));

                ModListPanel.Size = ModListGroup.Size - new Size(11, 30);
            }
        } catch (Exception e) {
            Console.WriteLine(e);
            MessageBox.Show(e.ToString(), Resources.Current.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveData(bool isInstall) {
        if(isInstall) {
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
        } else GlobalSetting.Instance.UninstallOption = UninstallAll.Checked ? 0 : UninstallManager.Checked ? 1 : 2;
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
        if(WorkUninstallRadio.Checked) {
            if(UninstallAll.Checked) {
                DialogResult result = MessageBox.Show(Resources.Current.Select_RemoveMods_Confirm, Resources.Current.Warn, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                return result != DialogResult.No;
            }
            if(UninstallManager.Checked) return true;
        }
        foreach(CheckBox checkBox in ExistMods) {
            if(!checkBox.Checked) continue;
            DialogResult result = MessageBox.Show(Resources.Current.Select_RemoveMods_Confirm, Resources.Current.Warn, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            return result != DialogResult.No;
        }
        return true;
    }

    public override void OnLeave() {
        ExistModsCancellationTokenSource?.Cancel();
        
        SaveData(WorkInstallRadio.Checked);
        List<string> removeRequestMods = [];
        for(int i = 0; i < ExistMods.Count; i++)
            if(ExistMods[i].Checked)
                removeRequestMods.Add(ExistModsPath[i]);

        GlobalSetting.Instance.RemoveRequestMods = removeRequestMods;
        GlobalSetting.Instance.IsUninstall = WorkUninstallRadio.Checked;
        GlobalSetting.Instance.SelectSaved = true;
        ExistModsPath.Clear();
        ExistMods.Clear();
        
        TopPanelLabels[1].Font = Constants.Arial16;
    }
}