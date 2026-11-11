namespace QuietOnTheSetUI
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.volumeTrackBar = new System.Windows.Forms.TrackBar();
            this.lockButton = new System.Windows.Forms.Button();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.confirmPasswordTextBox = new System.Windows.Forms.TextBox();
            this.currentVolumeLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.showPasswordCheckbox = new System.Windows.Forms.CheckBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.maxVolumeLabel = new System.Windows.Forms.Label();
            this.exitButton = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.footerLabel = new System.Windows.Forms.Label();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.showWarningCheckBox = new System.Windows.Forms.CheckBox();
            this.titleLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.volumeTrackBar)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Location = new System.Drawing.Point(20, 18);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(160, 21);
            this.titleLabel.TabIndex = 17;
            this.titleLabel.Text = "silencer";
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLabel.Location = new System.Drawing.Point(280, 20);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(160, 21);
            this.statusLabel.TabIndex = 18;
            this.statusLabel.Text = "未锁定";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.currentVolumeLabel);
            this.panel1.Location = new System.Drawing.Point(20, 56);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(8);
            this.panel1.Size = new System.Drawing.Size(200, 108);
            this.panel1.TabIndex = 11;
            // 
            // currentVolumeLabel
            // 
            this.currentVolumeLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.currentVolumeLabel.Location = new System.Drawing.Point(8, 8);
            this.currentVolumeLabel.Name = "currentVolumeLabel";
            this.currentVolumeLabel.Size = new System.Drawing.Size(184, 58);
            this.currentVolumeLabel.TabIndex = 5;
            this.currentVolumeLabel.Text = "100";
            this.currentVolumeLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label4.Location = new System.Drawing.Point(8, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(184, 28);
            this.label4.TabIndex = 10;
            this.label4.Text = "当前音量";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.maxVolumeLabel);
            this.panel2.Location = new System.Drawing.Point(240, 56);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(8);
            this.panel2.Size = new System.Drawing.Size(200, 108);
            this.panel2.TabIndex = 12;
            // 
            // maxVolumeLabel
            // 
            this.maxVolumeLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.maxVolumeLabel.Location = new System.Drawing.Point(8, 8);
            this.maxVolumeLabel.Name = "maxVolumeLabel";
            this.maxVolumeLabel.Size = new System.Drawing.Size(184, 58);
            this.maxVolumeLabel.TabIndex = 5;
            this.maxVolumeLabel.Text = "100";
            this.maxVolumeLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label5
            // 
            this.label5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label5.Location = new System.Drawing.Point(8, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(184, 28);
            this.label5.TabIndex = 10;
            this.label5.Text = "音量上限";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(20, 180);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(420, 22);
            this.label3.TabIndex = 9;
            this.label3.Text = "拖动滑块设置最高音量";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // volumeTrackBar
            // 
            this.volumeTrackBar.AutoSize = false;
            this.volumeTrackBar.Location = new System.Drawing.Point(14, 204);
            this.volumeTrackBar.Maximum = 100;
            this.volumeTrackBar.Name = "volumeTrackBar";
            this.volumeTrackBar.Size = new System.Drawing.Size(432, 36);
            this.volumeTrackBar.TabIndex = 0;
            this.volumeTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.volumeTrackBar.Scroll += new System.EventHandler(this.volumeTrackBar_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 254);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "锁定密码（可选）";
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(20, 276);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(420, 23);
            this.passwordTextBox.TabIndex = 3;
            this.passwordTextBox.UseSystemPasswordChar = true;
            this.passwordTextBox.TextChanged += new System.EventHandler(this.passwordTextBox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 312);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "确认密码";
            // 
            // confirmPasswordTextBox
            // 
            this.confirmPasswordTextBox.Location = new System.Drawing.Point(20, 334);
            this.confirmPasswordTextBox.Name = "confirmPasswordTextBox";
            this.confirmPasswordTextBox.Size = new System.Drawing.Size(420, 23);
            this.confirmPasswordTextBox.TabIndex = 4;
            this.confirmPasswordTextBox.UseSystemPasswordChar = true;
            this.confirmPasswordTextBox.TextChanged += new System.EventHandler(this.confirmPasswordTextBox_TextChanged);
            // 
            // showPasswordCheckbox
            // 
            this.showPasswordCheckbox.AutoSize = true;
            this.showPasswordCheckbox.Location = new System.Drawing.Point(20, 368);
            this.showPasswordCheckbox.Name = "showPasswordCheckbox";
            this.showPasswordCheckbox.Size = new System.Drawing.Size(87, 21);
            this.showPasswordCheckbox.TabIndex = 8;
            this.showPasswordCheckbox.Text = "显示密码";
            this.showPasswordCheckbox.UseVisualStyleBackColor = true;
            this.showPasswordCheckbox.CheckedChanged += new System.EventHandler(this.showPasswordCheckbox_CheckedChanged);
            // 
            // lockButton
            // 
            this.lockButton.Location = new System.Drawing.Point(20, 410);
            this.lockButton.Name = "lockButton";
            this.lockButton.Size = new System.Drawing.Size(200, 44);
            this.lockButton.TabIndex = 2;
            this.lockButton.Text = "锁定";
            this.lockButton.UseVisualStyleBackColor = false;
            this.lockButton.Click += new System.EventHandler(this.lockButton_Click);
            // 
            // exitButton
            // 
            this.exitButton.Location = new System.Drawing.Point(240, 410);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(200, 44);
            this.exitButton.TabIndex = 13;
            this.exitButton.Text = "退出";
            this.exitButton.UseVisualStyleBackColor = false;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(20, 470);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(135, 21);
            this.checkBox1.TabIndex = 14;
            this.checkBox1.Text = "登录时自动启动";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(20, 496);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(123, 21);
            this.checkBox2.TabIndex = 16;
            this.checkBox2.Text = "启动时最小化";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // showWarningCheckBox
            // 
            this.showWarningCheckBox.AutoSize = true;
            this.showWarningCheckBox.Checked = true;
            this.showWarningCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showWarningCheckBox.Location = new System.Drawing.Point(20, 522);
            this.showWarningCheckBox.Name = "showWarningCheckBox";
            this.showWarningCheckBox.Size = new System.Drawing.Size(303, 21);
            this.showWarningCheckBox.TabIndex = 17;
            this.showWarningCheckBox.Text = "音量超限时弹出警示（全屏或右下角）";
            this.showWarningCheckBox.UseVisualStyleBackColor = true;
            this.showWarningCheckBox.CheckedChanged += new System.EventHandler(this.showWarningCheckBox_CheckedChanged);
            // 
            // footerLabel
            // 
            this.footerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.footerLabel.Location = new System.Drawing.Point(20, 552);
            this.footerLabel.Name = "footerLabel";
            this.footerLabel.Size = new System.Drawing.Size(420, 18);
            this.footerLabel.TabIndex = 15;
            this.footerLabel.Text = "silencer";
            this.footerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "silencer";
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(460, 580);
            this.Controls.Add(this.footerLabel);
            this.Controls.Add(this.showWarningCheckBox);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.lockButton);
            this.Controls.Add(this.showPasswordCheckbox);
            this.Controls.Add(this.confirmPasswordTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.volumeTrackBar);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.titleLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "silencer";
            ((System.ComponentModel.ISupportInitialize)(this.volumeTrackBar)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TrackBar volumeTrackBar;
        private System.Windows.Forms.Button lockButton;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.TextBox confirmPasswordTextBox;
        private System.Windows.Forms.Label currentVolumeLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox showPasswordCheckbox;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label maxVolumeLabel;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label footerLabel;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox showWarningCheckBox;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label statusLabel;
    }
}
