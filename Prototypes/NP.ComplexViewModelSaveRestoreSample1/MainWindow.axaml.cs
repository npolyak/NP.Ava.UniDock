using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using NP.Avalonia.UniDock;
using NP.Avalonia.UniDockService;
using NP.Utilities;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace NP.ComplexViewModelSaveRestoreSample
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
            _dockManager = (DockManager) this.Resources["TheDockManager"]!;

            Button saveButton = this.FindControl<Button>("SaveButton");
            saveButton.Click += SaveButton_Click;

            Button restoreButton = this.FindControl<Button>("RestoreButton");
            restoreButton.Click += RestoreButton_Click;

            ObservableCollection<DockItemViewModelBase> vms = 
                new ObservableCollection<DockItemViewModelBase>();

            _dockManager.DockItemsViewModels = new ObservableCollection<DockItemViewModelBase>();



            Button addUserControlButton = this.FindControl<Button>("AddUserControlTabButton");

            addUserControlButton.Click += addUserControlButton_Click;
        }

        int _testNumber = 0;
        private void addUserControlButton_Click(object? sender, RoutedEventArgs e)
        {
            _testNumber++;
            var newTestTabVm = new TestDockItemViewModel
            {
                DockId = "Test" + _testNumber,
                DefaultDockGroupId = "TestUserControlsGroup",
                DefaultDockOrderInGroup = _testNumber,
                HeaderContentTemplateResourceKey = "TestUserControlHeaderDataTemplate",
                ContentTemplateResourceKey = "TestUserControlDataTemplate",
                TheVM = new object(),
                IsPredefined = false
            };

            _dockManager.DockItemsViewModels!.Add(newTestTabVm);
            newTestTabVm.IsSelected = true;
        }


        private const string SerializationFile = "Serialization.xml";
        private const string VMSerializationFile = "VMSerialization.xml";
        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            _dockManager.SaveToFile(SerializationFile);

            _dockManager.SaveViewModelsToFile(VMSerializationFile);
        }

        private void RestoreButton_Click(object? sender, RoutedEventArgs e)
        {
            _dockManager.DockItemsViewModels = null;
            _dockManager.RestoreFromFile(SerializationFile);

            _dockManager
                .RestoreViewModelsFromFile
                (
                    VMSerializationFile,
                    typeof(TestDockItemViewModel));

            _dockManager.DockItemsViewModels?.OfType<TestDockItemViewModel>().FirstOrDefault()?.Select();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }

    public class UserControlDockItemViewModel : DockItemViewModel<UserControl>
    {
    }
}
