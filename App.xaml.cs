using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ApkHelper
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private MainWindow m_window;

        private static App _app;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            _app = this;
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        /// https://learn.microsoft.com/zh-cn/uwp/api/windows.ui.xaml.application.onlaunched?view=winrt-22621
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            SetTheme(m_window);
            m_window.Activate();
            // Hide default title bar.
            m_window.ExtendsContentIntoTitleBar = true;
        }

        // https://learn.microsoft.com/zh-cn/uwp/api/windows.ui.xaml.application.onactivated?view=winrt-22621
        // protected virtual void OnActivated(IActivatedEventArgs args)
        // {
        //     
        // }
        
        // https://learn.microsoft.com/zh-cn/uwp/api/windows.ui.xaml.application.onfileopenpickeractivated?view=winrt-22621
        protected virtual void OnFileOpenPickerActivated(FileOpenPickerActivatedEventArgs args)
        {
            
        }

        // https://learn.microsoft.com/zh-cn/uwp/api/windows.ui.xaml.application.onfileactivated?view=winrt-22621
        protected virtual void OnFileActivated(FileActivatedEventArgs args)
        {
            
        }
        
        // https://learn.microsoft.com/zh-cn/previous-versions/windows/apps/hh779669(v=win.10)

        private static void SetTheme(Window window)
        {
            var uiElement = window.Content;
            if (uiElement is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ElementTheme.Dark;
            }
        }

        public static ElementTheme CurrentTheme()
        {
            if (_app == null || _app.m_window == null)
            {
                return ElementTheme.Default;
            }

            var uiElement = _app.m_window.Content;
            if (uiElement is FrameworkElement rootElement)
            {
                return rootElement.RequestedTheme;
            }

            return ElementTheme.Default;
        }

        public static async Task<ContentDialogResult> ShowDialog(
            Window window,
            string title,
            string content,
            string primary,
            string close
        )
        {
            var alertDialog = new ContentDialog()
            {
                Title = title,
                Content = content,
                CloseButtonText = close,
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = window.Content.XamlRoot,
            };

            if (!string.IsNullOrEmpty(primary))
            {
                alertDialog.PrimaryButtonText = primary;
            }

            return await alertDialog.ShowAsync();
        }
    }
}