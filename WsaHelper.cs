using System;
using System.Diagnostics;

namespace ApkHelper;

public class WsaHelper
{
    public delegate void LogCallback(bool isError, string tag, string value);

    private event LogCallback LogCallbackImpl;

    private bool _isConnected = false;
    
    public WsaHelper(LogCallback callback)
    {
        LogCallbackImpl = callback;
    }

    public static bool CheckWsa()
    {
        var processes = Process.GetProcessesByName("WsaClient");
        var isRunning = processes.Length > 0;
        return isRunning;
    }

    public void CheckAdb()
    {
        var tag = "adb";
        CommandLineHelper.Create(@"adb", @"--version")
            .OnOutput(value => { Log(false, tag, value); })
            .OnError(value => { Log(true, tag, value); })
            .OnExit(() => { Log(false, tag, "结束"); })
            .Send();
    }

    public bool TryStartWsa()
    {
        if (CheckWsa())
        {
            ConnectWsa();
            return true;
        }

        _isConnected = false;
        Log(false, "", "WSA 未启动，尝试启动");
        try
        {
            StartWsa();
            ConnectWsa();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return false;
    }

    private void ConnectWsa()
    {
        if (_isConnected)
        {
            return;
        }

        var tag = "adb";
        CommandLineHelper.Create("adb", "connect", "127.0.0.1:58526")
            .OnOutput(value => { Log(false, tag, value); })
            .OnError(value => { Log(true, tag, value); })
            .OnExit(() => { Log(false, tag, "结束"); })
            .Send();
    }

    private void Log(bool isError, string tag, string value)
    {
        LogCallbackImpl?.Invoke(isError, tag, value);
    }

    private void StartWsa()
    {
        var tag = "wsa";
        CommandLineHelper.Create(@"WsaClient", @"/launch", @"wsa://system")
            .OnOutput(value => { Log(false, tag, value); })
            .OnError(value => { Log(true, tag, value); })
            .OnExit(() => { Log(false, tag, "结束"); })
            .Send();
    }
    
    public void SendInstallCommand(
        string taskId,
        string apkPath,
        CommandLineHelper.CmdExitCallback exitCallback
    )
    {
        if (TryStartWsa())
        {
            CommandLineHelper.Create("adb", "install", apkPath)
                .OnOutput(value => { Log(false, taskId, value); })
                .OnError(value => { Log(true, taskId, value); })
                .OnExit(() =>
                {
                    Log(false, taskId, "结束");
                    exitCallback?.Invoke();
                })
                .Send();
        }
    }
}