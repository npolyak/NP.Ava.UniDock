using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using NP.Ava.UniDock;
using System.Linq;
using System.Threading.Tasks;

namespace NP.TablessSample
{
    public partial class MainWindow : Window
    {
        private DockManager _dockManager;

        public MainWindow()
        {

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

        }
    }
}
