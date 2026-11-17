using Microsoft.Win32;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuietOnTheSetUI
{
    public partial class Form1 : Form
    {
        VolumeGuard _volumeGuard;
        private bool _isLocked = false;
        private string _password;
        private int _maxVolume;
        private bool _exitAllowed = false;
        private bool _suppressAutostartEvents = false;

        // Removes the app from Alt+Tab window if minimized
        protected override CreateParams CreateParams
        {
            get
            {
                var Params = base.CreateParams;
                if (FormWindowState.Minimized == this.WindowState)
                {
                    Params.ExStyle |= 0x80;
                }
                return Params;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                base.OnShown(e);
                Hide();
            }
        }

        public Form1()
        {
            InitializeComponent();
            ApplyTheme();

            try
            {
                checkBox2.Checked = Convert.ToBoolean(Properties.Settings.Default["StartMinimized"]);
            }
            catch (Exception)
            {
                checkBox2.Checked = false;
            }

            InitializeAutostartUi();

            try
            {
                showWarningCheckBox.Checked = Convert.ToBoolean(Properties.Settings.Default["ShowVolumeWarning"]);
            }
            catch (Exception)
            {
                showWarningCheckBox.Checked = true;
            }

            if (checkBox2.Checked)
            {
                //  Hides the app completely
                Form1_FormClosing(null, new FormClosingEventArgs(new CloseReason(), true));

                //  The volume is automatically locked if the app is minimized 
                Properties.Settings.Default["IsLocked"] = true;
                Properties.Settings.Default.Save();
            }

            this.Icon = QuietOnTheSetUI.Properties.Resources.appicon;
            notifyIcon1.Icon = QuietOnTheSetUI.Properties.Resources.appicon;
            volumeTrackBar.ValueChanged += VolumeTrackBar_ValueChanged;
            _volumeGuard = new VolumeGuard();
            _volumeGuard.CurrentVolumeChanged += VolumeGuard_CurrentVolumeChanged;
            _volumeGuard.VolumeLimitResisted += VolumeGuard_VolumeLimitResisted;
            _maxVolume = Convert.ToInt16(Properties.Settings.Default["MaxVolume"]);
            volumeTrackBar.Value = _maxVolume;
            _volumeGuard.MaxVolume = _maxVolume;
            _isLocked = Convert.ToBoolean(Properties.Settings.Default["IsLocked"]);
            _password = Properties.Settings.Default["UnlockCode"].ToString();
            currentVolumeLabel.Text = _volumeGuard.ReadCurrentVolume().ToString();
            notifyIcon1.BalloonTipTitle = "silencer";
            if (_isLocked)
            {
                LockVolume(true);
            }
            else
            {
                UnlockVolume();
            }

            this.FormClosing += Form1_FormClosing;
            this.FormClosed += Form1_FormClosed;
            this.Resize += Form1_Resize;

            UpdateFooter();
        }

        private void UpdateFooter()
        {
            DateTime buildDate = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;

            footerLabel.Text = $"silencer · {buildDate.ToString("g")}";
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500);
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
                notifyIcon1.Visible = false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_exitAllowed == false)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
            }
        }

        internal void LockVolume(bool initializing = false)
        {
            _isLocked = true;
            volumeTrackBar.Enabled = false;
            if (!initializing)
            {
                _maxVolume = volumeTrackBar.Value;
                Properties.Settings.Default["MaxVolume"] = _maxVolume.ToString();
                Properties.Settings.Default["IsLocked"] = true;
                Properties.Settings.Default["UnlockCode"] = passwordTextBox.Text;
                Properties.Settings.Default.Save();
            }
            _password = passwordTextBox.Text;
            passwordTextBox.Text = string.Empty;
            confirmPasswordTextBox.Text = string.Empty;
            notifyIcon1.BalloonTipText = _balloonTipText;
            notifyIcon1.Text = _balloonTipText;
            if (_password.Length > 0) { lockButton.Enabled = false; }
            exitButton.Visible = false;
            _volumeGuard.MaxVolume = _maxVolume;
            _volumeGuard.IsLocked = true;
            ApplyLockVisuals();
        }
        internal void UnlockVolume()
        {
            _isLocked = false;
            volumeTrackBar.Enabled = true;
            Properties.Settings.Default["IsLocked"] = false;
            Properties.Settings.Default["UnlockCode"] = string.Empty;
            Properties.Settings.Default.Save();
            passwordTextBox.Text = string.Empty;
            confirmPasswordTextBox.Text = string.Empty;
            notifyIcon1.BalloonTipText = _balloonTipText;
            notifyIcon1.Text = _balloonTipText;
            exitButton.Visible = true;
            _password = string.Empty;
            _volumeGuard.IsLocked = false;
            ApplyLockVisuals();
        }

        private string _balloonTipText
        {
            get
            {
                if (_isLocked)
                {
                    return $"音量上限已锁定在 {volumeTrackBar.Value}";
                }
                else
                {
                    return "尚未设置音量上限";
                }
            }
        }

        private void VolumeGuard_CurrentVolumeChanged(int newVolume)
        {
            if (currentVolumeLabel.InvokeRequired)
            {
                currentVolumeLabel.Invoke(new MethodInvoker(delegate { currentVolumeLabel.Text = newVolume.ToString(); }));
            }
            else
            {
                currentVolumeLabel.Text = newVolume.ToString();
            }
        }

        private void VolumeGuard_VolumeLimitResisted()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(VolumeGuard_VolumeLimitResisted));
                return;
            }
            if (showWarningCheckBox.Checked)
            {
                QuoteToastForm.ShowRandom();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_volumeGuard != null)
            {
                _volumeGuard.CurrentVolumeChanged -= VolumeGuard_CurrentVolumeChanged;
                _volumeGuard.VolumeLimitResisted -= VolumeGuard_VolumeLimitResisted;
                _volumeGuard.Dispose();
                _volumeGuard = null;
            }
        }

        private void VolumeTrackBar_ValueChanged(object sender, EventArgs e)
        {
            maxVolumeLabel.Text = volumeTrackBar.Value.ToString();
        }

        private void volumeTrackBar_Scroll(object sender, EventArgs e)
        {
            var trackBar = (TrackBar)sender;
            maxVolumeLabel.Text = trackBar.Value.ToString();
        }

        private void lockButton_Click(object sender, EventArgs e)
        {
            if (_isLocked)
            {
                UnlockVolume();
            }
            else
            {
                LockVolume();
            }
        }

        private void passwordTextBox_TextChanged(object sender, EventArgs e)
        {
            ValidatePasswords();
        }

        private void confirmPasswordTextBox_TextChanged(object sender, EventArgs e)
        {
            ValidatePasswords();
        }

        internal void ValidatePasswords()
        {
            if (_isLocked)
            {
                lockButton.Enabled = passwordTextBox.Text.Equals(confirmPasswordTextBox.Text) && passwordTextBox.Text.Equals(_password);
            }
            else
            {
                lockButton.Enabled = passwordTextBox.Text.Equals(confirmPasswordTextBox.Text);
            }
        }

        private void showPasswordCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            var isChecked = ((CheckBox)sender).Checked;
            passwordTextBox.UseSystemPasswordChar = !isChecked;
            confirmPasswordTextBox.UseSystemPasswordChar = !isChecked;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Show must be called before setting WindowState,
            // otherwise the window loses its size and position
            this.Show();
            this.WindowState = FormWindowState.Normal;
            MaxmizedFromTray();
        }

        private void MaxmizedFromTray()
        {
            notifyIcon1.Visible = false;
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            var response = MessageBox.Show("退出后将不再限制音量，系统音量可以被调到最大。确定要退出吗？", "退出确认", MessageBoxButtons.YesNo);
            if (response == DialogResult.Yes)
            {
                _exitAllowed = true;
                Application.Exit();
            }
        }

        private void InitializeAutostartUi()
        {
            _suppressAutostartEvents = true;
            try
            {
                bool preferAutostart;
                try
                {
                    preferAutostart = Convert.ToBoolean(Properties.Settings.Default["StartAutomatically"]);
                }
                catch (Exception)
                {
                    preferAutostart = true;
                }

                string exePath = Application.ExecutablePath;
                bool registered = AutostartHelper.IsRegisteredFor(exePath);
                bool windowsDisabled = AutostartHelper.IsDisabledByWindows();

                // Installer may have written the Run key before first launch.
                // Prefer registry truth for display; do not clear a valid install registration
                // just because an older user.config still says false.
                if (registered && !preferAutostart)
                {
                    preferAutostart = true;
                    Properties.Settings.Default["StartAutomatically"] = true;
                    Properties.Settings.Default.Save();
                    AutostartLog.Write("prefer-synced-from-registry", "registered=true");
                }

                // Repair missing/wrong Run value only when the user still prefers autostart
                // and Windows has not disabled the startup entry.
                if (preferAutostart && !registered && !windowsDisabled)
                {
                    string error;
                    if (AutostartHelper.TryEnable(exePath, out error))
                    {
                        registered = true;
                    }
                    else
                    {
                        AutostartLog.Write("startup-provision-failed", error ?? "");
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            MessageBox.Show(error, "登录自启动", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                else if (preferAutostart && windowsDisabled)
                {
                    AutostartLog.Write("startup-windows-disabled", "leaving StartupApproved unchanged");
                    MessageBox.Show(
                        "检测到 Windows 已在任务管理器中禁用 silencer 登录启动。请手动重新启用以恢复开机自启；本程序不会绕过该设置。",
                        "登录自启动",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                // Checkbox reflects the Run key itself. StartupApproved (Task Manager)
                // disable is reported separately and never cleared by this app.
                checkBox1.Checked = AutostartHelper.IsRegisteredFor(exePath);
            }
            finally
            {
                _suppressAutostartEvents = false;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressAutostartEvents)
            {
                return;
            }

            if (checkBox1.Checked)
            {
                string error;
                if (!AutostartHelper.TryEnable(Application.ExecutablePath, out error))
                {
                    _suppressAutostartEvents = true;
                    checkBox1.Checked = false;
                    _suppressAutostartEvents = false;
                    Properties.Settings.Default["StartAutomatically"] = false;
                    Properties.Settings.Default.Save();
                    MessageBox.Show(
                        string.IsNullOrWhiteSpace(error) ? "设置登录自启动失败。" : error,
                        "登录自启动",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                Properties.Settings.Default["StartAutomatically"] = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                string error;
                if (!AutostartHelper.TryDisable(out error))
                {
                    MessageBox.Show(
                        string.IsNullOrWhiteSpace(error) ? "取消登录自启动失败。" : error,
                        "登录自启动",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                Properties.Settings.Default["StartAutomatically"] = false;
                Properties.Settings.Default.Save();
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default["StartMinimized"] = checkBox2.Checked;
            Properties.Settings.Default.Save();
        }

        private void showWarningCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default["ShowVolumeWarning"] = showWarningCheckBox.Checked;
            Properties.Settings.Default.Save();
        }

        void ApplyTheme()
        {
            var paper = Color.FromArgb(246, 246, 244);
            var ink = Color.FromArgb(28, 28, 30);
            var muted = Color.FromArgb(110, 110, 115);
            var card = Color.White;
            var titleFont = new Font("Segoe UI", 12F, FontStyle.Bold);
            var numberFont = new Font("Segoe UI", 28F, FontStyle.Bold);
            var captionFont = new Font("Segoe UI", 9F);
            var bodyFont = new Font("Segoe UI", 9.75F);
            var buttonFont = new Font("Segoe UI", 11F, FontStyle.Bold);

            this.BackColor = paper;
            this.ForeColor = ink;
            this.Font = bodyFont;

            titleLabel.Font = titleFont;
            titleLabel.ForeColor = ink;
            statusLabel.Font = captionFont;

            foreach (var panel in new[] { panel1, panel2 })
            {
                panel.BackColor = card;
                panel.BorderStyle = BorderStyle.None;
            }

            currentVolumeLabel.Font = numberFont;
            currentVolumeLabel.ForeColor = ink;
            maxVolumeLabel.Font = numberFont;
            maxVolumeLabel.ForeColor = ink;
            label4.Font = captionFont;
            label4.ForeColor = muted;
            label5.Font = captionFont;
            label5.ForeColor = muted;
            label3.Font = captionFont;
            label3.ForeColor = muted;
            label1.Font = captionFont;
            label1.ForeColor = muted;
            label2.Font = captionFont;
            label2.ForeColor = muted;
            footerLabel.Font = new Font("Segoe UI", 8.25F);
            footerLabel.ForeColor = muted;

            passwordTextBox.Font = bodyFont;
            passwordTextBox.BorderStyle = BorderStyle.FixedSingle;
            confirmPasswordTextBox.Font = bodyFont;
            confirmPasswordTextBox.BorderStyle = BorderStyle.FixedSingle;

            showPasswordCheckbox.Font = captionFont;
            showPasswordCheckbox.ForeColor = ink;
            checkBox1.Font = captionFont;
            checkBox1.ForeColor = ink;
            checkBox2.Font = captionFont;
            checkBox2.ForeColor = ink;
            showWarningCheckBox.Font = captionFont;
            showWarningCheckBox.ForeColor = ink;

            StyleButton(lockButton, buttonFont, true);
            StyleButton(exitButton, buttonFont, false);
            ApplyLockVisuals();
        }

        void StyleButton(Button button, Font font, bool primary)
        {
            button.Font = font;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(214, 214, 210);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        void ApplyLockVisuals()
        {
            var accent = Color.FromArgb(28, 112, 102);
            var accentDark = Color.FromArgb(18, 90, 82);
            var muted = Color.FromArgb(110, 110, 115);
            var ink = Color.FromArgb(28, 28, 30);

            if (_isLocked)
            {
                statusLabel.Text = "已锁定";
                statusLabel.ForeColor = accent;
                lockButton.Text = "解锁";
                lockButton.BackColor = accentDark;
                lockButton.ForeColor = Color.White;
                lockButton.FlatAppearance.BorderSize = 0;
                exitButton.Visible = false;
                lockButton.SetBounds(20, 410, 420, 44);
                panel2.BackColor = Color.FromArgb(232, 244, 241);
            }
            else
            {
                statusLabel.Text = "未锁定";
                statusLabel.ForeColor = muted;
                lockButton.Text = "锁定";
                lockButton.BackColor = accent;
                lockButton.ForeColor = Color.White;
                lockButton.FlatAppearance.BorderSize = 0;
                exitButton.Visible = true;
                lockButton.SetBounds(20, 410, 200, 44);
                exitButton.BackColor = Color.White;
                exitButton.ForeColor = ink;
                exitButton.FlatAppearance.BorderSize = 1;
                panel2.BackColor = Color.White;
            }
        }
    }
}
