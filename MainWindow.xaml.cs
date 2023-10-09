using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using Windows.Storage;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
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
        private WsaHelper _wsaHelper;

        public MainWindow()
        {
            InitializeComponent();
            InitWindow();
            InitWsaHelper();
            UpdateStateToIdle();
            LogInfo("", "准备就绪");
        }

        private void InitWindow()
        {
            LogErrorButton.IsChecked = LogLine.ShowError;
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

        private void InitWsaHelper()
        {
            _wsaHelper = new WsaHelper(
                this,
                (error, tag, value) =>
                {
                    if (error)
                    {
                        LogError(tag, value);
                    }
                    else
                    {
                        LogInfo(tag, value);
                    }
                });
            if (WsaHelper.CheckWsa())
            {
                LogInfo("", "WSA 正在运行");
            }
            else
            {
                LogError("", "WSA 没有启动");
            }

            _wsaHelper.CheckAdb();
        }

        public AppWindow GetAppWindow()
        {
            var windowHandler = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(windowHandler);
            return AppWindow.GetFromWindowId(windowId);
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "安装";
        }

        private async void OnDropAsync(object sender, DragEventArgs e)
        {
            var apkList = await GetDragItemsAsync(e);
            if (apkList.Count < 1)
            {
                // 数量不对，那么放弃
                UpdateStateToIdle();
                return;
            }

            apkList.ForEach(apk =>
                _pendingApkList.Add(apk.Path)
            );
            Install();
        }

        private void LogErrorChecked(object sender, RoutedEventArgs e)
        {
            LogLine.ShowError = true;
        }

        private void LogErrorUnchecked(object sender, RoutedEventArgs e)
        {
            LogLine.ShowError = false;
        }

        private static async Task<List<IStorageItem>> GetDragItemsAsync(DragEventArgs e)
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
                if (HasAttributes(attributes, FileAttributes.Directory) ||
                    HasAttributes(attributes, FileAttributes.LocallyIncomplete))
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

        private void Install()
        {
            if (_pendingApkList.Count < 1)
            {
                _installCount = 0;
                UpdateStateToIdle();
                return;
            }

            var apkPath = _pendingApkList[0];
            _pendingApkList.RemoveAt(0);
            _installCount++;
            UpdateStateToLoading(_installCount, _installCount + _pendingApkList.Count);
            // 安装完成一个之后，触发下一个
            _wsaHelper.SendInstallCommand(_installCount.ToString(), apkPath, Install);
        }

        private static StringBuilder LogContent(string tag, string value)
        {
            var builder = new StringBuilder();
            builder.Append(DateTime.Now.ToString("T")).Append(" -> ");
            if (tag is { Length: > 0 })
            {
                builder.Append(tag).Append(" => ");
            }

            if (value is not { Length: > 0 }) return builder;
            builder.Append(value);
            builder.Append(' ');
            return builder;
        }

        private void LogInfo(string tag, string value)
        {
            var builder = LogContent(tag, value);
            RunUi(() =>
            {
                var logLine = LogLine.Info(builder.ToString());
                LogModel.LogLines.Add(logLine);
                LogView.ScrollIntoView(logLine);
            });
        }

        private void LogError(string tag, string value)
        {
            var builder = LogContent(tag, value);
            RunUi(() =>
            {
                var logLine = LogLine.Error(builder.ToString());
                LogModel.LogLines.Add(logLine);
                LogView.ScrollIntoView(logLine);
            });
        }

        private void RunUi(DispatcherQueueHandler agileCallback)
        {
            DispatcherQueue.TryEnqueue(agileCallback);
        }

        private static bool HasAttributes(FileAttributes attributes, FileAttributes flag) =>
            (attributes & flag) == flag;

        private void UpdateStateToIdle()
        {
            RunUi(() =>
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                ProgressBar.IsIndeterminate = false;
                HintTextView.Text = "拖拽 APK 文件到窗口中来安装";
            });
        }

        private void UpdateStateToReady()
        {
            RunUi(() =>
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                ProgressBar.IsIndeterminate = false;
                HintTextView.Text = "松手释放拖拽的文件来安装";
            });
        }

        private void UpdateStateToLoading(int position, int all)
        {
            RunUi(() =>
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                ProgressBar.IsIndeterminate = false;
                HintTextView.Text = "正在安装 " + (position) + "/" + all;
            });
        }
    }
}