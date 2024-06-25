using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using NP.Ava.UniDock;
using NP.Ava.Visuals;
using System;
using System.Linq;

namespace NP.DockItemsWidthSample
{
    public partial class MainWindow : Window
    {
        DockManager _dockManager;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Closed += MainWindow_Closed;

            SaveLayoutButton.Click += SaveLayoutButton_Click;
            RestoreLayoutButton.Click += RestoreLayoutButton_Click;

            _dockManager =
                DockAttachedProperties.GetTheDockManager(TheRootGroup);
        }

        private void SaveLayoutButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _dockManager.SaveToTmpStr();
        }

        private void RestoreLayoutButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _dockManager.RestoreFromTmpStr();
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
