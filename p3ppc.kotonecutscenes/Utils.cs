using System.Diagnostics;
using System.Text;
using p3ppc.kotonecutscenes.Configuration;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace p3ppc.kotonecutscenes;

public class Utils
{
    private static ILogger _logger;
    private static Config _config;
    private static IStartupScanner _startupScanner;
    internal static nint BaseAddress { get; private set; }

    internal static bool Initialise(ILogger logger, Config config, IModLoader modLoader)
    {
        _logger = logger;
        _config = config;
        using var thisProcess = Process.GetCurrentProcess();
        BaseAddress = thisProcess.MainModule!.BaseAddress;

        var startupScannerController = modLoader.GetController<IStartupScanner>();
        if (startupScannerController == null || !startupScannerController.TryGetTarget(out _startupScanner))
        {
            LogError($"Unable to get controller for Reloaded SigScan Library, stuff won't work :(");
            return false;
        }

        return true;

    }

    internal static void Log(string message)
    {
        _logger.WriteLine($"[Kotone Cutscenes Project] {message}");
    }

    internal static void LogError(string message, Exception e)
    {
        _logger.WriteLine($"[Kotone Cutscenes Project] {message}: {e.Message}", System.Drawing.Color.Red);
    }

    internal static void LogError(string message)
    {
        _logger.WriteLine($"[Kotone Cutscenes Project] {message}", System.Drawing.Color.Red);
    }

    internal static void SigScan(string pattern, string name, Action<nint> action)
    {
        _startupScanner.AddMainModuleScan(pattern, result =>
        {
            if (!result.Found)
            {
                LogError($"Unable to find {name}, stuff won't work :(");
                return;
            }
            Log($"Found {name} at 0x{result.Offset + BaseAddress:X}");

            action(result.Offset + BaseAddress);
        });
    }

    // Pushes the value of an xmm register to the stack, saving it so it can be restored with PopXmm
    public static string PushXmm(int xmmNum)
    {
        return // Save an xmm register 
            $"sub rsp, 16\n" + // allocate space on stack
            $"movdqu dqword [rsp], xmm{xmmNum}\n";
    }

    // Pushes all xmm registers (0-15) to the stack, saving them to be restored with PopXmm
    public static string PushXmm()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < 16; i++)
        {
            sb.Append(PushXmm(i));
        }
        return sb.ToString();
    }

    // Pops the value of an xmm register to the stack, restoring it after being saved with PushXmm
    public static string PopXmm(int xmmNum)
    {
        return                 //Pop back the value from stack to xmm
            $"movdqu xmm{xmmNum}, dqword [rsp]\n" +
            $"add rsp, 16\n"; // re-align the stack
    }

    // Pops all xmm registers (0-7) from the stack, restoring them after being saved with PushXmm
    public static string PopXmm()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 7; i >= 0; i--)
        {
            sb.Append(PopXmm(i));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Gets the address of a global from something that references it
    /// </summary>
    /// <param name="ptrAddress">The address to the pointer to the global (like in a mov instruction or something)</param>
    /// <returns>The address of the global</returns>
    internal static unsafe nuint GetGlobalAddress(nint ptrAddress)
    {
        return (nuint)((*(int*)ptrAddress) + ptrAddress + 4);
    }

    /// <summary>
    /// Scans for multiple instances of a signature
    /// </summary>
    /// <param name="pattern">The pattern/signature to look for</param>
    /// <param name="name">The name of the thing you're looking for (used only for logging)</param>
    /// <param name="indexes">A list of 0 based indexes of the results to find. e.g. [0, 2] will get the first and third instance of the pattern</param>
    /// <param name="action">The action to run each time an instance of the pattern is found at one of the specified indexes</param>
    internal static void SigScan(string pattern, string name, int[] indexes, Action<nint> action)
    {
        using var thisProcess = Process.GetCurrentProcess();
        using var scanner = new Scanner(thisProcess, thisProcess.MainModule);
        int offset = 0;
        int maxIndex = indexes.Max() + 1;

        for (int i = 0; i < maxIndex; i++)
        {
            var result = scanner.FindPattern(pattern, offset);

            if (!result.Found)
            {
                LogError($"Unable to find {name} at index {i}, stuff won't work :(");
                return;
            }

            if (indexes.Contains(i))
            {
                Log($"Found {name} ({i}) at 0x{result.Offset + BaseAddress:X}");

                action(result.Offset + BaseAddress);
                offset = result.Offset + 1;
            }
        }
    }

    /// <summary>
    /// Scans for all instances of a signature
    /// </summary>
    /// <param name="pattern">The pattern/signature to look for</param>
    /// <param name="name">The name of the thing you're looking for (used only for logging)</param>
    /// <param name="action">The action to run each time an instance of the pattern is found</param>

    internal static void SigScanAll(string pattern, string name, Action<nint> action)
    {
        using var thisProcess = Process.GetCurrentProcess();
        using var scanner = new Scanner(thisProcess, thisProcess.MainModule);
        int offset = 0;

        var result = scanner.FindPattern(pattern, offset);
        int i = 0;
        while (result.Found)
        {
            Log($"Found {name} ({i++}) at 0x{result.Offset + BaseAddress:X}");

            action(result.Offset + BaseAddress);
            offset = result.Offset + 1;
            result = scanner.FindPattern(pattern, offset);
        }
    }

}