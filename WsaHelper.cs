using System;
using System.Diagnostics;
using System.IO;

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
        const string tag = "adb";
        CommandLineHelper.Create(@"adb", @"--version")
            .OnOutput(value => { Log(false, tag, value); })
            .OnError(value => { Log(true, tag, value); })
            .OnExit(() => { Log(false, tag, "----------------------------"); })
            .Send();
    }

    private bool TryStartWsa(CommandLineHelper.CmdExitCallback exitCallback)
    {
        if (CheckWsa())
        {
            ConnectWsa(exitCallback);
            return true;
        }

        const string tag = "wsa";
        _isConnected = false;
        Log(false, tag, "WSA 未启动，尝试启动");
        Log(true, tag, "请等待Log输出停止后重试");
        try
        {
            StartWsa();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            if (LogLine.ShowError)
            {
                Log(true, tag, e.Message);
            }
        }

        return false;
    }

    private void ConnectWsa(CommandLineHelper.CmdExitCallback exitCallback)
    {
        if (_isConnected)
        {
            exitCallback?.Invoke();
            return;
        }

        var tag = "adb";
        CommandLineHelper.Create("adb", "connect", "127.0.0.1:58526")
            .OnOutput(value =>
            {
                if (value.Contains("connected to 127.0.0.1:58526"))
                {
                    _isConnected = true;
                }

                Log(false, tag, value);
            })
            .OnError(value => { Log(true, tag, value); })
            .OnExit(() => { exitCallback?.Invoke(); })
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
            .OnError(value =>
            {
                if (LogLine.ShowError)
                {
                    Log(true, tag, value);
                }
            })
            .OnExit(() => { Log(false, tag, "----------------------------"); })
            .Send();
    }

    public void SendInstallCommand(
        string taskId,
        string apkPath,
        CommandLineHelper.CmdExitCallback exitCallback
    )
    {
        var result = TryStartWsa(() =>
        {
            var separator = Path.DirectorySeparatorChar;
            var pathArray = apkPath.Split(separator);
            var fileName = pathArray[^1];
            Log(false, taskId, fileName);
            CommandLineHelper.Create("adb", "install", apkPath)
                .OnOutput(value => { Log(false, taskId, value); })
                .OnError(value => { Log(true, taskId, value); })
                .OnExit(() =>
                {
                    Log(false, taskId, "----------------------------");
                    exitCallback?.Invoke();
                })
                .Send();
        });
        if (!result)
        {
            exitCallback?.Invoke();
        }
    }
}