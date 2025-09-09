using System;
using System.IO;
using System.Windows.Forms;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FingerprintGuiApp());
        }
        catch (Exception ex)
        {
            File.WriteAllText("error.log", ex.ToString());
            MessageBox.Show("Unhandled exception: " + ex.Message);
        }
    }
}
