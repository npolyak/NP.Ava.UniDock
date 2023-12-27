using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using NP.Avalonia.UniDock;
using NP.Concepts.Behaviors;
using System.Linq;

namespace NP.DockItemsInMenus
{
    public partial class MainWindow : Window
    {
        private DockManager _dockManager;

        public ExpandoObjectBehavior<IDockGroup> DockIdKeysGroups { get; }

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            _dockManager = (DockManager) this.FindResource("TheDockManager")!;

            DockIdKeysGroups =
                new ExpandoObjectBehavior<IDockGroup>
                (
                    _dockManager.AllGroupsBehavior.Result,
                    group => group.DockId,
                    group => group.GetObservable(DockGroupBaseControl.DockIdProperty));

            Opened += MainWindow_Opened;

            Button saveButton = this.FindControl<Button>("SaveButton");
            saveButton.Click += SaveButton_Click;

            Button restoreButton = this.FindControl<Button>("RestoreButton");
            restoreButton.Click += RestoreButton_Click;
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
