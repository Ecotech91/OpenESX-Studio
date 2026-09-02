using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("OpenESX Studio Setup")]
[assembly: AssemblyDescription("Installer für OpenESX Studio 2.0 RC5")]
[assembly: AssemblyCompany("OpenESX Studio")]
[assembly: AssemblyProduct("OpenESX Studio Setup")]
[assembly: AssemblyCopyright("Copyright (c) 2026")]
[assembly: AssemblyVersion("2.0.5.0")]
[assembly: AssemblyFileVersion("2.0.5.0")]

namespace OpenEsxStudio.Setup
{
    internal static class SetupProgram
    {
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && string.Equals(args[0], "--uninstall", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.ExitCode = InstallerEngine.BeginUninstall(false);
                    return;
                }
                if (args.Length > 0 && string.Equals(args[0], "--silent-uninstall", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.ExitCode = InstallerEngine.BeginUninstall(true);
                    return;
                }
                if (args.Length >= 3 && string.Equals(args[0], "--cleanup", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.ExitCode = InstallerEngine.CleanupInstalledFiles(args[1], args[2], args.Length > 3 && args[3] == "show");
                    return;
                }
                if (args.Length >= 2 && string.Equals(args[0], "--self-test", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.ExitCode = InstallerEngine.RunSelfTest(args[1]);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new SetupForm());
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Der Vorgang konnte nicht abgeschlossen werden.\r\n\r\n" + exception.Message,
                    "OpenESX Studio Setup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.ExitCode = 1;
            }
        }
    }

    internal sealed class SetupForm : Form
    {
        private readonly CheckBox desktopShortcut;
        private readonly CheckBox startAfterInstall;
        private readonly Button installButton;
        private readonly Label statusLabel;

        internal SetupForm()
        {
            Text = "OpenESX Studio Setup";
            ClientSize = new Size(610, 395);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(8, 18, 31);
            ForeColor = Color.FromArgb(232, 239, 247);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            Panel accent = new Panel();
            accent.BackColor = Color.FromArgb(59, 130, 246);
            accent.Location = new Point(0, 0);
            accent.Size = new Size(10, ClientSize.Height);
            Controls.Add(accent);

            Label eyebrow = CreateLabel("OFFLINE EDITION · WINDOWS X64", 42, 28, 520, 22, 9F, FontStyle.Bold, Color.FromArgb(96, 165, 250));
            Label title = CreateLabel("OpenESX Studio", 40, 55, 530, 48, 27F, FontStyle.Bold, Color.White);
            Label version = CreateLabel("Version 2.0 RC5", 43, 104, 300, 24, 11F, FontStyle.Regular, Color.FromArgb(162, 178, 198));
            Label copy = CreateLabel(
                "Installiert den vollständigen lokalen ESX-1 Sample- und Pattern-Editor.\r\n" +
                "Das Programm benötigt keine Internetverbindung und verändert keine Originaldatei.",
                43, 143, 525, 50, 10F, FontStyle.Regular, Color.FromArgb(198, 211, 226));
            Controls.Add(eyebrow);
            Controls.Add(title);
            Controls.Add(version);
            Controls.Add(copy);

            Label destinationTitle = CreateLabel("Installationsordner", 43, 207, 180, 21, 9F, FontStyle.Bold, Color.FromArgb(162, 178, 198));
            Controls.Add(destinationTitle);
            TextBox destination = new TextBox();
            destination.Location = new Point(43, 231);
            destination.Size = new Size(525, 25);
            destination.ReadOnly = true;
            destination.Text = InstallerEngine.InstallDirectory;
            destination.BackColor = Color.FromArgb(13, 28, 46);
            destination.ForeColor = Color.FromArgb(220, 230, 240);
            destination.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(destination);

            desktopShortcut = new CheckBox();
            desktopShortcut.Text = "Desktop-Verknüpfung erstellen";
            desktopShortcut.Checked = true;
            desktopShortcut.AutoSize = true;
            desktopShortcut.Location = new Point(43, 274);
            desktopShortcut.ForeColor = ForeColor;
            Controls.Add(desktopShortcut);

            startAfterInstall = new CheckBox();
            startAfterInstall.Text = "OpenESX Studio nach der Installation starten";
            startAfterInstall.Checked = true;
            startAfterInstall.AutoSize = true;
            startAfterInstall.Location = new Point(302, 274);
            startAfterInstall.ForeColor = ForeColor;
            Controls.Add(startAfterInstall);

            statusLabel = CreateLabel(
                "Keine Administratorrechte erforderlich.",
                43, 309, 340, 25, 9F, FontStyle.Regular, Color.FromArgb(134, 239, 172));
            Controls.Add(statusLabel);

            Button closeButton = new Button();
            closeButton.Text = "Abbrechen";
            closeButton.Location = new Point(356, 343);
            closeButton.Size = new Size(100, 34);
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.FlatAppearance.BorderColor = Color.FromArgb(58, 76, 99);
            closeButton.BackColor = Color.FromArgb(17, 32, 52);
            closeButton.ForeColor = ForeColor;
            closeButton.Click += delegate { Close(); };
            Controls.Add(closeButton);

            installButton = new Button();
            installButton.Text = InstallerEngine.IsInstalled ? "Aktualisieren" : "Installieren";
            installButton.Location = new Point(464, 343);
            installButton.Size = new Size(104, 34);
            installButton.FlatStyle = FlatStyle.Flat;
            installButton.FlatAppearance.BorderColor = Color.FromArgb(96, 165, 250);
            installButton.BackColor = Color.FromArgb(37, 99, 235);
            installButton.ForeColor = Color.White;
            installButton.Font = new Font(Font, FontStyle.Bold);
            installButton.Click += InstallClicked;
            Controls.Add(installButton);

            AcceptButton = installButton;
            CancelButton = closeButton;
        }

