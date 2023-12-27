// (c) Nick Polyak 2021 - http://awebpros.com/
// License: MIT License (https://opensource.org/licenses/MIT)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NP.Avalonia.UniDock;

namespace NP.Demos.UniDockWindowsSample
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
            AvaloniaXamlLoader.Load(this);
        }

        const string SerializationFilePath = "../../../SerializationResult.xml";

        public void Save()
        {
            DockManager dockManager = DockAttachedProperties.GetTheDockManager(this);

            dockManager.SaveToFile(SerializationFilePath);
        }


        public void Restore()
        {
            DockManager dockManager = DockAttachedProperties.GetTheDockManager(this);

            dockManager.RestoreFromFile(SerializationFilePath);
        }
    }
}
