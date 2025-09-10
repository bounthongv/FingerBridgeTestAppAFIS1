using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySqlConnector;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using FingerBridge.Native;
using System.Linq;
using System.Threading;
using SourceAFIS.Engine;
using SourceAFIS;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;




using System.ComponentModel;
using System.Xml;

/// <summary>
/// Main application form for the Fingerprint Bridge Test Application.
/// Provides a graphical interface for fingerprint capture, verification, and device management.
/// Supports both manual operation through UI controls and automated operation through network commands.
/// </summary>
public class FingerprintGuiApp : Form
{
    /// <summary>Text box for entering the person's identification number.</summary>
    private TextBox personIdTextBox;
    /// <summary>Dropdown for selecting which finger to capture (1-10).</summary>
    private ComboBox fingerSelector;
    /// <summary>Button to initiate fingerprint capture.</summary>
    private Button captureButton;
    /// <summary>Button to capture and verify a fingerprint against stored template.</summary>
    private Button captureAndVerifyButton;
    /// <summary>Checkbox to toggle verification mode.</summary>
    private CheckBox verificationModeCheckbox;
    /// <summary>Picture box for displaying fingerprint preview.</summary>
    private PictureBox previewBox;
    /// <summary>Label for displaying status messages to the user.</summary>
    private Label statusLabel;

    /// <summary>Button to perform fingerprint matching operations.</summary>
    private Button matchButton;
    private Button testConnectionButton;

    /// <summary>Handle to the connected fingerprint device.</summary>
    private IntPtr handle = IntPtr.Zero;

    /// <summary>System tray icon for the application.</summary>
    private NotifyIcon trayIcon;
    /// <summary>Context menu for the system tray icon.</summary>
    private ContextMenuStrip trayMenu;
    /// <summary>Button to connect to the fingerprint device.</summary>
    private Button? connectButton;

    /// <summary>
    /// Handles changes to the verification mode checkbox state.
    /// Updates the UI button states based on the current verification mode.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void VerificationModeCheckbox_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateButtonStates();
    }



    // private void StartSocketListener()
    // {
    //     new Thread(() =>
    //     {
    //         TcpListener listener = new TcpListener(IPAddress.Loopback, 8123);
    //         listener.Start();
    //         while (true)
    //         {
    //             TcpClient client = listener.AcceptTcpClient();
    //             new Thread(() =>
    //             {
    //                 using var stream = client.GetStream();
    //                 using var reader = new StreamReader(stream);
    //                 using var writer = new StreamWriter(stream) { AutoFlush = true };

    //                 string? line = reader.ReadLine();
    //                 if (line != null && line.StartsWith("CAPTURE"))
    //                 {
    //                     string[] parts = line.Split(' ');
    //                     if (parts.Length == 3)
    //                     {
    //                         string personId = parts[1];
    //                         int fingerIndex = int.Parse(parts[2]);
    //                         this.Invoke((MethodInvoker)(() => CaptureAndSave(personId, fingerIndex)));
    //                         writer.WriteLine("OK");
    //                     }
    //                     else
    //                     {
    //                         writer.WriteLine("Invalid command format");
    //                     }
    //                 }
    //                 else
    //                 {
    //                     writer.WriteLine("Unknown command");
    //                 }
    //             }).Start();
    //         }
    //     }).Start();
    // }


    private IntPtr handle2 = IntPtr.Zero;
    /// <summary>
    /// Initializes the fingerprint scanning device and its SDK.
    /// Attempts to connect to the device if available and sets up the appropriate UI state.
    /// </summary>
    /// <returns>True if device initialization was successful, false otherwise.</returns>
    private bool InitializeDevice()
    {
        string exePath = Environment.ProcessPath ?? string.Empty;
        string exeDir = Path.GetDirectoryName(exePath) ?? string.Empty;
        string libDir = exeDir;

        if (!Directory.Exists(libDir))
        {
            Console.WriteLine("[Init] Library directory not found.");
            statusLabel.Text = "Library folder missing.";
            return false;
        }

        var initResult = TrustFingerNative.ARAFPSCAN_GlobalInit();

        if (initResult == -115) // SDK already initialized
        {
            int count = 0;
            TrustFingerNative.ARAFPSCAN_GetDeviceCount(ref count);

            if (count > 0)
            {
                Console.WriteLine("[Init] Device is already connected.");
                statusLabel.Text = "Device is already connected.";
                connectButton!.Enabled = false;
                EnableControlsAfterConnected();
                return true;
            }
            else
            {
                Console.WriteLine("[Init] SDK ready, but no device plugged in.");
                statusLabel.Text = "No fingerprint device found.";
                connectButton!.Enabled = true;
                DisableControlsUntilConnected();
                return false;
            }
        }
        else if (initResult != 0)
        {
            Console.WriteLine($"[Init] SDK init failed (error {initResult})");
            statusLabel.Text = $"SDK init failed (code {initResult})";
            connectButton!.Enabled = true;
            return false;
        }

        // SDK initialized successfully, now look for devices
        int devCount = 0;
        TrustFingerNative.ARAFPSCAN_GetDeviceCount(ref devCount);

        if (devCount > 0)
        {
            var openResult = TrustFingerNative.ARAFPSCAN_OpenDevice(ref handle, 0);
            if (openResult == 0)
            {
                Console.WriteLine("[Init] Device connected successfully.");
                statusLabel.Text = "Device connected successfully.";
                connectButton!.Enabled = false;
                EnableControlsAfterConnected();
                return true;
            }
            else
            {
                Console.WriteLine("[Init] Device found, but failed to open.");
                statusLabel.Text = "Device found, but failed to open.";
                connectButton!.Enabled = true;
                return false;
            }
        }
        else
        {
            Console.WriteLine("[Init] No fingerprint device found.");
            statusLabel.Text = "No fingerprint device found.";
            connectButton!.Enabled = true;
            DisableControlsUntilConnected();
            return false;
        }
    }

    