        private Label CreateLabel(string text, int x, int y, int width, int height, float size, FontStyle style, Color color)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = new Point(x, y);
            label.Size = new Size(width, height);
            label.Font = new Font("Segoe UI", size, style, GraphicsUnit.Point);
            label.ForeColor = color;
            return label;
        }

        private void InstallClicked(object sender, EventArgs e)
        {
            installButton.Enabled = false;
            statusLabel.ForeColor = Color.FromArgb(147, 197, 253);
            statusLabel.Text = "OpenESX Studio wird installiert …";
            Refresh();
            try
            {
                InstallerEngine.Install(desktopShortcut.Checked, startAfterInstall.Checked);
                statusLabel.ForeColor = Color.FromArgb(134, 239, 172);
                statusLabel.Text = "Installation erfolgreich abgeschlossen.";
                MessageBox.Show(
                    "OpenESX Studio 2.0 RC5 wurde erfolgreich installiert.",
                    "OpenESX Studio Setup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                Close();
            }
            catch (Exception exception)
            {
                statusLabel.ForeColor = Color.FromArgb(253, 164, 175);
                statusLabel.Text = "Installation fehlgeschlagen.";
                installButton.Enabled = true;
                MessageBox.Show(exception.Message, "OpenESX Studio Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    internal static class InstallerEngine
    {
        private const string LauncherResource = "OpenESXStudio.Setup.Portable.exe";
        private const string ReadmeResource = "OpenESXStudio.Setup.Readme.txt";
        private const string ProductKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenESXStudio";
        private const string MarkerText = "OpenESXStudio:2.0.5";
        private const string AppFileName = "OpenESXStudio.exe";
        private const string UninstallerFileName = "OpenESX Studio deinstallieren.exe";
        private const string ReadmeFileName = "README.txt";

        internal static string InstallDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programs",
                    "OpenESX Studio");
            }
        }

        private static string StartMenuDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Microsoft",
                    "Windows",
                    "Start Menu",
                    "Programs",
                    "OpenESX Studio");
            }
        }

        internal static bool IsInstalled
        {
            get { return File.Exists(Path.Combine(InstallDirectory, AppFileName)); }
        }

