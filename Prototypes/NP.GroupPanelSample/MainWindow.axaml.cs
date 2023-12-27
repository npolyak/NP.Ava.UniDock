using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NP.Avalonia.UniDock;
using NP.Avalonia.Visuals.Controls;
using NP.Utilities;

namespace NP.GroupPanelSample
{
    public partial class MainWindow : Window
    {
        StackGroup _stackGroup;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            _stackGroup = this.FindControl<StackGroup>("TheStackGroup");

            //_stackGroup.Items.Add(new Grid { Background = new SolidColorBrush(Colors.Red) });
            //_stackGroup.Items.Add(new Grid { Background = new SolidColorBrush(Colors.Green) });
            //_stackGroup.Items.Insert(0, new Grid { Background = new SolidColorBrush(Colors.Yellow) });
            //_stackGroup.Items.Insert(2, new Grid { Background = new SolidColorBrush(Colors.Blue) });
            //_stackGroup.Items.Insert(3, new Grid { Background = new SolidColorBrush(Colors.Purple) });
        }

        private void OnCurrentScreenPointChanged(Point2D screenPoint)
        {
            AttachedProperties.SetCurrentScreenPoint(this, screenPoint);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void RemoveFirst()
        {
            _stackGroup.Items.RemoveAt(0);
        }

        public void RemoveLast()
        {
            _stackGroup.Items.RemoveAt(_stackGroup.Items.Count() - 1);
        }

        public void RemoveSecond()
        {
            _stackGroup.Items.RemoveAt(1);
        }
    }
}
