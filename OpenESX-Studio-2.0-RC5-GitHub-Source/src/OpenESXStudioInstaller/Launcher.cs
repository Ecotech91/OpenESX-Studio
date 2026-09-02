using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("OpenESX Studio")]
[assembly: AssemblyDescription("Lokaler Korg ESX-1 Sample- und Pattern-Editor")]
[assembly: AssemblyCompany("OpenESX Studio")]
[assembly: AssemblyProduct("OpenESX Studio")]
[assembly: AssemblyCopyright("Copyright (c) 2026")]
[assembly: AssemblyVersion("2.0.5.0")]
[assembly: AssemblyFileVersion("2.0.5.0")]

namespace OpenEsxStudio.Desktop
{
    internal static class Launcher
    {
        private const string HtmlResource = "OpenESXStudio.Offline.html";
        private const string HtmlFileName = "OpenESX-Studio-Offline.html";

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                if (args.Length >= 2 && string.Equals(args[0], "--self-test", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.ExitCode = RunSelfTest(args[1]);
                    return;
                }

                string runtimeDirectory = GetRuntimeDirectory();
                string htmlPath = EnsureRuntimeHtml(runtimeDirectory);
                using (NativeBridge bridge = new NativeBridge(htmlPath))
                {
                    Process application = OpenApplicationWindow(bridge.AppUri, htmlPath, runtimeDirectory);
                    if (application != null)
                        application.WaitForExit();
                }
            }
            catch (Exception exception)
            {
                LogError(exception);
                MessageBox.Show(
                    "OpenESX Studio konnte nicht gestartet werden.\r\n\r\n" +
                    exception.Message + "\r\n\r\n" +
                    "Ein Fehlerprotokoll wurde unter %LOCALAPPDATA%\\OpenESX Studio gespeichert.",
                    "OpenESX Studio",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.ExitCode = 1;
            }
        }

