using System.Security.Principal;
using Microsoft.UI.Xaml;

namespace ApkHelper;

public class ApkFileRegistry
{
    private readonly Window _window;

    public ApkFileRegistry(Window window)
    {
        this._window = window;
    }

    // 晚上抄了一段代码，尝试写注册表，但是感觉不对劲
    // https://www.cnblogs.com/kybs0/p/5772321.html
    // string toolPath = @"D:\Gitee\WhiteBoard\Code\bin\Debug\WhiteBoard.exe";
    string extension = ".apk";
    // string fileType = "White Board File";
    // string icon = @"D:\Gitee\WhiteBoard\Code\bin\Debug\fileIcon.ico";

    private static bool IsAdmin()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public void Register()
    {
        if (!IsAdmin())
        {
            ShowAdministratorDialog();
            return;
        }

        WriteToRegistry();
    }

    private void WriteToRegistry()
    {
        CleanRegistry();
        // 写入之前先清理
        // TODO 接着做写入
    }

    private void CleanRegistry()
    {
        // TODO 接着做写入
    }

    public void Unregister()
    {
        if (!IsAdmin())
        {
            ShowAdministratorDialog();
            return;
        }

        CleanRegistry();
    }

    private async void ShowAdministratorDialog()
    {
        await App.ShowDialog(
            window: _window,
            title: "未开启管理员权限",
            content: "关联文件需要写入注册表，这需要管理员权限。\n请关闭APP并右键以管理员权限运行，再重试",
            primary: "",
            close: "知道了。"
        );
    }
}