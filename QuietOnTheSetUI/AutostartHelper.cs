using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Principal;
using System.Text;

namespace QuietOnTheSetUI
{
    /// <summary>
    /// Manages HKCU Run registration for login autostart.
    /// Does not bypass Windows StartupApproved / Task Manager disable flags.
    /// </summary>
    internal static class AutostartHelper
    {
        internal const string RunValueName = "silencer";
        private const string LegacyRunValueName = "QuietOnTheSet";
        private const string RunSubKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string StartupApprovedSubKeyPath =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

        internal static string QuoteExecutablePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Executable path is empty.", nameof(path));
            }

            var trimmed = path.Trim().Trim('"');
            return "\"" + trimmed + "\"";
        }

        internal static string NormalizePath(string pathOrQuoted)
        {
            if (string.IsNullOrWhiteSpace(pathOrQuoted))
            {
                return string.Empty;
            }

            var trimmed = pathOrQuoted.Trim().Trim('"');
            try
            {
                return Path.GetFullPath(trimmed);
            }
            catch
            {
                return trimmed;
            }
        }

        internal static bool PathsEqual(string left, string right)
        {
            return string.Equals(
                NormalizePath(left),
                NormalizePath(right),
                StringComparison.OrdinalIgnoreCase);
        }

        internal static string GetCurrentUserName()
        {
            try
            {
                return WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName;
            }
            catch
            {
                return Environment.UserName;
            }
        }

        internal static bool IsElevated()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        internal static string GetRegisteredCommand()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunSubKeyPath, false))
                {
                    if (key == null)
                    {
                        return null;
                    }

                    return key.GetValue(RunValueName) as string;
                }
            }
            catch (Exception ex)
            {
                AutostartLog.Write("read-run-failed", ex.Message);
                return null;
            }
        }

        internal static bool IsRegisteredFor(string executablePath)
        {
            var registered = GetRegisteredCommand();
            if (string.IsNullOrWhiteSpace(registered))
            {
                return false;
            }

            return PathsEqual(registered, executablePath);
        }

        /// <summary>
        /// Returns true when Windows StartupApproved marks the Run entry as disabled.
        /// We never clear or rewrite StartupApproved.
        /// </summary>
        internal static bool IsDisabledByWindows()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(StartupApprovedSubKeyPath, false))
                {
                    if (key == null)
                    {
                        return false;
                    }

                    var raw = key.GetValue(RunValueName) as byte[];
                    if (raw == null || raw.Length < 1)
                    {
                        return false;
                    }

                    // Common disabled markers observed on Windows 10/11: 0x03 / 0x07.
                    // Enabled markers are typically 0x02 / 0x06.
                    return raw[0] == 0x03 || raw[0] == 0x07;
                }
            }
            catch (Exception ex)
            {
                AutostartLog.Write("read-startup-approved-failed", ex.Message);
                return false;
            }
        }

        internal static bool TryEnable(string executablePath, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                if (IsDisabledByWindows())
                {
                    errorMessage =
                        "Windows 任务管理器已禁用 silencer 的登录启动项。请在「任务管理器 → 启动」中重新启用；本程序不会绕过该设置。";
                    AutostartLog.Write("enable-blocked-by-windows", "StartupApproved disabled");
                    return false;
                }

                var quoted = QuoteExecutablePath(executablePath);
                using (var key = Registry.CurrentUser.OpenSubKey(RunSubKeyPath, true)
                    ?? Registry.CurrentUser.CreateSubKey(RunSubKeyPath))
                {
                    if (key == null)
                    {
                        errorMessage = "无法打开当前用户的启动项注册表。";
                        AutostartLog.Write("enable-open-key-failed", errorMessage);
                        return false;
                    }

                    key.SetValue(RunValueName, quoted);
                    try
                    {
                        key.DeleteValue(LegacyRunValueName, false);
                    }
                    catch
                    {
                        // Ignore missing legacy value.
                    }
                }

                var verified = GetRegisteredCommand();
                if (!PathsEqual(verified, executablePath))
                {
                    errorMessage = "写入启动项后校验失败。";
                    AutostartLog.Write("enable-verify-failed", "got=" + (verified ?? "(null)"));
                    return false;
                }

                AutostartLog.Write(
                    "enable-ok",
                    "user=" + GetCurrentUserName()
                    + "; elevated=" + IsElevated()
                    + "; path=" + quoted);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "设置登录自启动失败：" + ex.Message;
                AutostartLog.Write("enable-exception", ex.GetType().Name + ": " + ex.Message);
                return false;
            }
        }

        internal static bool TryDisable(out string errorMessage)
        {
            errorMessage = null;
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunSubKeyPath, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(RunValueName, false);
                        key.DeleteValue(LegacyRunValueName, false);
                    }
                }

                AutostartLog.Write(
                    "disable-ok",
                    "user=" + GetCurrentUserName() + "; elevated=" + IsElevated());
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "取消登录自启动失败：" + ex.Message;
                AutostartLog.Write("disable-exception", ex.GetType().Name + ": " + ex.Message);
                return false;
            }
        }
    }

    internal static class AutostartLog
    {
        private static readonly object Sync = new object();

        internal static string GetLogDirectory()
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(root, "silencer", "logs");
        }

        internal static string GetLogFilePath()
        {
            return Path.Combine(GetLogDirectory(), "autostart.log");
        }

        internal static void Write(string eventName, string detail)
        {
            try
            {
                var dir = GetLogDirectory();
                Directory.CreateDirectory(dir);
                var line = new StringBuilder()
                    .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    .Append('\t')
                    .Append(eventName ?? "")
                    .Append('\t')
                    .Append(Sanitize(detail))
                    .AppendLine()
                    .ToString();

                lock (Sync)
                {
                    File.AppendAllText(GetLogFilePath(), line, Encoding.UTF8);
                }
            }
            catch
            {
                // Logging must never break the UI.
            }
        }

        private static string Sanitize(string detail)
        {
            if (string.IsNullOrEmpty(detail))
            {
                return string.Empty;
            }

            // Never persist passwords or unlock codes if they somehow appear.
            var cleaned = detail.Replace('\r', ' ').Replace('\n', ' ');
            if (cleaned.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0
                || cleaned.IndexOf("UnlockCode", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "(redacted)";
            }

            if (cleaned.Length > 500)
            {
                return cleaned.Substring(0, 500);
            }

            return cleaned;
        }
    }
}
