using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaUI.DiagnosticsSupport;
using Microsoft.Extensions.Logging;

namespace NP.GroupCompassSample
{
    public class App : Application
    {

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