        internal static void Install(bool createDesktopShortcut, bool launchAfterInstall)
        {
            Directory.CreateDirectory(InstallDirectory);
            string launcherPath = Path.Combine(InstallDirectory, AppFileName);
            string uninstallerPath = Path.Combine(InstallDirectory, UninstallerFileName);
            string readmePath = Path.Combine(InstallDirectory, ReadmeFileName);
            string markerPath = Path.Combine(InstallDirectory, ".openesx-install");

            WriteResourceAtomic(LauncherResource, launcherPath);
            WriteResourceAtomic(ReadmeResource, readmePath);
            File.Copy(Application.ExecutablePath, uninstallerPath, true);
            File.WriteAllText(markerPath, MarkerText, Encoding.UTF8);

            Directory.CreateDirectory(StartMenuDirectory);
            CreateShortcut(Path.Combine(StartMenuDirectory, "OpenESX Studio.lnk"), launcherPath, null, "OpenESX Studio starten", launcherPath);
            CreateShortcut(Path.Combine(StartMenuDirectory, "OpenESX Studio deinstallieren.lnk"), uninstallerPath, "--uninstall", "OpenESX Studio entfernen", uninstallerPath);

            string desktopShortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "OpenESX Studio.lnk");
            if (createDesktopShortcut)
                CreateShortcut(desktopShortcutPath, launcherPath, null, "OpenESX Studio starten", launcherPath);
            else if (File.Exists(desktopShortcutPath))
                File.Delete(desktopShortcutPath);

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(ProductKey))
            {
                if (key == null)
                    throw new InvalidOperationException("Der Windows-Deinstallations-Eintrag konnte nicht angelegt werden.");
                key.SetValue("DisplayName", "OpenESX Studio", RegistryValueKind.String);
                key.SetValue("DisplayVersion", "2.0 RC5", RegistryValueKind.String);
                key.SetValue("Publisher", "OpenESX Studio", RegistryValueKind.String);
                key.SetValue("InstallLocation", InstallDirectory, RegistryValueKind.String);
                key.SetValue("DisplayIcon", launcherPath + ",0", RegistryValueKind.String);
                key.SetValue("UninstallString", Quote(uninstallerPath) + " --uninstall", RegistryValueKind.String);
                key.SetValue("QuietUninstallString", Quote(uninstallerPath) + " --silent-uninstall", RegistryValueKind.String);
                key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                long estimatedBytes = new FileInfo(launcherPath).Length + new FileInfo(uninstallerPath).Length + new FileInfo(readmePath).Length;
                int estimatedKilobytes = (int)Math.Min(Int32.MaxValue, Math.Max(1L, estimatedBytes / 1024L));
                key.SetValue("EstimatedSize", estimatedKilobytes, RegistryValueKind.DWord);
            }

            if (launchAfterInstall)
            {
                ProcessStartInfo start = new ProcessStartInfo(launcherPath);
                start.UseShellExecute = true;
                Process.Start(start);
            }
        }

        private static void WriteResourceAtomic(string resourceName, string destination)
        {
            byte[] content = ReadResource(resourceName);
            string temporary = destination + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllBytes(temporary, content);
                if (File.Exists(destination))
                    File.Delete(destination);
                File.Move(temporary, destination);
            }
            finally
            {
                if (File.Exists(temporary))
                    File.Delete(temporary);
            }
        }

        private static byte[] ReadResource(string resourceName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException("Eine Installationsdatei fehlt: " + resourceName);
                using (MemoryStream output = new MemoryStream())
                {
                    stream.CopyTo(output);
                    return output.ToArray();
                }
            }
        }

        private static void CreateShortcut(string shortcutPath, string targetPath, string arguments, string description, string iconPath)
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null)
                throw new InvalidOperationException("Windows Script Host ist für die Programmverknüpfung nicht verfügbar.");
            object shell = null;
            object shortcut = null;
            try
            {
                shell = Activator.CreateInstance(shellType);
                shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                Type shortcutType = shortcut.GetType();
                shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { targetPath });
                shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { Path.GetDirectoryName(targetPath) });
                shortcutType.InvokeMember("Description", BindingFlags.SetProperty, null, shortcut, new object[] { description });
                shortcutType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, shortcut, new object[] { iconPath + ",0" });
                if (!string.IsNullOrEmpty(arguments))
                    shortcutType.InvokeMember("Arguments", BindingFlags.SetProperty, null, shortcut, new object[] { arguments });
                shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
            }
            finally
            {
                if (shortcut != null && Marshal.IsComObject(shortcut))
                    Marshal.FinalReleaseComObject(shortcut);
                if (shell != null && Marshal.IsComObject(shell))
                    Marshal.FinalReleaseComObject(shell);
            }
        }

        internal static int BeginUninstall(bool silent)
        {
            if (!IsSafeInstallDirectory(InstallDirectory))
            {
                if (!silent)
                    MessageBox.Show("Die Installation konnte nicht eindeutig erkannt werden.", "OpenESX Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }

            if (!silent)
            {
                DialogResult answer = MessageBox.Show(
                    "OpenESX Studio vollständig von diesem Benutzerkonto entfernen?\r\n\r\nDeine ESX- und WAV-Dateien werden nicht gelöscht.",
                    "OpenESX Studio deinstallieren",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (answer != DialogResult.Yes)
                    return 0;
            }

            RemoveRegistration();
            string cleanupCopy = Path.Combine(Path.GetTempPath(), "OpenESX-Uninstall-" + Guid.NewGuid().ToString("N") + ".exe");
            File.Copy(Application.ExecutablePath, cleanupCopy, true);
            int processId = Process.GetCurrentProcess().Id;
            ProcessStartInfo cleanup = new ProcessStartInfo();
            cleanup.FileName = cleanupCopy;
            cleanup.Arguments = "--cleanup " + Quote(InstallDirectory) + " " + processId.ToString() + (silent ? "" : " show");
            cleanup.UseShellExecute = false;
            cleanup.CreateNoWindow = true;
            Process process = Process.Start(cleanup);
            return process == null ? 1 : 0;
        }

        private static void RemoveRegistration()
        {
            string desktopShortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "OpenESX Studio.lnk");
            if (File.Exists(desktopShortcutPath))
                File.Delete(desktopShortcutPath);
            if (Directory.Exists(StartMenuDirectory))
                Directory.Delete(StartMenuDirectory, true);
            Registry.CurrentUser.DeleteSubKeyTree(ProductKey, false);
        }

        private static bool IsSafeInstallDirectory(string directory)
        {
            try
            {
                string expected = Path.GetFullPath(InstallDirectory).TrimEnd(Path.DirectorySeparatorChar);
                string actual = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar);
                if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
                    return false;
                string marker = Path.Combine(actual, ".openesx-install");
                return File.Exists(marker) && string.Equals(File.ReadAllText(marker, Encoding.UTF8), MarkerText, StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        internal static int CleanupInstalledFiles(string directory, string processIdText, bool showCompletion)
        {
            try
            {
                int processId;
                if (!int.TryParse(processIdText, out processId))
                    throw new InvalidOperationException("Ungültiger Deinstallationsprozess.");
                try
                {
                    Process source = Process.GetProcessById(processId);
                    source.WaitForExit(15000);
                }
                catch (ArgumentException) { }

                if (!IsSafeInstallDirectory(directory))
                    throw new InvalidOperationException("Der Installationsordner konnte nicht sicher bestätigt werden.");
                Directory.Delete(Path.GetFullPath(directory), true);
                MoveFileEx(Application.ExecutablePath, null, 4);
                if (showCompletion)
                    MessageBox.Show("OpenESX Studio wurde entfernt.", "OpenESX Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return 0;
            }
            catch (Exception exception)
            {
                if (showCompletion)
                    MessageBox.Show("Die Deinstallation konnte nicht vollständig abgeschlossen werden.\r\n\r\n" + exception.Message, "OpenESX Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
        }

        internal static int RunSelfTest(string outputDirectory)
        {
            string fullOutput = Path.GetFullPath(outputDirectory);
            try
            {
                Directory.CreateDirectory(fullOutput);
                string launcherPath = Path.Combine(fullOutput, AppFileName);
                string readmePath = Path.Combine(fullOutput, ReadmeFileName);
                WriteResourceToTest(LauncherResource, launcherPath);
                WriteResourceToTest(ReadmeResource, readmePath);
                byte[] header = File.ReadAllBytes(launcherPath);
                if (header.Length < 2 || header[0] != (byte)'M' || header[1] != (byte)'Z')
                    throw new InvalidDataException("Die portable Programmdatei besitzt keinen gültigen Windows-EXE-Header.");
                string runtimeDirectory = Path.Combine(fullOutput, "Runtime");
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = launcherPath;
                start.Arguments = "--self-test " + Quote(runtimeDirectory);
                start.UseShellExecute = false;
                start.CreateNoWindow = true;
                Process process = Process.Start(start);
                if (process == null || !process.WaitForExit(30000) || process.ExitCode != 0)
                    throw new InvalidOperationException("Der Selbsttest der portablen EXE ist fehlgeschlagen.");
                string launcherReport = Path.Combine(runtimeDirectory, "launcher-self-test.txt");
                if (!File.Exists(launcherReport) || !File.ReadAllText(launcherReport).Contains("PASS"))
                    throw new InvalidDataException("Der Selbsttestbericht der portablen EXE fehlt.");
                File.WriteAllText(
                    Path.Combine(fullOutput, "installer-self-test.txt"),
                    "OpenESX Studio installer self-test: PASS\r\n" +
                    "Portable Windows-x64 EXE: extracted and executed\r\n" +
                    "Embedded Offline UI 2.0 RC5: extracted and validated\r\n" +
                    "README: present\r\n",
                    Encoding.UTF8);
                return 0;
            }
            catch (Exception exception)
            {
                try
                {
                    Directory.CreateDirectory(fullOutput);
                    File.WriteAllText(Path.Combine(fullOutput, "installer-self-test.txt"), "FAIL\r\n" + exception, Encoding.UTF8);
                }
                catch { }
                return 1;
            }
        }

        private static void WriteResourceToTest(string resourceName, string destination)
        {
            File.WriteAllBytes(destination, ReadResource(resourceName));
        }

        private static string Quote(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool MoveFileEx(string existingFileName, string newFileName, int flags);
    }
}
