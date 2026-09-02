using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace OpenEsxStudio.Desktop
{
    internal sealed class NativeBridge : IDisposable
    {
        private const int MaximumHeaderBytes = 65536;
        private const int MaximumEsxBytes = 64 * 1024 * 1024;
        private const int MinimumEsxBytes = 0x250010;
        private const long TwoGiB = 2L * 1024L * 1024L * 1024L;
        private const long ThirtyTwoGiB = 32L * 1024L * 1024L * 1024L;
        private readonly TcpListener listener;
        private readonly Thread serverThread;
        private readonly string html;
        private volatile bool running;

        internal NativeBridge(string htmlPath)
        {
            if (string.IsNullOrWhiteSpace(htmlPath) || !File.Exists(htmlPath))
                throw new FileNotFoundException("Die Offline-Oberfläche wurde nicht gefunden.", htmlPath);

            Token = CreateToken();
            string template = File.ReadAllText(htmlPath, Encoding.UTF8);
            const string marker = "<!--OPENESX_NATIVE_BRIDGE-->";
            if (!template.Contains(marker))
                throw new InvalidDataException("Die Offline-Oberfläche enthält keine Windows-Schnittstelle.");
            html = template.Replace(marker, "<script>globalThis.__OPENESX_NATIVE_TOKEN__='" + Token + "';</script>");

            listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start(8);
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            AppUri = new Uri("http://127.0.0.1:" + port.ToString() + "/");
            running = true;
            serverThread = new Thread(ServerLoop);
            serverThread.IsBackground = true;
            serverThread.Name = "OpenESX local card bridge";
            serverThread.Start();
        }

        internal Uri AppUri { get; private set; }
        internal string Token { get; private set; }

        private static string CreateToken()
        {
            byte[] token = new byte[32];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
                random.GetBytes(token);
            StringBuilder result = new StringBuilder(token.Length * 2);
            foreach (byte value in token)
                result.Append(value.ToString("x2"));
            return result.ToString();
        }

        private void ServerLoop()
        {
            while (running)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(delegate(object ignored) { HandleClient(client); });
                }
                catch (SocketException)
                {
                    if (running)
                        Thread.Sleep(50);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                try
                {
                    client.ReceiveTimeout = 45000;
                    client.SendTimeout = 45000;
                    Request request = ReadRequest(stream);
                    Route(stream, request);
                }
                catch (RequestException exception)
                {
                    try { SendJson(stream, exception.StatusCode, "{\"error\":" + JsonString(exception.Message) + "}"); }
                    catch { }
                }
                catch (Exception exception)
                {
                    try { SendJson(stream, 500, "{\"error\":" + JsonString("Windows konnte die Kartenaktion nicht abschließen: " + exception.Message) + "}"); }
                    catch { }
                }
            }
        }

        private static Request ReadRequest(Stream stream)
        {
            MemoryStream headerBuffer = new MemoryStream();
            int matched = 0;
            byte[] delimiter = new byte[] { 13, 10, 13, 10 };
            while (matched < delimiter.Length)
            {
                int value = stream.ReadByte();
                if (value < 0)
                    throw new RequestException(400, "Die lokale Windows-Anfrage war unvollständig.");
                headerBuffer.WriteByte((byte)value);
                if (value == delimiter[matched])
                    matched++;
                else
                    matched = value == delimiter[0] ? 1 : 0;
                if (headerBuffer.Length > MaximumHeaderBytes)
                    throw new RequestException(431, "Die lokale Windows-Anfrage war zu groß.");
            }

            string headerText = Encoding.ASCII.GetString(headerBuffer.ToArray());
            string[] lines = headerText.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            string[] first = lines[0].Split(' ');
            if (first.Length < 2)
                throw new RequestException(400, "Ungültige lokale Windows-Anfrage.");
            Request request = new Request();
            request.Method = first[0].ToUpperInvariant();
            request.Target = first[1];
            request.Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 1; index < lines.Length; index++)
            {
                int separator = lines[index].IndexOf(':');
                if (separator <= 0)
                    continue;
                request.Headers[lines[index].Substring(0, separator).Trim()] = lines[index].Substring(separator + 1).Trim();
            }

            int contentLength = 0;
            string lengthText;
            if (request.Headers.TryGetValue("Content-Length", out lengthText) && (!Int32.TryParse(lengthText, out contentLength) || contentLength < 0 || contentLength > MaximumEsxBytes))
                throw new RequestException(413, "Die ESX-Datei ist für den Karten-Manager zu groß.");
            request.Body = new byte[contentLength];
            int offset = 0;
            while (offset < contentLength)
            {
                int read = stream.Read(request.Body, offset, contentLength - offset);
                if (read <= 0)
                    throw new RequestException(400, "Die ESX-Datei wurde nicht vollständig übertragen.");
                offset += read;
            }
            return request;
        }

        private void Route(Stream stream, Request request)
        {
            Uri target;
            if (!Uri.TryCreate(AppUri, request.Target, out target))
                throw new RequestException(400, "Ungültige lokale Adresse.");
            string path = target.AbsolutePath;

            if (request.Method == "GET" && path == "/")
            {
                SendBytes(stream, 200, "text/html; charset=utf-8", Encoding.UTF8.GetBytes(html), null);
                return;
            }
            if (request.Method == "GET" && path == "/favicon.ico")
            {
                SendBytes(stream, 204, "image/x-icon", new byte[0], null);
                return;
            }
            if (!path.StartsWith("/api/", StringComparison.Ordinal))
                throw new RequestException(404, "Die lokale Funktion wurde nicht gefunden.");

            string suppliedToken;
            if (!request.Headers.TryGetValue("X-OpenESX-Token", out suppliedToken) || !FixedTimeEquals(Token, suppliedToken))
                throw new RequestException(403, "Die sichere Windows-Verbindung wurde abgelehnt.");

            string[] segments = path.Trim('/').Split('/');
            if (request.Method == "GET" && segments.Length == 2 && segments[0] == "api" && segments[1] == "cards")
            {
                SendJson(stream, 200, CardsJson());
                return;
            }
            if (segments.Length == 4 && segments[0] == "api" && segments[1] == "cards")
            {
                DriveInfo drive = ResolveDrive(segments[2]);
                if (request.Method == "GET" && segments[3] == "banks")
                {
                    SendJson(stream, 200, BanksJson(drive));
                    return;
                }
                if (request.Method == "GET" && segments[3] == "bank")
                {
                    string name = RequireSafeEsxName(GetQueryValue(target.Query, "name"));
                    string filePath = SafeCardPath(drive, name);
                    if (!File.Exists(filePath))
                        throw new RequestException(404, "Die ESX-Bank wurde auf der Karte nicht gefunden.");
                    SendBytes(stream, 200, "application/octet-stream", File.ReadAllBytes(filePath), "attachment; filename=\"" + name.Replace("\"", "") + "\"");
                    return;
                }
                if (request.Method == "POST" && segments[3] == "banks")
                {
                    string name = RequireSafeEsxName(GetQueryValue(target.Query, "name"));
                    bool overwrite = string.Equals(GetQueryValue(target.Query, "overwrite"), "1", StringComparison.Ordinal);
                    SaveBank(drive, name, request.Body, overwrite);
                    SendJson(stream, 200, "{\"saved\":true,\"name\":" + JsonString(name) + ",\"bytes\":" + request.Body.Length.ToString() + "}");
                    return;
                }
            }
            throw new RequestException(404, "Die lokale Funktion wurde nicht gefunden.");
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;
            int difference = 0;
            for (int index = 0; index < left.Length; index++)
                difference |= left[index] ^ right[index];
            return difference == 0;
        }

        private static List<DriveInfo> GetCardDrives()
        {
            List<DriveInfo> result = new List<DriveInfo>();
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                try
                {
                    if (!drive.IsReady)
                        continue;
                    if (drive.DriveType == DriveType.Removable)
                        result.Add(drive);
                }
                catch { }
            }
            result.Sort(delegate(DriveInfo left, DriveInfo right)
            {
                int type = left.DriveType.CompareTo(right.DriveType);
                return type != 0 ? type : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            });
            return result;
        }

        private static string CardsJson()
        {
            StringBuilder json = new StringBuilder("{\"cards\":[");
            bool first = true;
            foreach (DriveInfo drive in GetCardDrives())
            {
                try
                {
                    if (!first)
                        json.Append(',');
                    first = false;
                    string label = string.IsNullOrWhiteSpace(drive.VolumeLabel) ? "Wechseldatenträger" : drive.VolumeLabel;
                    int fileCount = CountRootFiles(drive);
                    int esxCount = Directory.GetFiles(drive.RootDirectory.FullName, "*.esx", SearchOption.TopDirectoryOnly).Length;
                    bool compatible = drive.TotalSize <= ThirtyTwoGiB;
                    string status = drive.TotalSize <= TwoGiB ? "SD · bis 2 GB" : compatible ? "SDHC · bis 32 GB" : "über 32 GB";
                    json.Append('{');
                    AppendProperty(json, "id", EncodeDriveId(drive.RootDirectory.FullName), true);
                    AppendProperty(json, "root", drive.RootDirectory.FullName, false);
                    AppendProperty(json, "label", label, false);
                    AppendProperty(json, "driveType", DriveTypeName(drive.DriveType), false);
                    AppendProperty(json, "format", SafeDriveFormat(drive), false);
                    AppendNumber(json, "totalBytes", drive.TotalSize, false);
                    AppendNumber(json, "freeBytes", drive.AvailableFreeSpace, false);
                    AppendNumber(json, "fileCount", fileCount, false);
                    AppendNumber(json, "esxFileCount", esxCount, false);
                    AppendBoolean(json, "compatible", compatible, false);
                    AppendProperty(json, "capacityStatus", status, false);
                    json.Append('}');
                }
                catch { }
            }
            json.Append("]}");
            return json.ToString();
        }

        private static string BanksJson(DriveInfo drive)
        {
            string[] files = Directory.GetFiles(drive.RootDirectory.FullName, "*.esx", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            StringBuilder json = new StringBuilder("{\"banks\":[");
            for (int index = 0; index < files.Length; index++)
            {
                if (index > 0)
                    json.Append(',');
                FileInfo file = new FileInfo(files[index]);
                json.Append('{');
                AppendProperty(json, "name", file.Name, true);
                AppendNumber(json, "size", file.Length, false);
                AppendProperty(json, "lastWriteLocal", file.LastWriteTime.ToString("dd.MM.yyyy HH:mm"), false);
                json.Append('}');
            }
            json.Append("]}");
            return json.ToString();
        }

        private static int CountRootFiles(DriveInfo drive)
        {
            try { return Directory.GetFiles(drive.RootDirectory.FullName, "*", SearchOption.TopDirectoryOnly).Length; }
            catch { return 0; }
        }

        private static string SafeDriveFormat(DriveInfo drive)
        {
            try { return drive.DriveFormat; }
            catch { return ""; }
        }

        private static string DriveTypeName(DriveType type)
        {
            if (type == DriveType.Removable)
                return "Wechseldatenträger";
            return type.ToString();
        }

        private static string EncodeDriveId(string root)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(root)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static DriveInfo ResolveDrive(string encodedId)
        {
            foreach (DriveInfo drive in GetCardDrives())
            {
                if (string.Equals(EncodeDriveId(drive.RootDirectory.FullName), encodedId, StringComparison.Ordinal))
                    return drive;
            }
            throw new RequestException(404, "Die Speicherkarte ist nicht mehr angeschlossen oder nicht bereit.");
        }

        private static string RequireSafeEsxName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new RequestException(400, "Bitte gib einen Dateinamen für die ESX-Bank ein.");
            if (!name.EndsWith(".esx", StringComparison.OrdinalIgnoreCase))
                throw new RequestException(400, "Der Bankname muss mit .esx enden.");
            if (!string.Equals(Path.GetFileName(name), name, StringComparison.Ordinal) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new RequestException(400, "Der Dateiname der ESX-Bank ist ungültig.");
            return name;
        }

        private static string SafeCardPath(DriveInfo drive, string name)
        {
            string root = Path.GetFullPath(drive.RootDirectory.FullName).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string fullPath = Path.GetFullPath(Path.Combine(root, name));
            if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new RequestException(400, "Der Zielpfad liegt nicht auf der gewählten Speicherkarte.");
            return fullPath;
        }

        private static void SaveBank(DriveInfo drive, string name, byte[] bytes, bool overwrite)
        {
            if (!IsEsx(bytes))
                throw new RequestException(400, "Die Arbeitskopie besitzt keine gültige KORG-ESX-1-Signatur.");
            string destination = SafeCardPath(drive, name);
            bool exists = File.Exists(destination);
            if (exists && !overwrite)
                throw new RequestException(409, "Diese ESX-Bank existiert bereits.");
            if (!exists && CountRootFiles(drive) >= 256)
                throw new RequestException(507, "Die Karte enthält bereits 256 Dateien. Die Korg ignoriert weitere Dateien; wähle einen bestehenden Banknamen zum Überschreiben.");
            if (drive.AvailableFreeSpace < bytes.LongLength)
                throw new RequestException(507, "Auf der Speicherkarte ist nicht genug freier Platz für die sicher geschriebene ESX-Arbeitskopie.");

            string temporary = Path.Combine(drive.RootDirectory.FullName, ".openesx-" + Guid.NewGuid().ToString("N") + ".tmp");
            string displaced = Path.Combine(drive.RootDirectory.FullName, ".openesx-" + Guid.NewGuid().ToString("N") + ".old");
            try
            {
                using (FileStream output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 1024, FileOptions.WriteThrough))
                {
                    output.Write(bytes, 0, bytes.Length);
                    output.Flush();
                }
                byte[] written = File.ReadAllBytes(temporary);
                if (!IsEsx(written) || !ByteArraysEqual(bytes, written))
                    throw new IOException("Die bitgenaue Prüfung der geschriebenen ESX-Datei ist fehlgeschlagen.");
                if (exists)
                    File.Move(destination, displaced);
                try
                {
                    File.Move(temporary, destination);
                }
                catch
                {
                    if (exists && File.Exists(displaced) && !File.Exists(destination))
                        File.Move(displaced, destination);
                    throw;
                }
                if (File.Exists(displaced))
                    File.Delete(displaced);
            }
            finally
            {
                if (File.Exists(temporary))
                    File.Delete(temporary);
                if (File.Exists(displaced) && !File.Exists(destination))
                    File.Move(displaced, destination);
                else if (File.Exists(displaced) && File.Exists(destination))
                {
                    try { File.Delete(displaced); }
                    catch { }
                }
            }
        }

        private static bool ByteArraysEqual(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;
            int difference = 0;
            for (int index = 0; index < left.Length; index++)
                difference |= left[index] ^ right[index];
            return difference == 0;
        }

        private static bool IsEsx(byte[] bytes)
        {
            return bytes != null && bytes.Length >= MinimumEsxBytes && bytes.Length <= MaximumEsxBytes &&
                   bytes[0] == (byte)'K' && bytes[1] == (byte)'O' && bytes[2] == (byte)'R' && bytes[3] == (byte)'G' &&
                   bytes[7] == 0x71 && bytes[8] == (byte)'E' && bytes[9] == (byte)'S' && bytes[10] == (byte)'X';
        }

        private static string GetQueryValue(string query, string key)
        {
            if (string.IsNullOrEmpty(query))
                return "";
            string[] pairs = query.TrimStart('?').Split('&');
            foreach (string pair in pairs)
            {
                int separator = pair.IndexOf('=');
                string name = separator < 0 ? pair : pair.Substring(0, separator);
                if (string.Equals(DecodeUrl(name), key, StringComparison.OrdinalIgnoreCase))
                    return DecodeUrl(separator < 0 ? "" : pair.Substring(separator + 1));
            }
            return "";
        }

        private static string DecodeUrl(string value)
        {
            return Uri.UnescapeDataString((value ?? "").Replace('+', ' '));
        }

        private static void AppendProperty(StringBuilder json, string name, string value, bool first)
        {
            if (!first)
                json.Append(',');
            json.Append(JsonString(name)).Append(':').Append(JsonString(value));
        }

        private static void AppendNumber(StringBuilder json, string name, long value, bool first)
        {
            if (!first)
                json.Append(',');
            json.Append(JsonString(name)).Append(':').Append(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        private static void AppendBoolean(StringBuilder json, string name, bool value, bool first)
        {
            if (!first)
                json.Append(',');
            json.Append(JsonString(name)).Append(':').Append(value ? "true" : "false");
        }

        private static string JsonString(string value)
        {
            if (value == null)
                return "null";
            StringBuilder result = new StringBuilder("\"");
            foreach (char character in value)
            {
                switch (character)
                {
                    case '\\': result.Append("\\\\"); break;
                    case '\"': result.Append("\\\""); break;
                    case '\r': result.Append("\\r"); break;
                    case '\n': result.Append("\\n"); break;
                    case '\t': result.Append("\\t"); break;
                    default:
                        if (character < 32)
                            result.Append("\\u").Append(((int)character).ToString("x4"));
                        else
                            result.Append(character);
                        break;
                }
            }
            return result.Append('\"').ToString();
        }

        private static void SendJson(Stream stream, int statusCode, string json)
        {
            SendBytes(stream, statusCode, "application/json; charset=utf-8", Encoding.UTF8.GetBytes(json), null);
        }

        private static void SendBytes(Stream stream, int statusCode, string contentType, byte[] bytes, string contentDisposition)
        {
            string reason = ReasonPhrase(statusCode);
            StringBuilder header = new StringBuilder();
            header.Append("HTTP/1.1 ").Append(statusCode).Append(' ').Append(reason).Append("\r\n");
            header.Append("Content-Type: ").Append(contentType).Append("\r\n");
            header.Append("Content-Length: ").Append(bytes.Length).Append("\r\n");
            header.Append("Cache-Control: no-store\r\n");
            header.Append("X-Content-Type-Options: nosniff\r\n");
            header.Append("Referrer-Policy: no-referrer\r\n");
            if (!string.IsNullOrEmpty(contentDisposition))
                header.Append("Content-Disposition: ").Append(contentDisposition).Append("\r\n");
            header.Append("Connection: close\r\n\r\n");
            byte[] headerBytes = Encoding.ASCII.GetBytes(header.ToString());
            stream.Write(headerBytes, 0, headerBytes.Length);
            if (bytes.Length > 0)
                stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        private static string ReasonPhrase(int statusCode)
        {
            switch (statusCode)
            {
                case 200: return "OK";
                case 204: return "No Content";
                case 400: return "Bad Request";
                case 403: return "Forbidden";
                case 404: return "Not Found";
                case 409: return "Conflict";
                case 413: return "Payload Too Large";
                case 431: return "Request Header Fields Too Large";
                case 500: return "Internal Server Error";
                case 507: return "Insufficient Storage";
                default: return "Error";
            }
        }

        public void Dispose()
        {
            running = false;
            try { listener.Stop(); }
            catch { }
            if (serverThread != null && serverThread.IsAlive)
                serverThread.Join(1000);
        }

        private sealed class Request
        {
            internal string Method;
            internal string Target;
            internal Dictionary<string, string> Headers;
            internal byte[] Body;
        }

        private sealed class RequestException : Exception
        {
            internal RequestException(int statusCode, string message) : base(message)
            {
                StatusCode = statusCode;
            }

            internal int StatusCode { get; private set; }
        }
    }
}
