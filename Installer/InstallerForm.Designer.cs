using JipperResourcePack.Installer.Properties;

namespace JipperResourcePack.Installer {
    partial class InstallerForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallerForm));
            this.MainPanel = new System.Windows.Forms.Panel();
            this.UnderPanel = new System.Windows.Forms.Panel();
            this.Prev = new System.Windows.Forms.Button();
            this.Next = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.TopPanel = new System.Windows.Forms.Panel();
            this.UnderPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            resources.ApplyResources(this.MainPanel, "MainPanel");
            this.MainPanel.Name = "MainPanel";
            // 
            // UnderPanel
            // 
            this.UnderPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.UnderPanel.Controls.Add(this.Prev);
            this.UnderPanel.Controls.Add(this.Next);
            this.UnderPanel.Controls.Add(this.Cancel);
            resources.ApplyResources(this.UnderPanel, "UnderPanel");
            this.UnderPanel.Name = "UnderPanel";
            // 
            // Prev
            // 
            resources.ApplyResources(this.Prev, "Prev");
            this.Prev.Name = "Prev";
            this.Prev.UseVisualStyleBackColor = true;
            // 
            // Next
            // 
            resources.ApplyResources(this.Next, "Next");
            this.Next.Name = "Next";
            this.Next.UseVisualStyleBackColor = true;
            // 
            // Cancel
            // 
            resources.ApplyResources(this.Cancel, "Cancel");
            this.Cancel.Name = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // TopPanel
            // 
            this.TopPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            resources.ApplyResources(this.TopPanel, "TopPanel");
            this.TopPanel.Name = "TopPanel";
            // 
            // InstallerForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.TopPanel);
            this.Controls.Add(this.UnderPanel);
            this.Controls.Add(this.MainPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "InstallerForm";
            this.UnderPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel TopPanel;

        private System.Windows.Forms.Button Prev;

        private System.Windows.Forms.Button Next;

        private System.Windows.Forms.Button Cancel;

        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.Panel UnderPanel;

        #endregion

    }
}