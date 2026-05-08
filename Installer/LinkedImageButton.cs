using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using JipperResourcePack.Installer.Resource;

namespace JipperResourcePack.Installer;

public class LinkedImageButton : PictureBox {
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