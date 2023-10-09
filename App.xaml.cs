﻿using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            SetTheme(m_window);
            m_window.Activate();
            // Hide default title bar.
            m_window.ExtendsContentIntoTitleBar = true;
        }

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
                PrimaryButtonText = primary,
                CloseButtonText = close,
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = window.Content.XamlRoot,
            };

            return await alertDialog.ShowAsync();
        }
    }
}