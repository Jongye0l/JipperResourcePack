using System.Windows.Forms;

namespace JipperResourcePack.Installer.Screen;

public abstract class Screen {
    public static Screen CurrentScreen;
    public Screen PrevScreen;
    public Screen NextScreen;
    public bool Cancelable = true;
    public bool Prevable = true;
    public bool Nextable = true;
    public bool UpPanelVisible = true;
    public static Panel MainPanel;
    public static Panel TopPanel;
    public static Panel UnderPanel;
    public static Button PrevButton;
    public static Button NextButton;
    public static Button CancelButton;

    public abstract void OnEnter();
    public abstract void OnLeave();
    public virtual bool OnNext() => true;

    public void Enter() {
        CurrentScreen?.OnLeave();
        MainPanel.Controls.Clear();
        TopPanel.Controls.Clear();
        TopPanel.Visible = UpPanelVisible;
        PrevButton.Visible = PrevScreen != null;
        PrevButton.Enabled = Prevable;
        NextButton.Enabled = Nextable;
        CancelButton.Enabled = Cancelable;
        OnEnter();
        CurrentScreen = this;
    }

    public static void SetupButton() {
        NextButton.Click += NextButton_Click;
        PrevButton.Click += PrevButton_Click;
        CancelButton.Click += CancelButton_Click;
    }

    private static void NextButton_Click(object sender, System.EventArgs e) {
        if(CurrentScreen.OnNext()) Next();
    }

    private static void PrevButton_Click(object sender, System.EventArgs e) {
        Prev();
    }

    private static void CancelButton_Click(object sender, System.EventArgs e) {
        Application.Exit();
    }

    public static void Next() {
        Screen screen = CurrentScreen.NextScreen;
        if(screen != null) screen.Enter();
        else {
            CurrentScreen.OnLeave();
            Application.Exit();
        }
    }

    public static void Prev() {
        CurrentScreen.PrevScreen.Enter();
    }
}