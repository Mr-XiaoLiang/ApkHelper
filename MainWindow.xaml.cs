using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using Windows.Storage;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ApkHelper
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        private readonly List<string> _pendingApkList = new();
        private int _installCount = 0;
        public readonly LogViewModel LogModel = new();

        public MainWindow()
        {
            InitializeComponent();
            InitWindow();
            UpdateStateToIdel();
            LogInfo(0, "准备就绪");
        }

        private void InitWindow()
        {
            var window = GetAppWindow();
            if (window == null)
            {
                return;
            }
            var screenHeight = DisplayArea.Primary.WorkArea.Height;
            var screenWidth = DisplayArea.Primary.WorkArea.Width;
            var windowHeight = 480;
            var windowWidth = 640;
            if (windowWidth > screenWidth)
            {
                windowWidth = screenWidth;
            }
            if (windowHeight > screenHeight)
            {
                windowHeight = screenHeight;
            }
            var windowLeft = (screenWidth - windowWidth) / 2;
            var windowTop = (screenHeight - windowHeight) / 2;
            window.MoveAndResize(new RectInt32(windowLeft, windowTop, windowWidth, windowHeight));
        }

        private AppWindow GetAppWindow()
        {
            var windowHandler = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(windowHandler);
            return AppWindow.GetFromWindowId(windowId);
        }

        public void OnDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "安装";
        }

        public async void OnDropAsync(object sender, DragEventArgs e)
        {
            var apkList = await GetDragItemsAsync(e);
            if (apkList.Count < 1)
            {
                // 数量不对，那么放弃
                UpdateStateToIdel();
                return;
            }
            apkList.ForEach(apk =>
                _pendingApkList.Add(apk.Path)
            ) ;
            Install();
        }

        private async Task<List<IStorageItem>> GetDragItemsAsync(DragEventArgs e)
        {
            var apkList = new List<IStorageItem>();
            if (!e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                // 不包含文件信息，那么放弃
                return apkList;
            }
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count < 1)
            {
                // 数量不对，那么放弃
                return apkList;
            }
            
            foreach (var item in items)
            {
                var attributes = item.Attributes;
                if (HasAttributes(attributes, FileAttributes.Directory) || HasAttributes(attributes, FileAttributes.LocallyIncomplete))
                {
                    continue;
                }
                var fileName = item.Name;
                if (fileName.EndsWith(".apk", true, null))
                {
                    apkList.Add(item);
                }
            }
            return apkList;
        }

        private async void Install()
        {
            if (_pendingApkList.Count < 1)
            {
                return;
            }
            var apkPath = _pendingApkList[0];
            _pendingApkList.RemoveAt(0);
            _installCount ++;
            UpdateStateToLoading(_installCount, _installCount + _pendingApkList.Count);

            SendInstallCommand(_installCount, apkPath);
        }

        private async void SendInstallCommand(int taskId, string apkPath)
        {
            CommandLineHelper.Create("adb", "install", apkPath)
                .OnOutput(value => { LogInfo(taskId, value); })
                .OnError(value => { LogError(taskId, value); })
                .OnExit(() => { LogInfo(taskId, "结束"); })
                .Send();
        }
        
        private async void SendWsaCommand()
        {
            var wskCommand = "";
            CommandLineHelper.Create("adb", "install", wskCommand)
                .OnOutput(value => { LogInfo(0, value); })
                .OnError(value => { LogError(0, value); })
                .OnExit(() => { LogInfo(0, "结束"); })
                .Send();
        }

        private StringBuilder LogContent(int taskId, string[] value)
        {
            var builder = new StringBuilder();
            builder.Append(DateTime.Now.ToString("T")).Append(" -> ");
            if (taskId != 0)
            {
                builder.Append("task:").Append(taskId.ToString()).Append(' ');
            }
            foreach (var s in value)
            {
                builder.Append(s);
                builder.Append(' ');
            }
            return builder;
        }
        
        private void LogInfo(int taskId, params string[] value)
        {
            var builder = LogContent(taskId, value);
            LogModel.LogLines.Add(LogLine.Info(builder.ToString()));
        }
        
        private void LogError(int taskId, params string[] value)
        {
            var builder = LogContent(taskId, value);
            LogModel.LogLines.Add(LogLine.Error(builder.ToString()));
        }

        private Boolean HasAttributes(FileAttributes attributes, FileAttributes flag) => (attributes & flag) == flag;

        private void UpdateStateToIdel()
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            HintTextView.Text = "拖拽 APK 文件到窗口中来安装";
        }

        private void UpdateStateToReady()
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            HintTextView.Text = "松手释放拖拽的文件来安装";
        }

        private void UpdateStateToLoading(int position, int all)
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            HintTextView.Text = "正在安装 " + (position + 1) + "/" + all;
        }

    }
}
