using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using NP.Avalonia.UniDock;
using NP.Avalonia.UniDockService;
using System.Collections.ObjectModel;
using System.Linq;

namespace NP.ViewModelSample
{
    public partial class MainWindow : Window
    {
        private DockManager _dockManager;

        private ObservableCollection<DockItemViewModelBase> _vms;

        public ObservableCollection<DockItemViewModelBase> VMs => _vms;
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            _dockManager = MyContainer.TheDockManager;

            Opened += MainWindow_Opened;

            Button saveButton = this.FindControl<Button>("SaveButton");
            saveButton.Click += SaveButton_Click;

            Button restoreButton = this.FindControl<Button>("RestoreButton");
            restoreButton.Click += RestoreButton_Click;

            ObservableCollection<DockItemViewModelBase> vms = 
                new ObservableCollection<DockItemViewModelBase>();

            _vms = vms;

            DockItemViewModelBase vm1 = new DockItemViewModelBase
            {
                DockId = "Tab1",
                Header = "Tab1",
                DefaultDockGroupId = "Group1",
                DefaultDockOrderInGroup = 1,
                Content = "Hello World!"
            };

            vms.Add(vm1);


            DockItemViewModelBase vm2 = new DockItemViewModelBase
            {
                DockId = "Tab2",
                Header = "Tab2",
                DefaultDockGroupId = "Group1",
                DefaultDockOrderInGroup = 2,
                Content = "Hi World!",
                ContentTemplateResourceKey = "TheDataTemplate"
            };

            vms.Add(vm2);

            vm1.IsActive = true;

            _dockManager.DockItemsViewModels = vms;

            DockItemViewModelBase vm3 = new DockItemViewModelBase
            {
                DockId = "Tab3",
                Header = "Tab3",
                DefaultDockGroupId = "Group2",
                DefaultDockOrderInGroup = 0,
                Content = "3333"
            };

            vms.Add(vm3);

            DockItemViewModelBase vm4 = new DockItemViewModelBase
            {
                DockId = "Tab4",
                Header = "Tab4",
                DefaultDockGroupId = "Group2",
                DefaultDockOrderInGroup = 1,
                Content = "4444"
            };

            vms.Add(vm4);
            DockItemViewModelBase floatingVm1 = new DockItemViewModelBase
            {
                DockId = "FloatingDockItem1",
                Header = "FloatingWindowPanel1",
                DefaultDockGroupId = "FloatingGroup1",
                DefaultDockOrderInGroup = 1,
                Content = "Floating Panel"
            };

            vms.Add(floatingVm1);

            DockItemViewModelBase floatingVm2 = new DockItemViewModelBase
            {
                DockId = "FloatingDockItem2",
                Header = "Floating Tab 1",
                DefaultDockGroupId = "FloatingGroup2",
                DefaultDockOrderInGroup = 1,
                Content = "Floating Tab 1"
            };

            vms.Add(floatingVm2);

            DockItemViewModelBase floatingVm3 = new DockItemViewModelBase
            {
                DockId = "FloatingDockItem3",
                Header = "Floating Tab 2",
                DefaultDockGroupId = "FloatingGroup2",
                DefaultDockOrderInGroup = 2,
                Content = "Floating Tab 2"
            };

            vms.Add(floatingVm3);

            // object? result = this.FindResource("DefaultWindowTitleAreaDataTemplate");

            Button mainTabsButton = this.FindControl<Button>("AddMainTabButton");

            mainTabsButton.Click += MainTabsButton_Click;


            Button floatingTabsButton = this.FindControl<Button>("AddFloatingTabButton");

            floatingTabsButton.Click += FloatingTabsButton_Click;
        }

        int _tabNumber = 5;
        private void MainTabsButton_Click(object? sender, RoutedEventArgs e)
        {
            string tabStr = $"Tab{_tabNumber}";

            var newTabVm = new DockItemViewModelBase
            {
                DockId = tabStr,
                Header = tabStr,
                DefaultDockGroupId = "Group2",
                DefaultDockOrderInGroup = _tabNumber,
                Content = tabStr,
                IsPredefined = false
            };
            _vms.Add(newTabVm);

            newTabVm.IsSelected = true;

            _tabNumber++;
        }

        int _floatingTabNumber = 3;
        private void FloatingTabsButton_Click(object? sender, RoutedEventArgs e)
        {
            string floatingTabStr = $"FloatingTab{_floatingTabNumber}";

            var newTabVm = new DockItemViewModelBase
            {
                DockId = floatingTabStr,
                Header = floatingTabStr,
                DefaultDockGroupId = "FloatingGroup2",
                DefaultDockOrderInGroup = _floatingTabNumber,
                Content = floatingTabStr,
                IsPredefined = false
            };

            _vms.Add(newTabVm);

            _floatingTabNumber++;
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
