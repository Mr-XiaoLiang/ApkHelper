using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
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

        private List<string> PendingApkList = new();
        private int InstallCount = 0;

        public MainWindow()
        {
            this.InitializeComponent();
            InitWindow();
            UpdateStateToIdel();
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
            window.MoveAndResize(new Windows.Graphics.RectInt32(windowLeft, windowTop, windowWidth, windowHeight));
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
                PendingApkList.Add(apk.Path)
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
                if (HasAttributes(attributes, Windows.Storage.FileAttributes.Directory) || HasAttributes(attributes, Windows.Storage.FileAttributes.LocallyIncomplete))
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
            if (PendingApkList.Count < 1)
            {
                return;
            }
            var apkPath = PendingApkList[0];
            PendingApkList.RemoveAt(0);
            InstallCount ++;
            UpdateStateToLoading(InstallCount, InstallCount + PendingApkList.Count);


        }

        private Boolean HasAttributes(Windows.Storage.FileAttributes attributes, Windows.Storage.FileAttributes flag) => (attributes & flag) == flag;

        private void UpdateStateToIdel()
        {
            progressBar.Visibility = Visibility.Collapsed;
            progressBar.IsIndeterminate = false;
            hintTextView.Text = "拖拽 APK 文件到窗口中来安装";
        }

        private void UpdateStateToReady()
        {
            progressBar.Visibility = Visibility.Collapsed;
            progressBar.IsIndeterminate = false;
            hintTextView.Text = "松手释放拖拽的文件来安装";
        }

        private void UpdateStateToLoading(int position, int all)
        {
            progressBar.Visibility = Visibility.Collapsed;
            progressBar.IsIndeterminate = false;
            hintTextView.Text = "正在安装 " + (position + 1) + "/" + all;
        }

    }
}
