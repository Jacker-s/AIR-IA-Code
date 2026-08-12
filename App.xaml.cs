using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace AirCodeNative;

public partial class App : System.Windows.Application
{
    static Mutex? instanceMutex;
    static bool ownsMutex;
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr handle);
    [DllImport("user32.dll")] static extern bool ShowWindowAsync(IntPtr handle, int command);

    protected override void OnStartup(StartupEventArgs e)
    {
        instanceMutex = new Mutex(true, "Local\\AIRIACode.SingleInstance", out var firstInstance);
        ownsMutex = firstInstance;
        if (!firstInstance)
        {
            var current = Environment.ProcessId;
            var existing = Process.GetProcessesByName("AIRIACode").FirstOrDefault(process => process.Id != current && process.MainWindowHandle != IntPtr.Zero);
            if (existing is not null) { ShowWindowAsync(existing.MainWindowHandle, 9); SetForegroundWindow(existing.MainWindowHandle); }
            Shutdown(); return;
        }
        base.OnStartup(e); MainWindow = new MainWindow(); MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e) { try { if (ownsMutex) instanceMutex?.ReleaseMutex(); } catch (ApplicationException) { } finally { instanceMutex?.Dispose(); ownsMutex = false; } base.OnExit(e); }
}
