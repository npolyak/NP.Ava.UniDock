using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NP.Avalonia.UniDock;

namespace NickPTutorials
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            this.Closing += OnMainWindowClosing;
            AvaloniaXamlLoader.Load(this);
            //this.Closing += OnMainWindowClosing;
        }

        private void DisplayWindowStatus(DockManager dockManager)
        {
            System.Diagnostics.Debug.WriteLine("---- All ----");

            foreach (Avalonia.Controls.Window window in dockManager.AllWindows)
            {
                System.Diagnostics.Debug.WriteLine(window);
            }

            System.Diagnostics.Debug.WriteLine("---- Floating ----");


            System.Diagnostics.Debug.WriteLine("---- Disconnected ----");

            foreach (var window in dockManager.DisconnectedGroups)
            {
                System.Diagnostics.Debug.WriteLine(window);
            }

            System.Diagnostics.Debug.WriteLine("-------------");
        }

        private void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            
            //System.Diagnostics.Debug.WriteLine("Window OnMainWindowClosing");
            //DockManager dockManager = DockAttachedProperties.GetTheDockManager(this);
            //DisplayWindowStatus(dockManager);
            //dockManager.SaveToFile(SerializationFilePath);
        }

        const string SerializationFilePath = "../../../SerializationResult.xml";

        public void Save()
        {
            DockManager dockManager = DockAttachedProperties.GetTheDockManager(this);
            DisplayWindowStatus(dockManager);
            dockManager.SaveToFile(SerializationFilePath);
        }


        public void Restore()
        {
            DockManager dockManager = DockAttachedProperties.GetTheDockManager(this);

            dockManager.RestoreFromFile(SerializationFilePath);
        }

    }


}