        private static string GetRuntimeDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "OpenESX Studio",
                "Runtime");
        }

        private static string EnsureRuntimeHtml(string runtimeDirectory)
        {
            Directory.CreateDirectory(runtimeDirectory);
            string destination = Path.Combine(runtimeDirectory, HtmlFileName);
            byte[] embedded = ReadResource(HtmlResource);

            if (File.Exists(destination))
            {
                byte[] existing = File.ReadAllBytes(destination);
                if (FixedTimeEquals(ComputeSha256(existing), ComputeSha256(embedded)))
                    return destination;
            }

            string temporary = destination + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllBytes(temporary, embedded);
                if (File.Exists(destination))
                    File.Delete(destination);
                File.Move(temporary, destination);
            }
            finally
            {
                if (File.Exists(temporary))
                    File.Delete(temporary);
            }

            return destination;
        }

        private static byte[] ReadResource(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException("Die eingebettete Offline-Oberfläche fehlt.");
                using (MemoryStream output = new MemoryStream())
                {
                    stream.CopyTo(output);
                    return output.ToArray();
                }
            }
        }

        private static byte[] ComputeSha256(byte[] bytes)
        {
            using (SHA256 algorithm = SHA256.Create())
                return algorithm.ComputeHash(bytes);
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;
            int difference = 0;
            for (int index = 0; index < left.Length; index++)
                difference |= left[index] ^ right[index];
            return difference == 0;
        }

        private static Process OpenApplicationWindow(Uri appUri, string htmlPath, string runtimeDirectory)
        {
            string edgePath = FindMicrosoftEdge();
            if (!string.IsNullOrEmpty(edgePath))
            {
                string profileDirectory = Path.Combine(runtimeDirectory, "EdgeProfile");
                Directory.CreateDirectory(profileDirectory);
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = edgePath;
                start.Arguments = "--app=" + Quote(appUri.AbsoluteUri) + " --user-data-dir=" + Quote(profileDirectory) + " --no-first-run --disable-first-run-ui --disable-background-mode --start-maximized";
                start.WorkingDirectory = Path.GetDirectoryName(htmlPath);
                start.UseShellExecute = false;
                Process process = Process.Start(start);
                if (process == null)
                    throw new InvalidOperationException("Microsoft Edge konnte nicht gestartet werden.");
                return process;
            }

            ProcessStartInfo fallback = new ProcessStartInfo();
            fallback.FileName = htmlPath;
            fallback.UseShellExecute = true;
            Process fallbackProcess = Process.Start(fallback);
            if (fallbackProcess == null)
                throw new InvalidOperationException("Für die Offline-Oberfläche wurde kein geeigneter Browser gefunden.");
            return fallbackProcess;
        }

        private static string FindMicrosoftEdge()
        {
            string registryPath = ReadAppPath(Registry.CurrentUser);
            if (!string.IsNullOrEmpty(registryPath))
                return registryPath;
            registryPath = ReadAppPath(Registry.LocalMachine);
            if (!string.IsNullOrEmpty(registryPath))
                return registryPath;

            string[] candidates = new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "Application", "msedge.exe")
            };
            foreach (string candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
                    return candidate;
            }
            return null;
        }

        private static string ReadAppPath(RegistryKey root)
        {
            try
            {
                using (RegistryKey key = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe"))
                {
                    string value = key == null ? null : key.GetValue(null) as string;
                    return !string.IsNullOrWhiteSpace(value) && File.Exists(value) ? value : null;
                }
            }
            catch
            {
                return null;
            }
        }

        private static string Quote(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private static int RunSelfTest(string outputDirectory)
        {
            try
            {
                string fullOutput = Path.GetFullPath(outputDirectory);
                Directory.CreateDirectory(fullOutput);
                string htmlPath = EnsureRuntimeHtml(fullOutput);
                string html = File.ReadAllText(htmlPath, Encoding.UTF8);
                if (!html.Contains("OpenESX Studio Offline 2.0 RC5"))
                    throw new InvalidDataException("Falsche Offline-Version eingebettet.");
                if (!html.Contains("data-action=\"part-sample-preview\"") || !html.Contains("data-bar=\"${bar}\"") || !html.Contains("data-preview-part=\"${index}\"") || !html.Contains("data-action=\"preview-all-off\""))
                    throw new InvalidDataException("Pattern-Studio-Funktionen fehlen in der eingebetteten Oberfläche.");
                if (!html.Contains("data-tab=\"cards\"") || !html.Contains("data-action=\"card-save\"") || !html.Contains("data-action=\"card-open\""))
                    throw new InvalidDataException("Der Karten- und Bank-Manager fehlt in der eingebetteten Oberfläche.");
                using (NativeBridge bridge = new NativeBridge(htmlPath))
                using (WebClient client = new WebClient())
                {
                    string servedHtml = client.DownloadString(bridge.AppUri);
                    if (!servedHtml.Contains(bridge.Token) || servedHtml.Contains("<!--OPENESX_NATIVE_BRIDGE-->"))
                        throw new InvalidDataException("Die sichere Windows-Verbindung wurde nicht in die Oberfläche eingebettet.");
                    client.Headers.Add("X-OpenESX-Token", bridge.Token);
                    string cardsJson = client.DownloadString(new Uri(bridge.AppUri, "api/cards"));
                    if (!cardsJson.Contains("\"cards\""))
                        throw new InvalidDataException("Die Windows-Laufwerkerkennung antwortet nicht.");
                }
                File.WriteAllText(
                    Path.Combine(fullOutput, "launcher-self-test.txt"),
                    "OpenESX Studio Desktop self-test: PASS\r\n" +
                    "Embedded UI: 2.0 RC5\r\n" +
                    "Pattern bars, sample preview and live preview mixer: present\r\n" +
                    "Card manager and protected local Windows bridge: present\r\n" +
                    "HTML SHA-256: " + ToHex(ComputeSha256(File.ReadAllBytes(htmlPath))) + "\r\n",
                    Encoding.UTF8);
                return 0;
            }
            catch (Exception exception)
            {
                try
                {
                    Directory.CreateDirectory(outputDirectory);
                    File.WriteAllText(Path.Combine(outputDirectory, "launcher-self-test.txt"), "FAIL\r\n" + exception, Encoding.UTF8);
                }
                catch { }
                return 1;
            }
        }

        private static string ToHex(byte[] bytes)
        {
            StringBuilder output = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes)
                output.Append(value.ToString("X2"));
            return output.ToString();
        }

        private static void LogError(Exception exception)
        {
            try
            {
                string directory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "OpenESX Studio");
                Directory.CreateDirectory(directory);
                File.AppendAllText(
                    Path.Combine(directory, "desktop-error.log"),
                    DateTime.Now.ToString("u") + Environment.NewLine + exception + Environment.NewLine + new string('-', 72) + Environment.NewLine,
                    Encoding.UTF8);
            }
            catch { }
        }
    }
}
