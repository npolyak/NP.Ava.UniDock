using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using NP.Avalonia.UniDock;
using System.Linq;
using System.Threading.Tasks;

namespace NP.DataContextSample
{
    public partial class MainWindow : Window
    {
        private DockManager _dockManager;

        public MainWindow()
        {
            this.Closing += OnMainWindowClosing;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            _dockManager = MyContainer.TheDockManager;

            Opened += MainWindow_Opened;

            Button changeDockDataContext = this.FindControl<Button>("ChangeDockDataContextButton");
            changeDockDataContext.Click += ChangeDockDataContext_Click;

            Button saveButton = this.FindControl<Button>("SaveButton");
            saveButton.Click += SaveButton_Click;

            Button restoreButton = this.FindControl<Button>("RestoreButton");
            restoreButton.Click += RestoreButton_Click;

        }

        private async void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Window OnMainWindowClosing");
            DockManager dockManager = DockAttachedProperties.GetTheDockManager(this);
            dockManager.SaveToFile(SerializationFile);
        }


        private void ChangeDockDataContext_Click(object? sender, RoutedEventArgs e)
        {
            TestVM vm = (TestVM) this.Resources["TheViewModel"]!;

            vm.TheStr = "Hi World";
        }

        private const string SerializationFile = "Serialization.xml";
        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            _dockManager.SaveToFile(SerializationFile);
        }

        private void RestoreButton_Click(object? sender, RoutedEventArgs e)
        {
            _dockManager.RestoreFromFile(SerializationFile);
        }

        private void MainWindow_Opened(object? sender, System.EventArgs e)
        {
            var w = (FloatingWindow)(App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Windows[1];

            var visualDescendants = 
                w.GetVisualDescendants().ToList();
        }

        private void MainWindow_Initialized(object? sender, System.EventArgs e)
        {
            
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
