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
            this.MainPanel = new System.Windows.Forms.Panel();
            this.VersionLabel = new System.Windows.Forms.Label();
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
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Size = new System.Drawing.Size(984, 561);
            this.MainPanel.TabIndex = 0;
            this.MainPanel.Name = "MainPanel";
            // 
            // VersionLabel
            // 
            this.VersionLabel.Location = new System.Drawing.Point(909, 547);
            this.VersionLabel.Size = new System.Drawing.Size(75, 14);
            this.VersionLabel.TabIndex = 0;
            this.VersionLabel.Text = "Initializing...";
            this.VersionLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.VersionLabel.ForeColor = System.Drawing.Color.FromArgb(((int) (((byte) (224)))), ((int) (((byte) (224)))), ((int) (((byte) (224)))));
            this.VersionLabel.Name = "VersionLabel";
            // 
            // UnderPanel
            // 
            this.UnderPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.UnderPanel.Controls.Add(this.Prev);
            this.UnderPanel.Controls.Add(this.Next);
            this.UnderPanel.Controls.Add(this.Cancel);
            this.UnderPanel.Location = new System.Drawing.Point(0, 561);
            this.UnderPanel.Size = new System.Drawing.Size(984, 40);
            this.UnderPanel.TabIndex = 1;
            this.UnderPanel.Name = "UnderPanel";
            // 
            // Prev
            // 
            this.Prev.Location = new System.Drawing.Point(710, 5);
            this.Prev.Size = new System.Drawing.Size(80, 30);
            this.Prev.TabIndex = 2;
            this.Prev.Name = "Prev";
            this.Prev.UseVisualStyleBackColor = true;
            // 
            // Next
            // 
            this.Next.Location = new System.Drawing.Point(790, 5);
            this.Next.Size = new System.Drawing.Size(80, 30);
            this.Next.TabIndex = 1;
            this.Next.Name = "Next";
            this.Next.UseVisualStyleBackColor = true;
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(890, 5);
            this.Cancel.Size = new System.Drawing.Size(80, 30);
            this.Cancel.TabIndex = 0;
            this.Cancel.Name = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // TopPanel
            // 
            this.TopPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.TopPanel.Location = new System.Drawing.Point(0, 0);
            this.TopPanel.Size = new System.Drawing.Size(984, 40);
            this.TopPanel.TabIndex = 2;
            this.TopPanel.Name = "TopPanel";
            // 
            // InstallerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(984, 601);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.VersionLabel);
            this.Controls.Add(this.TopPanel);
            this.Controls.Add(this.UnderPanel);
            this.Controls.Add(this.MainPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "InstallerForm";
            this.UnderPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.Label VersionLabel;

        private System.Windows.Forms.Panel TopPanel;

        private System.Windows.Forms.Button Prev;

        private System.Windows.Forms.Button Next;

        private System.Windows.Forms.Button Cancel;

        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.Panel UnderPanel;

        #endregion

    }
}
