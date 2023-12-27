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
            _dockManager = MyContainer.TheDockManager;

            Button saveButton = this.FindControl<Button>("SaveButton");
            saveButton.Click += SaveButton_Click;

            Button restoreButton = this.FindControl<Button>("RestoreButton");
            restoreButton.Click += RestoreButton_Click;

            ObservableCollection<DockItemViewModelBase> vms = 
                new ObservableCollection<DockItemViewModelBase>();

            _dockManager.DockItemsViewModels = new ObservableCollection<DockItemViewModelBase>();

            Button addStockButton = this.FindControl<Button>("AddStockTabButton");

            addStockButton.Click += AddStockButton_Click;


            Button addOrderButton = this.FindControl<Button>("AddOrderTabButton");

            addOrderButton.Click += AddOrderButton_Click;
        }


        #region CanAddStock Styled Avalonia Property
        public bool CanAddStock
        {
            get { return GetValue(CanAddStockProperty); }
            set { SetValue(CanAddStockProperty, value); }
        }

        public static readonly StyledProperty<bool> CanAddStockProperty =
            AvaloniaProperty.Register<MainWindow, bool>
            (
                nameof(CanAddStock),
                true
            );
        #endregion CanAddStock Styled Avalonia Property

        private int _numberStocks = 0;

        private static StockViewModel IBM =
            new StockViewModel
            {
                Symbol = "IBM",
                Description = "Internation Business Machines",
                Ask = 51,
                Bid = 49
            };

        private static StockViewModel MSFT =
            new StockViewModel
            {
                Symbol = "MSFT",
                Description = "Microsoft",
                Ask = 101,
                Bid = 99
            };

        private static StockViewModel[] Stocks =
        {
            IBM,
            MSFT
        };

        private void AddStockButton_Click(object? sender, RoutedEventArgs e)
        {
            var stock = Stocks[_numberStocks];
            string? stockName = stock.Symbol;

            var newTabVm = new StockDockItemViewModel
            {
                DockId = stockName,
                DefaultDockGroupId = "StockGroup",
                DefaultDockOrderInGroup = _numberStocks,
                HeaderContentTemplateResourceKey= "StockHeaderDataTemplate",
                ContentTemplateResourceKey= "StockDataTemplate",
                TheVM = stock,
                IsPredefined = false
            };

            _dockManager.DockItemsViewModels!.Add(newTabVm);

            newTabVm.IsSelected = true;

            _numberStocks++;

            if (_numberStocks >= 2)
            {
                CanAddStock = false;
            }
        }

        int _numberOrders = 0;
        private void AddOrderButton_Click(object? sender, RoutedEventArgs e)
        {
            var stock = Stocks[_numberOrders % 2];
            OrderViewModel orderVM = new OrderViewModel
            {
                Symbol = stock.Symbol,
                MarketPrice = (stock.Ask + stock.Bid) / 2m,
                NumberShares = (_numberOrders + 1) * 1000
            };


            var newTabVm = new OrderDockItemViewModel
            {
                DockId = "Order" + _numberOrders + 1,
                DefaultDockGroupId = "OrdersGroup",
                DefaultDockOrderInGroup = _numberOrders,
                HeaderContentTemplateResourceKey = "OrderHeaderDataTemplate",
                ContentTemplateResourceKey = "OrderDataTemplate",
                IsPredefined = false,
                TheVM = orderVM
            };

            _dockManager.DockItemsViewModels!.Add(newTabVm);

            _numberOrders++;
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
                    typeof(StockDockItemViewModel),
                    typeof(OrderDockItemViewModel));

            _dockManager.DockItemsViewModels?.OfType<StockDockItemViewModel>().FirstOrDefault()?.Select();
            _dockManager.DockItemsViewModels?.OfType<OrderDockItemViewModel>().FirstOrDefault()?.Select();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
