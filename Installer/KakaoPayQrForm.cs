using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using JipperResourcePack.Installer.Resource;

namespace JipperResourcePack.Installer;

public class KakaoPayQrForm : Form {
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