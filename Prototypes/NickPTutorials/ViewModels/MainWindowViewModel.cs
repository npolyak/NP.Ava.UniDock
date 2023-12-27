using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace NickPTutorials.ViewModels
{
    class MainWindowViewModel
    {
        public void FileExit()
        {
            System.Diagnostics.Debug.WriteLine("Program exit requested!!!!!");

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Shutdown();
            }
        }
    }
}
