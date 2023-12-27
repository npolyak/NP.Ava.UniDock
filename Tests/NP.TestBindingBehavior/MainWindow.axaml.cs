using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NP.Avalonia.UniDock;
using NP.Avalonia.Visuals.Behaviors;
using System.Collections.ObjectModel;

namespace NP.TestBindingBehavior
{
    public partial class MainWindow : Window
    {
        ObservableCollection<DockManagerContainer> _collection = new ObservableCollection<DockManagerContainer>();
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            RootDockGroup dockGroup = this.FindControl<RootDockGroup>("TheSimpleDockGroup");

            _collection.Add(new DockManagerContainer());
            _collection.Add(new DockManagerContainer());

            AttachedPropToCollectionBindingBehavior<DockManager, DockManagerContainer> _behavior =
                new AttachedPropToCollectionBindingBehavior<DockManager, DockManagerContainer>
                (
                    dockGroup, 
                    DockAttachedProperties.TheDockManagerProperty,
                    _collection,
                    (container, dm) => container.TheDockManager = dm);

            _collection.Add(new DockManagerContainer());

            dockGroup.TheDockManager = null;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