/// <summary>
/// Initializes a new instance of the FingerprintGuiApp form.
/// Sets up the UI components, system tray icon, and attempts to initialize the fingerprint device.
/// </summary>
public FingerprintGuiApp()
{
    // Add DLL directory to the search path to help load native DLLs
    string exePath = Environment.ProcessPath ?? string.Empty;
    string exeDir = System.IO.Path.GetDirectoryName(exePath) ?? string.Empty;
    if (!string.IsNullOrEmpty(exeDir))
    {
        SetDllDirectory(exeDir);
    }

    Text = "Fingerprint Capture & Verification App";
    Width = 500;
    Height = 480;

    // Set the form icon to fingerprint.ico
    this.Icon = new Icon("fingerprint.ico");

    // Initialize tray icon and menu
    trayMenu = new ContextMenuStrip();
    trayMenu.Items.Add("Open", null, OnTrayOpenClicked);
    trayMenu.Items.Add("Exit", null, OnTrayExitClicked);
    trayMenu.Items.Add(new ToolStripSeparator()); // Separator
    trayMenu.Items.Add("About", null, OnTrayAboutClicked); // New About menu item

    trayIcon = new NotifyIcon();
    trayIcon.Text = "Fingerprint Capture & Verification App";
    trayIcon.Icon = new Icon("fingerprint.ico");
    trayIcon.ContextMenuStrip = trayMenu;
    trayIcon.Visible = true;
    trayIcon.DoubleClick += OnTrayIconDoubleClick;

    // Person ID input
    var personIdLabel = new Label { Text = "Person ID:", Left = 30, Top = 60, Width = 70 };
    personIdTextBox = new TextBox { Left = 100, Top = 60, Width = 120 };

    fingerSelector = new ComboBox { Left = 30, Top = 90, Width = 200 };
    for (int i = 1; i <= 10; i++)
        fingerSelector.Items.Add(FingerIndexToString(i));
    fingerSelector.SelectedIndex = 0;

    verificationModeCheckbox = new CheckBox { Text = "Verification Mode", Left = 250, Top = 60, Width = 180 };
    verificationModeCheckbox.CheckedChanged += VerificationModeCheckbox_CheckedChanged;

    captureButton = new Button { Text = "Capture Fingerprint", Left = 250, Top = 90, Width = 180, Visible = true };
    captureButton.Click += CaptureButton_Click;

    captureAndVerifyButton = new Button { Text = "Capture and Verify Finger", Left = 250, Top = 120, Width = 180, Visible = true };
    captureAndVerifyButton.Click += CaptureAndVerifyButton_Click;

    // Add this in constructor after other buttons
    matchButton = new Button
    {
        Text = "Capture and Match",
        Left = 250,
        Top = 150,
        Width = 180,
        Visible = true
    };
    matchButton.Click += (s, e) => MatchAndIdentify();

    // New Connect to Device button
    connectButton = new Button
    {
        Text = "Connect to Device",
        Left = 30,
        Top = 20,
        Width = 150,
        Height = 25
    };
    connectButton.Click += ConnectButton_Click;
    Controls.Add(connectButton);

    testConnectionButton = new Button
    {
        Text = "Test DB Connection",
        Width = 150,
        Height = 30,
        Top = 10,
        Left = this.ClientSize.Width - 160, // 10px from right
        Anchor = AnchorStyles.Top | AnchorStyles.Right
    };
    testConnectionButton.Click += TestConnectionButton_Click;
    Controls.Add(testConnectionButton);

    previewBox = new PictureBox { Left = 30, Top = 170, Width = 320, Height = 240, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
    statusLabel = new Label { Left = 30, Top = 420, Width = 400 };

    
    // Controls.Add(connectButton);
    Controls.Add(personIdLabel);
    Controls.Add(personIdTextBox);
    Controls.Add(fingerSelector);
    Controls.Add(verificationModeCheckbox);
    Controls.Add(captureButton);
    Controls.Add(captureAndVerifyButton);
    Controls.Add(previewBox);
    Controls.Add(statusLabel);
    Controls.Add(matchButton);
    

    // Remove immediate device initialization
        // InitializeDevice();

    DisableControlsUntilConnected();

        // StartSocketListener(); // ⬅ Add this at the end
    bool connected = InitializeDevice();  // attempt auto-connect

    if (!connected)
    {
        // Only enable the connect button; disable all others
        DisableControlsUntilConnected();
        connectButton!.Enabled = true;
    }
    else
    {
        connectButton!.Enabled = false;
        EnableControlsAfterConnected();
        UpdateButtonStates();
    }


    StartBridgeServer();

    // Start minimized to tray
    this.WindowState = FormWindowState.Minimized;
    this.ShowInTaskbar = false;
    this.Hide();
}
    
    private void TestConnectionButton_Click(object? sender, EventArgs e)
    {
        try
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.sys");
            var cfg = LoadConfig(configPath);

            string connStr = $"server={cfg["host"]};user={cfg["user"]};password={cfg["password"]};database={cfg["database"]}";

            using var conn = new MySqlConnection(connStr);
            conn.Open();

            Console.WriteLine("✅ Successfully connected to MySQL!");
            MessageBox.Show("✅ MySQL connection successful!", "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ MySQL connection failed: " + ex.Message);
            MessageBox.Show("❌ Connection failed:\n" + ex.Message, "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void StartBridgeServer()
    {
        Task.Run(() =>
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 8123);
            listener.Start();
            Console.WriteLine("🔌 Fingerprint bridge server started on port 8123");

            while (true)
            {
                try                {
                    using TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("📡 Received connection from: " + client.Client.RemoteEndPoint);

                    using NetworkStream stream = client.GetStream();
                    using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                    string? line = reader.ReadLine();
                    Console.WriteLine("➡️ Command received: " + line);

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        writer.WriteLine("ERROR Empty command");
                        continue;
                    }

                    string[] parts = line.Trim().Split();
                    string command = parts[0].ToUpperInvariant();

                    string response;

                    switch (command)
                    {
                        case "CAPTURE":
                            response = parts.Length == 4
                                ? RunCapture(parts[1], int.Parse(parts[2]), parts[3], writer)
                                : parts.Length == 3
                                    ? RunCapture(parts[1], int.Parse(parts[2]), "prisoner", writer) // backward compatibility
                                    : "ERROR Usage: CAPTURE <person_id> <finger_index> <member>";
                            break;

                        case "VERIFY":
                            response = parts.Length == 4
                                ? RunVerify(parts[1], int.Parse(parts[2]), parts[3], writer)
                                : parts.Length == 3
                                    ? RunVerify(parts[1], int.Parse(parts[2]), "prisoner", writer) // backward compatibility
                                    : "ERROR Usage: VERIFY <person_id> <finger_index> <member>";
                            break;

                        case "MATCH":
                            response = RunMatch(writer);
                            break;

                        default:
                            response = "ERROR Unknown command";
                            break;
                    }

                    Console.WriteLine("⬅️ Responding with: " + response);
                    writer.WriteLine(response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("🔥 Bridge error: " + ex.Message);
                }
            }
        });
    }

    private string RunCapture(string personId, int fingerIndex, string member, StreamWriter writer)
    {
        try
        {
            this.Invoke((MethodInvoker)(() =>
            {
                try
                {
                    statusLabel.Text = $"[RunCapture] Starting CaptureAndSave for {personId}, finger {fingerIndex}, member {member}";
                    CaptureAndSave(personId, fingerIndex, member, writer);  // ✅ use the overload with member
                    statusLabel.Text = $"[RunCapture] CaptureAndSave completed for {personId}, finger {fingerIndex}, member {member}";
                }
                catch (Exception ex)
                {
                    string err = "ERROR " + ex.Message;
                    writer.WriteLine(err);
                    writer.Flush();
                    statusLabel.Text = "[RunCapture] Exception: " + ex.Message;
                }
            }));
        }
        catch (Exception ex)
        {
            string err = "ERROR Invoke failed: " + ex.Message;
            writer.WriteLine(err);
            writer.Flush();
        }

        return ""; // response already sent via writer
    }






    private string RunVerify(string personId, int fingerIndex, string member, StreamWriter writer)
    {
        try
        {
            this.Invoke((MethodInvoker)(() =>
            {
                try
                {
                    Application.DoEvents(); // 👈 ensure UI updates
                    statusLabel.Text = $"[RunVerify] Starting CaptureAndVerify for {personId}, finger {fingerIndex}, member {member}";
                    string result = CaptureAndVerifyUsingMatch(personId, fingerIndex, member, writer);  // ✅ use new overload with member
                    statusLabel.Text = $"[RunVerify] Done. Result: {result}";
                    // result is already written through writer, no need to return it
                }
                catch (Exception ex)
                {
                    string err = "ERROR " + ex.Message;
                    writer.WriteLine(err);
                    writer.Flush();
                    statusLabel.Text = "[RunVerify] Exception: " + ex.Message;
                }
            }));
        }
        catch (Exception ex)
        {
            string err = "ERROR Invoke failed: " + ex.Message;
            writer.WriteLine(err);
            writer.Flush();
        }

        return ""; // response already sent through writer
    }


    private string RunMatch(StreamWriter writer)
    {
        try
        {
            this.Invoke((MethodInvoker)(() =>
            {
                try
                {
                    statusLabel.Text = "[RunMatch] Starting MatchAndIdentify";
                    string result = MatchAndIdentify(writer);  // ✅ use the overload
                    statusLabel.Text = $"[RunMatch] Done. Result: {result}";
                    // The result was already written to the writer (including BMP)
                }
                catch (Exception ex)
                {
                    string err = "ERROR " + ex.Message;
                    writer.WriteLine(err);
                    writer.Flush();
                    statusLabel.Text = "[RunMatch] Exception: " + ex.Message;
                }
            }));
        }
        catch (Exception ex)
        {
            string err = "ERROR Invoke failed: " + ex.Message;
            writer.WriteLine(err);
            writer.Flush();
        }

        return ""; // response already sent via writer
    }




    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);

    private void UpdateButtonStates()
    {
        bool isVerificationMode = verificationModeCheckbox.Checked;
        captureButton.Enabled = !isVerificationMode;
        captureAndVerifyButton.Enabled = isVerificationMode;
        matchButton.Enabled = isVerificationMode;
    }

    private void DisableControlsUntilConnected()
    {
        personIdTextBox.Enabled = false;
        fingerSelector.Enabled = false;
        verificationModeCheckbox.Enabled = false;
        captureButton.Enabled = false;
        captureAndVerifyButton.Enabled = false;
        captureAndVerifyButton.Visible = true; // Ensure visibility
        matchButton.Enabled = false;
        matchButton.Visible = true; // Ensure visibility
    }

    private void EnableControlsAfterConnected()
    {
        personIdTextBox.Enabled = true;
        fingerSelector.Enabled = true;
        verificationModeCheckbox.Enabled = true;
        captureButton.Enabled = true;
        captureAndVerifyButton.Enabled = true;
        matchButton.Enabled = true;
    }

    private void ConnectButton_Click(object? sender, EventArgs e)
    {
        Console.WriteLine("[UI] Manual connect button clicked.");
        statusLabel.Text = "Connecting to device...";
        Refresh();

        bool connected = InitializeDevice();
        if (connected)
        {
            statusLabel.Text = "Device connected successfully.";
            EnableControlsAfterConnected();
            connectButton!.Enabled = false;
            UpdateButtonStates();
        }
        else
        {
            statusLabel.Text = "Failed to connect to device.";
        }
    }


    private void CaptureButton_Click(object? sender, EventArgs e)
    {
        string personId = personIdTextBox.Text.Trim();
        int fingerIndex = fingerSelector.SelectedIndex + 1;

        if (string.IsNullOrEmpty(personId))
        {
            statusLabel.ForeColor = Color.Red;
            statusLabel.Text = "Please enter a Person ID.";
            Console.WriteLine("Validation failed: Person ID is required for capture.");
            personIdTextBox.Focus();
            return;
        }
        CaptureAndSave(personId, fingerIndex);
    }

    private void CaptureAndVerifyButton_Click(object? sender, EventArgs e)
    {
        string personId = personIdTextBox.Text.Trim();
        int fingerIndex = fingerSelector.SelectedIndex + 1;

        if (string.IsNullOrEmpty(personId))
        {
            statusLabel.ForeColor = Color.Red;
            statusLabel.Text = "Please enter a Person ID for verification.";
            Console.WriteLine("Validation failed: Person ID is required for verification.");
            personIdTextBox.Focus();
            return;
        }

        CaptureAndVerifyUsingMatch(personId, fingerIndex);
    }
    private Dictionary<string, string> LoadConfig(string path)
    {
        var config = new Dictionary<string, string>();

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"⚠️ config.sys not found at: {path}");
        }

        try
        {
            foreach (var line in File.ReadAllLines(path))
            {
                if (!string.IsNullOrWhiteSpace(line) && line.Contains('='))
                {
                    var parts = line.Split('=', 2);
                    config[parts[0].Trim()] = parts[1].Trim();
                }
            }

            return config;
        }
        catch (Exception ex)
        {
            throw new Exception($"⚠️ Error reading config.sys: {ex.Message}", ex);
        }
    }


    [return: System.Diagnostics.CodeAnalysis.MaybeNull]
    private byte[]? GetStoredTemplate(string personId, int fingerIndex, string member = "prisoner")
    {
        // const string connectionString = "server=localhost;user=root;password=sa;database=finger;";
        // using var conn = new MySqlConnection(connectionString);
        // conn.Open();

        // string sql = "SELECT template FROM fingerprint_templates WHERE person_id = @person_id AND finger_index = @finger_index";


  
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.sys");
        var cfg = LoadConfig(configPath);

        string connStr = $"server={cfg["host"]};user={cfg["user"]};password={cfg["password"]};database={cfg["database"]}";

        using var conn = new MySqlConnection(connStr);
        conn.Open();

        string sql = "SELECT `template` FROM `fingerprint_templates` WHERE `person_id` = @person_id AND `finger_index` = @finger_index AND `member` = @member";
        using var cmd = new MySqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@person_id", personId);
        cmd.Parameters.AddWithValue("@finger_index", fingerIndex);
        cmd.Parameters.AddWithValue("@member", member);

        var result = cmd.ExecuteScalar();
        if (result == DBNull.Value || result == null)
        {
            return null;
        }

        return (byte[]?)result;
    }

    private byte[]? ExtractTemplateFromCallback(IntPtr segmentList, uint numSegment)
    {
        if (segmentList != IntPtr.Zero && numSegment > 0)
        {
            int structSize = Marshal.SizeOf(typeof(FP_SegmentImagDesc));
            for (int i = 0; i < numSegment; i++)
            {
                IntPtr itemPtr = new IntPtr(segmentList.ToInt64() + i * structSize);
                var seg = Marshal.PtrToStructure<FP_SegmentImagDesc>(itemPtr)!;

                // ✅ Try FIR template first
                if (seg.pszFirData != IntPtr.Zero && seg.m_unFirLength > 0)
                {
                    byte[] firTemplate = new byte[seg.m_unFirLength];
                    Marshal.Copy(seg.pszFirData, firTemplate, 0, (int)seg.m_unFirLength);
                    return firTemplate!;
                }

                // 🔁 Fallback to FeatureData
                if (seg.pszFeatureData != IntPtr.Zero && seg.m_unFeatureLength > 0)
                {
                    byte[] featureTemplate = new byte[seg.m_unFeatureLength];
                    Marshal.Copy(seg.pszFeatureData, featureTemplate, 0, (int)seg.m_unFeatureLength);
                    return featureTemplate;
                }
            }
        }
        return null;
    }

    private byte[]? LoadBmpFromDatabase(string personId, int fingerIndex, string member = "prisoner")
    {
        try
        {
            // ✅ Load database config from config.sys
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.sys");
            var cfg = LoadConfig(configPath);
            string connStr = $"server={cfg["host"]};user={cfg["user"]};password={cfg["password"]};database={cfg["database"]}";

            using var conn = new MySqlConnection(connStr);
            conn.Open();

            string query = "SELECT `image_bmp` FROM `fingerprint_templates` WHERE `person_id` = @personId AND `finger_index` = @fingerIndex AND `member` = @member";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@personId", personId);
            cmd.Parameters.AddWithValue("@fingerIndex", fingerIndex);
            cmd.Parameters.AddWithValue("@member", member);

            object result = cmd.ExecuteScalar();

            if (result == null || result == DBNull.Value)
            {
                return null; // Not found
            }

            return (byte[]?)result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error accessing database: {ex.Message}");
            return null;
        }
    }




    private static byte[] ConvertBitmapToGrayscaleBytes(Bitmap bitmap)
    {
        if (bitmap.PixelFormat != PixelFormat.Format8bppIndexed)
            throw new ArgumentException("Bitmap must be 8bpp grayscale");

        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                        ImageLockMode.ReadOnly, bitmap.PixelFormat);

        int size = data.Stride * data.Height;
        byte[] pixelBytes = new byte[size];
        Marshal.Copy(data.Scan0, pixelBytes, 0, size);
        bitmap.UnlockBits(data);

        if (data.Stride != data.Width)
        {
            byte[] clean = new byte[data.Width * data.Height];
            for (int y = 0; y < data.Height; y++)
                Array.Copy(pixelBytes, y * data.Stride, clean, y * data.Width, data.Width);
            return clean;
        }

        return pixelBytes;
    }


    // private void CaptureAndVerify(byte[] storedTemplate)
    // {
    //     TrustFingerNative.ARAFPSCAN_GlobalInit(); // make sure it's called once in your app

    //     if (handle == IntPtr.Zero)
    //     {
    //         int openResult = TrustFingerNative.ARAFPSCAN_OpenDevice(ref handle, 0);
    //         if (openResult != 0)
    //         {
    //             MessageBox.Show("❌ Failed to open fingerprint device.");
    //             return;
    //         }
    //     }

    //     byte[] capturedTemplate = new byte[2048];
    //     int capturedLength = capturedTemplate.Length;

    //     int result = TrustFingerNative.ARAFPSCAN_CaptureISOData(
    //         handle,
    //         0,
    //         capturedTemplate,
    //         ref capturedLength,
    //         7000
    //     );

    //     if (result != 0 || capturedLength <= 0)
    //     {
    //         MessageBox.Show($"❌ Failed to capture fingerprint template. SDK Error Code: {result}, Length: {capturedLength}");
    //         return;
    //     }

    //     Array.Resize(ref capturedTemplate, capturedLength);

    //     int similarity = 0;
    //     int matchResult = TrustFingerNative.ARAFPSCAN_Verify(
    //         4, capturedTemplate, storedTemplate, out similarity
    //     );

    //     string message = (matchResult == 0 && similarity >= 60)
    //         ? $"✅ Match found! Similarity: {similarity}%"
    //         : $"❌ No match. Similarity: {similarity}%";

    //     MessageBox.Show(message);
    // }
    // private string CaptureAndVerifyFromBmp(string personId, int fingerIndex)
    // {
    //     if (handle == IntPtr.Zero)
    //     {
    //         string error = "❌ Fingerprint device is not connected.";
    //         // MessageBox.Show(error);
    //         Console.WriteLine("[CaptureAndVerifyFromBmp] " + error);
    //         return error;
    //     }

    //     byte[] storedBmpBytes = LoadBmpFromDatabase(personId, fingerIndex);
    //     if (storedBmpBytes == null || storedBmpBytes.Length == 0)
    //     {
    //         string error = "❌ No stored fingerprint image found for this person and finger.";
    //         // MessageBox.Show(error);
    //         Console.WriteLine("[CaptureAndVerifyFromBmp] " + error);

    //         return error;
    //     }

    //     string fingerName = FingerIndexToString(fingerIndex);
    //     statusLabel.Text = $"Place your {fingerName} on the scanner for verification...";
    //     Refresh();

    //     byte[]? liveBmpBytes = null;

    //     var param = new MultiFingerParam
    //     {
    //         OperationType = (uint)fingerIndex,
    //         FeatureFormat = 3,
    //         Duration = 7000,
    //         IQThreshold = 60,
    //         ConThreshold = 40,
    //         CutImgW = 0,
    //         CutImgH = 0
    //     };

    //     ARAFPSCAN_MultiFingerAcquisitionEventsManagerCallback callback = (eventCode, framePtr, width, height, segmentList, numSegment) =>
    //     {
    //         if (eventCode == 0 && framePtr != IntPtr.Zero)
    //         {
    //             byte[] rawImage = new byte[width * height];
    //             Marshal.Copy(framePtr, rawImage, 0, rawImage.Length);

    //             using Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
    //             ColorPalette palette = bmp.Palette;
    //             for (int i = 0; i < 256; i++) palette.Entries[i] = Color.FromArgb(i, i, i);
    //             bmp.Palette = palette;

    //             BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
    //             Marshal.Copy(rawImage, 0, bmpData.Scan0, rawImage.Length);
    //             bmp.UnlockBits(bmpData);

    //             using MemoryStream ms = new MemoryStream();
    //             bmp.Save(ms, ImageFormat.Bmp);
    //             liveBmpBytes = ms.ToArray();

    //             this.Invoke((Action)(() =>
    //             {
    //                 if (previewBox.Image != null)
    //                     previewBox.Image.Dispose();
    //                 previewBox.Image = new Bitmap(new MemoryStream(liveBmpBytes));
    //             }));
    //         }
    //         return 0;
    //     };

    //     Console.WriteLine("🔍 Starting acquisition...");
    //     int startResult = TrustFingerNative.ARAFPSCAN_MultiFingerStartAcquisition(handle, param, callback);
    //     if (startResult != 0)
    //     {
    //         string error = $"❌ Failed to start acquisition. Error code: {startResult}";
    //         // MessageBox.Show(error);
    //         Console.WriteLine("[CaptureAndVerifyFromBmp] " + error);
    //         return error;
    //     }

    //     Thread.Sleep((int)param.Duration);
    //     TrustFingerNative.ARAFPSCAN_MultiFingerStopAcquisition(handle);
    //     Console.WriteLine("🛑 Acquisition stopped.");

    //     if (liveBmpBytes == null)
    //     {
    //         string error = "❌ Failed to capture live fingerprint.";
    //         // MessageBox.Show(error);
    //         Console.WriteLine("[CaptureAndVerifyFromBmp] " + error);
    //         return error;
    //     }

    //     using MemoryStream msStored = new MemoryStream(storedBmpBytes);
    //     using Bitmap storedBmp = new Bitmap(msStored);
    //     byte[] storedPixels = ConvertBitmapToGrayscaleBytes(storedBmp);
    //     FingerprintImage storedImage = new FingerprintImage(storedBmp.Width, storedBmp.Height, storedPixels);
    //     FingerprintTemplate storedTemplate = new FingerprintTemplate(storedImage);

    //     using MemoryStream msLive = new MemoryStream(liveBmpBytes);
    //     using Bitmap liveBmp = new Bitmap(msLive);
    //     byte[] livePixels = ConvertBitmapToGrayscaleBytes(liveBmp);
    //     FingerprintImage liveImage = new FingerprintImage(liveBmp.Width, liveBmp.Height, livePixels);
    //     FingerprintTemplate liveTemplate = new FingerprintTemplate(liveImage);

    //     FingerprintMatcher matcher = new FingerprintMatcher(storedTemplate);
    //     double score = matcher.Match(liveTemplate);

    //     string resultMessage = score >= 40
    //         ? $"✅ Match! Score: {score:F2}"
    //         : $"❌ No Match. Score: {score:F2}";

    //     statusLabel.Text = resultMessage;
    //     // MessageBox.Show(resultMessage);
    //     Console.WriteLine("[CaptureAndVerifyFromBmp] " + resultMessage);
    //     return resultMessage;
    // }

    
    
    // private string CaptureAndVerifyFromBmp(string personId, int fingerIndex, StreamWriter? writer)
    // {
    //     // Ensure device is open
    //     if (handle == IntPtr.Zero)
    //     {
    //         Console.WriteLine("[Verify] Device handle is null, attempting to open...");
    //         int count = 0;
    //         TrustFingerNative.ARAFPSCAN_GetDeviceCount(ref count);
    //         if (count <= 0)
    //         {
    //             string error = "❌ No fingerprint device found.";
    //             Console.WriteLine("[Verify] " + error);
    //             writer?.WriteLine(error);
    //             writer?.Flush();
    //             return error;
    //         }

    //         int openResult = TrustFingerNative.ARAFPSCAN_OpenDevice(ref handle, 0);
    //         if (openResult != 0 || handle == IntPtr.Zero)
    //         {
    //             string error = $"❌ Failed to open fingerprint device. Error code: {openResult}";
    //             Console.WriteLine("[Verify] " + error);
    //             writer?.WriteLine(error);
    //             writer?.Flush();
    //             return error;
    //         }

    //         Console.WriteLine("[Verify] Device successfully opened.");
    //         Thread.Sleep(300); // allow time for device to stabilize
    //     }

    //     // Load stored BMP
    //     byte[] storedBmpBytes = LoadBmpFromDatabase(personId, fingerIndex);
    //     if (storedBmpBytes == null || storedBmpBytes.Length == 0)
    //     {
    //         string error = "❌ No stored fingerprint image found for this person and finger.";
    //         Console.WriteLine("[Verify] " + error);
    //         writer?.WriteLine(error);
    //         writer?.Flush();
    //         return error;
    //     }

    //     statusLabel.Text = $"Place your {FingerIndexToString(fingerIndex)} on the scanner for verification...";
    //     Refresh();

    //     byte[]? liveBmpBytes = null;
    //     ManualResetEvent captureDone = new ManualResetEvent(false);

    //     var param = new MultiFingerParam
    //     {
    //         OperationType = (uint)fingerIndex,
    //         FeatureFormat = 3,
    //         Duration = 7000,
    //         IQThreshold = 60,
    //         ConThreshold = 40,
    //         CutImgW = 0,
    //         CutImgH = 0
    //     };

    //     ARAFPSCAN_MultiFingerAcquisitionEventsManagerCallback callback = (eventCode, framePtr, width, height, segmentList, numSegment) =>
    //     {
    //         if (eventCode == 0 && framePtr != IntPtr.Zero)
    //         {
    //             try
    //             {
    //                 byte[] rawImage = new byte[width * height];
    //                 Marshal.Copy(framePtr, rawImage, 0, rawImage.Length);

    //                 using Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
    //                 ColorPalette palette = bmp.Palette;
    //                 for (int i = 0; i < 256; i++) palette.Entries[i] = Color.FromArgb(i, i, i);
    //                 bmp.Palette = palette;

    //                 BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
    //                 Marshal.Copy(rawImage, 0, bmpData.Scan0, rawImage.Length);
    //                 bmp.UnlockBits(bmpData);

    //                 using MemoryStream ms = new MemoryStream();
    //                 bmp.Save(ms, ImageFormat.Bmp);
    //                 liveBmpBytes = ms.ToArray();
    //                 captureDone.Set();

    //                 this.Invoke((Action)(() =>
    //                 {
    //                     if (previewBox.Image != null)
    //                         previewBox.Image.Dispose();
    //                     previewBox.Image = new Bitmap(new MemoryStream(liveBmpBytes));
    //                 }));
    //             }
    //             catch (Exception ex)
    //             {
    //                 Console.WriteLine($"[Verify] Image conversion failed: {ex.Message}");
    //             }
    //         }
    //         return 0;
    //     };

    //     Console.WriteLine("[Verify] Starting acquisition...");
    //     int result = TrustFingerNative.ARAFPSCAN_MultiFingerStartAcquisition(handle, param, callback);
    //     if (result != 0)
    //     {
    //         string error = $"❌ Failed to start acquisition. Error code: {result}";
    //         Console.WriteLine("[Verify] " + error);
    //         writer?.WriteLine(error);
    //         writer?.Flush();
    //         return error;
    //     }

    //     // Wait for image capture
    //     bool captured = captureDone.WaitOne((int)param.Duration);
    //     TrustFingerNative.ARAFPSCAN_MultiFingerStopAcquisition(handle);
    //     Console.WriteLine("[Verify] Acquisition stopped.");

    //     if (!captured || liveBmpBytes == null || liveBmpBytes.Length == 0)
    //     {
    //         string error = "❌ Failed to capture live fingerprint.";
    //         Console.WriteLine("[Verify] " + error);
    //         writer?.WriteLine(error);
    //         writer?.Flush();
    //         return error;
    //     }

    //     // Compare captured image with stored one
    //     try
    //     {
    //         using Bitmap storedBmp = new Bitmap(new MemoryStream(storedBmpBytes));
    //         using Bitmap liveBmp = new Bitmap(new MemoryStream(liveBmpBytes));

    //         var storedPixels = ConvertBitmapToGrayscaleBytes(storedBmp);
    //         var storedTemplate = new FingerprintTemplate(new FingerprintImage(storedBmp.Width, storedBmp.Height, storedPixels));

    //         var livePixels = ConvertBitmapToGrayscaleBytes(liveBmp);
    //         var liveTemplate = new FingerprintTemplate(new FingerprintImage(liveBmp.Width, liveBmp.Height, livePixels));

    //         var matcher = new FingerprintMatcher(storedTemplate);
    //         double score = matcher.Match(liveTemplate);

    //         string resultMessage = score >= 40
    //             ? $"✅ Match! Score: {score:F2}"
    //             : $"❌ No Match. Score: {score:F2}";

    //         Console.WriteLine("[Verify] " + resultMessage);
    //         statusLabel.Text = resultMessage;

    //         writer?.WriteLine(resultMessage);
    //         string base64Bmp = Convert.ToBase64String(liveBmpBytes);
    //         writer?.WriteLine($"BMP:{base64Bmp}");
    //         writer?.Flush();

    //         return resultMessage;
    //     }
    //     catch (Exception ex)
    //     {
    //         string error = $"❌ Exception during verification: {ex.Message}";
    //         Console.WriteLine("[Verify] " + error);
    //         writer?.WriteLine(error);
    //         writer?.Flush();
    //         return error;
    //     }
    // }



    private void CaptureAndSave(string personId, int fingerIndex, string member = "prisoner")
    {
        // Check if device is connected
        if (handle == IntPtr.Zero)
        {
            string error = "❌ Fingerprint device not connected.";
            Console.WriteLine("[Capture] " + error);
            statusLabel.Text = error;
            return;
        }

        // string personId = personIdTextBox.Text.Trim();
        // int fingerIndex = fingerSelector.SelectedIndex + 1;
        byte[]? bmpBytes = null;
        string fingerName = FingerIndexToString(fingerIndex);

        captureButton.Enabled = false;
        statusLabel.Text = $"Place your {fingerName} on the scanner...";
        Refresh();

        var param = new MultiFingerParam
        {
            OperationType = (uint)fingerIndex,
            FeatureFormat = 3, // still needed by SDK for raw capture
            Duration = 7000,
            IQThreshold = 60,
            ConThreshold = 40,
            CutImgW = 0,
            CutImgH = 0
        };

        ARAFPSCAN_MultiFingerAcquisitionEventsManagerCallback callback = (eventCode, framePtr, width, height, segmentList, numSegment) =>
        {
            if (eventCode == 0 && framePtr != IntPtr.Zero)
            {
                int imageSize = width * height;
                byte[] rawImage = new byte[imageSize];
                Marshal.Copy(framePtr, rawImage, 0, imageSize);

                using Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
                ColorPalette palette = bmp.Palette;
                for (int i = 0; i < 256; i++) palette.Entries[i] = Color.FromArgb(i, i, i);
                bmp.Palette = palette;

                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                Marshal.Copy(rawImage, 0, bmpData.Scan0, rawImage.Length);
                bmp.UnlockBits(bmpData);

                using MemoryStream ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Bmp);
                bmpBytes = ms.ToArray();

                this.Invoke((Action)(() =>
                {
                    if (previewBox.Image != null)
                        previewBox.Image.Dispose();
                    previewBox.Image = new Bitmap(new MemoryStream(bmpBytes));
                }));
            }
            return 0;
        };

        TrustFingerNative.ARAFPSCAN_MultiFingerStartAcquisition(handle, param, callback);
        System.Threading.Thread.Sleep(7000);
        TrustFingerNative.ARAFPSCAN_MultiFingerStopAcquisition(handle);

        if (bmpBytes != null)
        {
            try
            {
                // Save only BMP (no template)
                InsertFingerprintToDatabase(personId, fingerIndex, bmpBytes, null, member);

                statusLabel.Text = $"✅ Successfully captured and saved {fingerName}.";
            }
            catch (Exception ex)
            {
                // MessageBox.Show($"❌ Failed to save fingerprint: {ex.Message}");
                Console.WriteLine($"❌ Failed to save fingerprint: {ex.Message}");
                statusLabel.Text = "❌ Database error occurred.";
            }
        }
        else
        {
            statusLabel.Text = "❌ Capture failed. Please try again.";
        }

        captureButton.Enabled = true;
    }

    private void CaptureAndSave(string personId, int fingerIndex, string member, StreamWriter? writer)
    {
        // Check if device is connected
        if (handle == IntPtr.Zero)
        {
            string error = "❌ Fingerprint device not connected.";
            Console.WriteLine("[Capture] " + error);
            statusLabel.Text = error;
            if (writer != null)
            {
                writer.WriteLine(error);
                writer.Flush();
            }
            return;
        }

        byte[]? bmpBytes = null;
        string fingerName = FingerIndexToString(fingerIndex);

        captureButton.Enabled = false;
        statusLabel.Text = $"Place your {fingerName} on the scanner...";
        Refresh();

        var param = new MultiFingerParam
        {
            OperationType = (uint)fingerIndex,
            FeatureFormat = 3,
            Duration = 7000,
            IQThreshold = 60,
            ConThreshold = 40,
            CutImgW = 0,
            CutImgH = 0
        };

        ARAFPSCAN_MultiFingerAcquisitionEventsManagerCallback callback = (eventCode, framePtr, width, height, segmentList, numSegment) =>
        {
            if (eventCode == 0 && framePtr != IntPtr.Zero)
            {
                int imageSize = width * height;
                byte[] rawImage = new byte[imageSize];
                Marshal.Copy(framePtr, rawImage, 0, imageSize);

                using Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
                ColorPalette palette = bmp.Palette;
                for (int i = 0; i < 256; i++) palette.Entries[i] = Color.FromArgb(i, i, i);
                bmp.Palette = palette;

                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                Marshal.Copy(rawImage, 0, bmpData.Scan0, rawImage.Length);
                bmp.UnlockBits(bmpData);

                using MemoryStream ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Bmp);
                bmpBytes = ms.ToArray();

                this.Invoke((Action)(() =>
                {
                    if (previewBox.Image != null)
                        previewBox.Image.Dispose();
                    previewBox.Image = new Bitmap(new MemoryStream(bmpBytes));
                }));
            }
            return 0;
        };

        TrustFingerNative.ARAFPSCAN_MultiFingerStartAcquisition(handle, param, callback);
        Thread.Sleep(7000);
        TrustFingerNative.ARAFPSCAN_MultiFingerStopAcquisition(handle);

        if (bmpBytes != null)
        {
            try
            {
                InsertFingerprintToDatabase(personId, fingerIndex, bmpBytes, null, member);

                string message = $"✅ Successfully captured and saved {fingerName}.";
                statusLabel.Text = message;

                if (writer != null)
                {
                    writer.WriteLine(message);
                    string base64Bmp = Convert.ToBase64String(bmpBytes);
                    writer.WriteLine($"BMP:{base64Bmp}");
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                string error = $"❌ Failed to save fingerprint: {ex.Message}";
                statusLabel.Text = error;
                if (writer != null)
                {
                    writer.WriteLine(error);
                    writer.Flush();
                }
            }
        }
        else
        {
            string error = "❌ Capture failed. Please try again.";
            statusLabel.Text = error;
            if (writer != null)
            {
                writer.WriteLine(error);
                writer.Flush();
            }
        }

        captureButton.Enabled = true;
    }

    // Add this method to your MainForm.cs
    // Add this method to your MainForm.cs

    private List<(string PersonId, int FingerIndex, FingerprintTemplate Template)> LoadAllFingerprintTemplates()
    {
        var result = new List<(string, int, FingerprintTemplate)>();

        // Load config
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.sys");
        
        var cfg = LoadConfig(configPath);

        // Construct connection string
        string connStr = $"server={cfg["host"]};user={cfg["user"]};password={cfg["password"]};database={cfg["database"]}";

        using var conn = new MySqlConnection(connStr);
        conn.Open();

        string query = "SELECT `person_id`, `finger_index`, `image_bmp` FROM `fingerprint_templates` WHERE `image_bmp` IS NOT NULL";
        using var cmd = new MySqlCommand(query, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            string personId = reader.GetString("person_id");
            int fingerIndex = reader.GetInt32("finger_index");
            byte[] bmpBytes = (byte[])reader["image_bmp"];

            using MemoryStream ms = new MemoryStream(bmpBytes);
            using Bitmap bmp = new Bitmap(ms);
            byte[] pixels = ConvertBitmapToGrayscaleBytes(bmp);
            FingerprintImage img = new FingerprintImage(bmp.Width, bmp.Height, pixels);
            FingerprintTemplate template = new FingerprintTemplate(img);

            result.Add((personId, fingerIndex, template));
        }

        return result;
    }


    private Bitmap? CaptureBmpFromScanner()
    {
        Bitmap? bmp = null;
        byte[]? bmpBytes = null;
        bool imageCaptured = false;

        var param = new MultiFingerParam
        {
            OperationType = 1,
            FeatureFormat = 3,
            Duration = 7000,
            IQThreshold = 60,
            ConThreshold = 40,
            CutImgW = 0,
            CutImgH = 0
        };

        var callback = new ARAFPSCAN_MultiFingerAcquisitionEventsManagerCallback((eventCode, framePtr, width, height, segmentList, numSegment) =>
        {
            if (eventCode == 0 && framePtr != IntPtr.Zero)
            {
                byte[] rawImage = new byte[width * height];
                Marshal.Copy(framePtr, rawImage, 0, rawImage.Length);

                using Bitmap tempBmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
                ColorPalette palette = tempBmp.Palette;
                for (int i = 0; i < 256; i++) palette.Entries[i] = Color.FromArgb(i, i, i);
                tempBmp.Palette = palette;

                BitmapData bmpData = tempBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, tempBmp.PixelFormat);
                Marshal.Copy(rawImage, 0, bmpData.Scan0, rawImage.Length);
                tempBmp.UnlockBits(bmpData);

                using MemoryStream ms = new MemoryStream();
                tempBmp.Save(ms, ImageFormat.Bmp);
                bmpBytes = ms.ToArray();
                imageCaptured = true;
            }
            return 0;
        });

        int startResult = TrustFingerNative.ARAFPSCAN_MultiFingerStartAcquisition(handle, param, callback);
        Thread.Sleep((int)param.Duration);
        TrustFingerNative.ARAFPSCAN_MultiFingerStopAcquisition(handle);

        if (imageCaptured && bmpBytes != null)
        {
            bmp = new Bitmap(new MemoryStream(bmpBytes));
        }

        return bmp;
    }



    private string MatchAndIdentify()
    {
        captureButton.Enabled = true;
        matchButton.Enabled = true;
        statusLabel.Text = "Place your finger on the scanner to identify...";
        Refresh();

        // Step 1: Capture live image and convert to template
        Bitmap? liveBmp = CaptureBmpFromScanner();
        if (liveBmp == null)
        {
            string msg = "❌ Failed to capture fingerprint image.";
            statusLabel.Text = msg;
            captureButton.Enabled = true;
            matchButton.Enabled = true;
            return msg;
        }

        statusLabel.Text = "Processing fingerprint match...";
        Application.DoEvents();

        byte[] livePixels = ConvertBitmapToGrayscaleBytes(liveBmp);
        FingerprintImage liveImage = new FingerprintImage(liveBmp.Width, liveBmp.Height, livePixels);
        FingerprintTemplate liveTemplate = new FingerprintTemplate(liveImage);

        double bestScore = 0;
        string? bestPersonId = null;
        int bestFingerIndex = -1;
        var matcher = new FingerprintMatcher(liveTemplate);

        // ✅ Step 2: Load DB config from config.sys
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.sys");
        var cfg = LoadConfig(configPath);
        string connStr = $"server={cfg["host"]};user={cfg["user"]};password={cfg["password"]};database={cfg["database"]}";

        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();
            using (var cmd = new MySqlCommand("SELECT `person_id`, `finger_index`, `image_bmp` FROM `fingerprint_templates` WHERE `image_bmp` IS NOT NULL", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string personId = reader.GetString("person_id");
                    int fingerIndex = reader.GetInt32("finger_index");
                    byte[] bmpBytes = (byte[])reader["image_bmp"];

                    try
                    {
                        using (var ms = new MemoryStream(bmpBytes))
                        using (var bmp = new Bitmap(ms))
                        {
                            byte[] pixels = ConvertBitmapToGrayscaleBytes(bmp);
                            var dbImage = new FingerprintImage(bmp.Width, bmp.Height, pixels);
                            var dbTemplate = new FingerprintTemplate(dbImage);

                            double score = matcher.Match(dbTemplate);
                            if (score > bestScore)
                            {
                                bestScore = score;
                                bestPersonId = personId;
                                bestFingerIndex = fingerIndex;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error with template for {personId}: {ex.Message}");
                    }

                    GC.Collect(); // Manual cleanup
                }
            }
        }

        // Step 3: Return result
        string resultMessage;
        if (bestScore > 40 && bestPersonId != null)
        {
            resultMessage = $"✅ Match: {bestPersonId}, Finger: {FingerIndexToString(bestFingerIndex)}, Score: {bestScore:F2}";
        }
        else
        {
            resultMessage = $"❌ No good match found. Best score = {bestScore:F2}";
        }

        statusLabel.Text = resultMessage;
        return resultMessage;
    }


    private string MatchAndIdentify(StreamWriter? writer)
    {
        captureButton.Enabled = true;
        matchButton.Enabled = true;
        statusLabel.Text = "Place your finger on the scanner to identify...";
        Refresh();

        Bitmap? liveBmp = CaptureBmpFromScanner();
        if (liveBmp == null)
        {
            string msg = "❌ Failed to capture fingerprint image.";
            statusLabel.Text = msg;

            if (writer != null)
            {
                writer.WriteLine(msg);
                writer.Flush();
            }

            captureButton.Enabled = true;
            matchButton.Enabled = true;
            return msg;
        }

        statusLabel.Text = "Processing fingerprint match...";
        Application.DoEvents();

        byte[] livePixels = ConvertBitmapToGrayscaleBytes(liveBmp);
        FingerprintImage liveImage = new FingerprintImage(liveBmp.Width, liveBmp.Height, livePixels);
        FingerprintTemplate liveTemplate = new FingerprintTemplate(liveImage);

        double bestScore = 0;
        string? bestPersonId = null;
        int bestFingerIndex = -1;
        var matcher = new FingerprintMatcher(liveTemplate);

        // ✅ Load config for DB connection
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.sys");
        var cfg = LoadConfig(configPath);
        string connStr = $"server={cfg["host"]};user={cfg["user"]};password={cfg["password"]};database={cfg["database"]}";

        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();
            using (var cmd = new MySqlCommand("SELECT `person_id`, `finger_index`, `image_bmp` FROM `fingerprint_templates` WHERE `image_bmp` IS NOT NULL", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string personId = reader.GetString("person_id");
                    int fingerIndex = reader.GetInt32("finger_index");
                    byte[] bmpBytes = (byte[])reader["image_bmp"];

                    try
                    {
                        using (var ms = new MemoryStream(bmpBytes))
                        using (var dbBmp = new Bitmap(ms))
                        {
                            byte[] dbPixels = ConvertBitmapToGrayscaleBytes(dbBmp);
                            var dbImage = new FingerprintImage(dbBmp.Width, dbBmp.Height, dbPixels);
                            var dbTemplate = new FingerprintTemplate(dbImage);

                            double score = matcher.Match(dbTemplate);
                            if (score > bestScore)
                            {
                                bestScore = score;
                                bestPersonId = personId;
                                bestFingerIndex = fingerIndex;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error loading template for {personId}: {ex.Message}");
                    }

                    GC.Collect(); // Help prevent memory buildup
                }
            }
        }

        string resultMessage;
        if (bestScore > 40 && bestPersonId != null)
        {
            resultMessage = $"✅ Match: {bestPersonId}, Finger: {FingerIndexToString(bestFingerIndex)}, Score: {bestScore:F2}";
        }
        else
        {
            resultMessage = $"❌ No good match found. Best score = {bestScore:F2}";
        }

        statusLabel.Text = resultMessage;

        if (writer != null)
        {
            writer.WriteLine(resultMessage);

            try
            {
                using (var ms = new MemoryStream())
                {
                    liveBmp.Save(ms, ImageFormat.Bmp);
                    string base64Bmp = Convert.ToBase64String(ms.ToArray());
                    writer.WriteLine($"BMP:{base64Bmp}");
                }
            }
            catch (Exception ex)
            {
                writer.WriteLine($"ERROR: Failed to encode image - {ex.Message}");
            }

            writer.Flush();
        }

        return resultMessage;
    }


    // Add this in constructor after other buttons

    private void matchButton_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(personIdTextBox.Text.Trim()))
        {
            statusLabel.ForeColor = Color.Red;
            statusLabel.Text = "Please enter a Person ID for matching.";
            Console.WriteLine("Validation failed: Person ID is required for matching.");
            personIdTextBox.Focus();
            return;
        }

        if (captureButton.Enabled)
        {
            captureButton.Enabled = false;
            matchButton.Enabled = false;

            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fingerprint_match_log.txt");
            using (var writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"Match started at {DateTime.Now}");
                MatchAndIdentify(writer);
                writer.WriteLine($"Match ended at {DateTime.Now}\n");
            }

            captureButton.Enabled = true;
            matchButton.Enabled = true;
        }
    }

    private string CaptureAndVerifyUsingMatch(string personId, int fingerIndex, string member, StreamWriter? writer)
    {
        if (handle == IntPtr.Zero)
        {
            string error = "❌ Fingerprint device not connected.";
            Console.WriteLine("[Verify] " + error);
            writer?.WriteLine(error);
            writer?.Flush();
            return error;
        }

        // Load stored image
        byte[] storedBmpBytes = LoadBmpFromDatabase(personId, fingerIndex, member);
        if (storedBmpBytes == null || storedBmpBytes.Length == 0)
        {
            string error = "❌ No stored fingerprint found for this person/finger.";
            Console.WriteLine("[Verify] " + error);
            writer?.WriteLine(error);
            writer?.Flush();
            return error;
        }

        // Live capture
        statusLabel.Text = $"Place your {FingerIndexToString(fingerIndex)} on the scanner...";
        Refresh();
        Bitmap? liveBmp = CaptureBmpFromScanner();
        if (liveBmp == null)
        {
            string msg = "❌ Failed to capture fingerprint image.";
            Console.WriteLine("[Verify] " + msg);
            writer?.WriteLine(msg);
            writer?.Flush();
            return msg;
        }

        // Extract and match
        try
        {
            using var storedBmp = new Bitmap(new MemoryStream(storedBmpBytes));
            byte[] storedPixels = ConvertBitmapToGrayscaleBytes(storedBmp);
            var storedImage = new FingerprintImage(storedBmp.Width, storedBmp.Height, storedPixels);
            var storedTemplate = new FingerprintTemplate(storedImage);

            byte[] livePixels = ConvertBitmapToGrayscaleBytes(liveBmp);
            var liveImage = new FingerprintImage(liveBmp.Width, liveBmp.Height, livePixels);
            var liveTemplate = new FingerprintTemplate(liveImage);

            double score = new FingerprintMatcher(storedTemplate).Match(liveTemplate);
            string resultMessage = score >= 40
                ? $"✅ Match! Score: {score:F2}"
                : $"❌ No Match. Score: {score:F2}";

            Console.WriteLine("[Verify] " + resultMessage);
            statusLabel.Text = resultMessage;
            writer?.WriteLine(resultMessage);

            using var ms = new MemoryStream();
            liveBmp.Save(ms, ImageFormat.Bmp);
            writer?.WriteLine("BMP:" + Convert.ToBase64String(ms.ToArray()));
            writer?.Flush();

            return resultMessage;
        }
        catch (Exception ex)
        {
            string error = $"❌ Verification exception: {ex.Message}";
            Console.WriteLine("[Verify] " + error);
            writer?.WriteLine(error);
            writer?.Flush();
            return error;
        }
    }

    public (string result, string base64Bmp) CaptureAndVerifyUsingMatch(string personId, int fingerIndex, string member = "prisoner")
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms);

        string result = CaptureAndVerifyUsingMatch(personId, fingerIndex, member, writer);
        writer.Flush();
        string fullOutput = Encoding.UTF8.GetString(ms.ToArray());

        // Extract BMP line from output
        string? bmpBase64 = null;
        using var reader = new StringReader(fullOutput);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith("BMP:"))
            {
                bmpBase64 = line.Substring(4).Trim();
                break;
            }
        }

        return (result, bmpBase64 ?? "");
    }

    private void InsertFingerprintToDatabase(string personId, int fingerIndex, byte[] bmpData, byte[]? templateData, string member = "prisoner")
    {
        // const string connectionString = "server=localhost;user=root;password=sa;database=finger;";
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.sys");
        var cfg = LoadConfig(configPath);
        string connectionString = $"server={cfg["host"]};user={cfg["user"]};password={cfg["password"]};database={cfg["database"]}";

        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string sql = @"INSERT INTO `fingerprint_templates` (`person_id`, `finger_index`, `image_bmp`, `template`, `member`)
            VALUES (@person_id, @finger_index, @image_bmp, @template, @member)
            ON DUPLICATE KEY UPDATE 
                `image_bmp` = VALUES(`image_bmp`),
                `template` = VALUES(`template`)";

        Console.WriteLine("Executing SQL: " + sql);

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@person_id", personId);
        cmd.Parameters.AddWithValue("@finger_index", fingerIndex);
        cmd.Parameters.AddWithValue("@image_bmp", bmpData);
        cmd.Parameters.AddWithValue("@template", templateData ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@member", member);

        cmd.ExecuteNonQuery();
    }

    private string FingerIndexToString(int idx)
    {
        return idx switch
        {
            1 => "Right Thumb",
            2 => "Right Index",
            3 => "Right Middle",
            4 => "Right Ring",
            5 => "Right Little",
            6 => "Left Thumb",
            7 => "Left Index",
            8 => "Left Middle",
            9 => "Left Ring",
            10 => "Left Little",
            _ => "Unknown"
        };
    }

protected override void OnFormClosing(FormClosingEventArgs e)
{
    trayIcon.Visible = false;
    trayIcon.Dispose();
    base.OnFormClosing(e);
    if (handle != IntPtr.Zero)
    {
        TrustFingerNative.ARAFPSCAN_CloseDevice(ref handle);
        TrustFingerNative.ARAFPSCAN_GlobalFree();
    }
}

protected override void OnResize(EventArgs e)
{
    base.OnResize(e);
    if (this.WindowState == FormWindowState.Minimized)
    {
        this.Hide();
        trayIcon.Visible = true;
    }
}

private void OnTrayOpenClicked(object? sender, EventArgs e)
{
    this.Show();
    this.WindowState = FormWindowState.Normal;
    this.ShowInTaskbar = true;
    this.BringToFront();
}

    private void OnTrayExitClicked(object? sender, EventArgs e)
    {
        trayIcon.Visible = false;
        Application.Exit();
    }

    private void OnTrayAboutClicked(object? sender, EventArgs e)
    {
        var aboutForm = new Form
        {
            Text = "About",
            Width = 250,  // More compact width
            Height = 120, // Even more compact height
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            StartPosition = FormStartPosition.CenterParent
        };

        var richTextBox = new RichTextBox
        {
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = aboutForm.BackColor,
            Dock = DockStyle.Fill,
            Font = new Font(aboutForm.Font.FontFamily, aboutForm.Font.Size),
            SelectionAlignment = HorizontalAlignment.Center // Center align all text
        };

        // Add small padding at the top
        richTextBox.AppendText("\n");

        // Add the styled APIS text with 2x size
        richTextBox.SelectionFont = new Font(richTextBox.Font.FontFamily, richTextBox.Font.Size * 2.0f, FontStyle.Bold);
        richTextBox.SelectionColor = Color.RoyalBlue; // Brighter blue
        richTextBox.AppendText("AP");
        richTextBox.SelectionColor = Color.Crimson; // Brighter red
        richTextBox.AppendText("I");
        richTextBox.SelectionColor = Color.RoyalBlue; // Brighter blue
        richTextBox.AppendText("S");
        
        // Add half spacing
        richTextBox.AppendText("\n");

        // Add the original text at normal size
        richTextBox.SelectionFont = richTextBox.Font;
        richTextBox.SelectionColor = Color.Black;
        richTextBox.AppendText("APIS Co. Ltd, All rights reseverd");

        aboutForm.Controls.Add(richTextBox);
        aboutForm.ShowDialog();
    }

private void OnTrayIconDoubleClick(object? sender, EventArgs e)
{
    OnTrayOpenClicked(sender, e);
}


}
