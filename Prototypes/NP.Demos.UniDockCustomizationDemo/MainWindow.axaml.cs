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
using NP.Avalonia.Visuals;
using NP.Avalonia.Visuals.Behaviors;
using NP.Avalonia.Visuals.Controls;
using NP.Avalonia.UniDock;
using NP.Avalonia.UniDock.Serialization;
using NP.Utilities;
using System;
using System.IO;

namespace NP.Demos.DockWindowsSample
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            CurrentScreenPointBehavior.CurrentScreenPoint.Subscribe(OnCurrentScreenPointChanged);


        }

        private void OnCurrentScreenPointChanged(Point2D screenPoint)
        {
            AttachedProperties.SetCurrentScreenPoint(this, screenPoint);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void SetButtonBounds()
        {
            Button button = this.FindControl<Button>("TheButton");

            ButtonBounds = button.GetScreenBounds();
        }

        const string SerializationFilePath = "../../../../SerializationResult.xml";

        #region ButtonBounds Styled Avalonia Property
        public Rect2D ButtonBounds
        {
            get { return GetValue(ButtonBoundsProperty); }
            private set { SetValue(ButtonBoundsProperty, value); }
        }

        public static readonly StyledProperty<Rect2D> ButtonBoundsProperty =
            AvaloniaProperty.Register<MainWindow, Rect2D>
            (
                nameof(ButtonBounds)
            );
        #endregion ButtonBounds Styled Avalonia Property

        public void Serialize()
        {
            DockManager dockManager = DockAttachedProperties.GetTheDockManager(this);

            dockManager.SaveToFile(SerializationFilePath);
        }


        public void Restore()
        {
            DockManager dockManager = DockAttachedProperties.GetTheDockManager(this);

            dockManager.RestoreFromFile(SerializationFilePath);
        }

        public void ClearGroups()
        {
            DockManager dockManager = DockAttachedProperties.GetTheDockManager(this);

            dockManager.ClearGroups();
        }

    }
}
