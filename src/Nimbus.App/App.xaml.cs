using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Navigation;
using Nimbus.Core.Services;
using Nimbus.Core.ViewModels;

namespace Nimbus.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window window = Window.Current;
        private readonly ServiceProvider _serviceProvider;
        private static readonly string StartupLogPath = Path.Combine(Path.GetTempPath(), "nimbus-startup.log");

        public static IServiceProvider Services { get; private set; } = default!;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            HookGlobalExceptionLogging();

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            Services = _serviceProvider;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            window ??= new Window();
            window.Title = "Nimbus";

            if (window.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                window.Content = rootFrame;
            }

            window.Activate();

            try
            {
                var navigated = rootFrame.Navigate(typeof(MainPage), e.Arguments);
                if (!navigated)
                {
                    var message = "Main page navigation returned false.";
                    LogStartupFailure("OnLaunched", message, null);
                    ShowFatalStartupMessage(message);
                }
            }
            catch (Exception ex)
            {
                LogStartupFailure("OnLaunched", "Exception during main-page navigation.", ex);
                ShowFatalStartupMessage($"Nimbus failed to start.\n\n{ex.Message}\n\nSee log:\n{StartupLogPath}");
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IShellItemService, ShellItemService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IFileOperationsService, FileOperationsService>();
            services.AddSingleton<ISearchService, SearchService>();
            services.AddSingleton<IViewPreferenceService, ViewPreferenceService>();
            services.AddSingleton<IFilePreviewService, FilePreviewService>();

            services.AddSingleton<SidebarViewModel>();
            services.AddSingleton<FileListViewModel>();
            services.AddSingleton<NavigationViewModel>();
            services.AddSingleton<MainPageViewModel>();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            var pageName = e.SourcePageType?.FullName ?? "(unknown)";
            var message = $"Failed to load page: {pageName}";
            LogStartupFailure("OnNavigationFailed", message, e.Exception);
            ShowFatalStartupMessage($"{message}\n\nSee log:\n{StartupLogPath}");
        }

        private void HookGlobalExceptionLogging()
        {
            UnhandledException += OnAppUnhandledException;

            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    LogStartupFailure("AppDomain.UnhandledException", "Unhandled app-domain exception.", ex);
                }
                else
                {
                    LogStartupFailure("AppDomain.UnhandledException", "Unhandled non-exception app-domain error.", null);
                }
            };

            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                LogStartupFailure("TaskScheduler.UnobservedTaskException", "Unobserved task exception.", args.Exception);
                args.SetObserved();
            };
        }

        private void OnAppUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            LogStartupFailure("Application.UnhandledException", "Unhandled UI exception.", e.Exception);
            e.Handled = true;
            ShowFatalStartupMessage($"Unhandled startup error.\n\n{e.Message}\n\nSee log:\n{StartupLogPath}");
        }

        private static void LogStartupFailure(string source, string message, Exception? exception)
        {
            try
            {
                var lines = new[]
                {
                    "---- Nimbus Startup Failure ----",
                    $"UTC: {DateTime.UtcNow:O}",
                    $"Source: {source}",
                    $"Message: {message}",
                    exception?.ToString() ?? "(no exception object)",
                    string.Empty
                };
                File.AppendAllLines(StartupLogPath, lines);
            }
            catch
            {
                // Swallow logging exceptions to avoid recursive failures during startup.
            }
        }

        private void ShowFatalStartupMessage(string message)
        {
            try
            {
                window ??= new Window();
                var text = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.WrapWholeWords,
                    Margin = new Thickness(20)
                };

                window.Content = new ScrollViewer
                {
                    Content = text
                };
                window.Activate();
            }
            catch
            {
                // If rendering a fatal message fails, we already logged the root issue.
            }
        }
    }
}
