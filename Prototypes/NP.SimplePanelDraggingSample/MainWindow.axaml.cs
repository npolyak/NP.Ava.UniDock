using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using NP.Ava.Visuals;
using NP.Ava.Visuals.Behaviors;
using NP.Ava.Visuals.Controls;
using NP.Utilities;

namespace NP.SimplePanelDraggingSample
{
    public partial class MainWindow : CustomWindow
    {
        private Point2D? _startMousePoint;

        private CustomWindow _customWindow;

        public Control CustomWindowHeaderControl
        {
            get
            {
                return
                    _customWindow
                        ?.GetVisualDescendants()
                        ?.OfType<TemplatedControl>()
                        ?.FirstOrDefault(c => c.Name == "PART_HeaderControl");
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DraggableHeader.PointerPressed += OnPointerPressed;
        }

        private void OnPointerPressed(object? sender, PointerEventArgs e)
        {
            _startMousePoint = e.GetPosition(DraggableHeader).ToPoint2D();

            e.Pointer.Capture(DraggableHeader);

            DraggableHeader.PointerMoved += OnPointerMoved;

        }

        private bool _afterLoaded = false;

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            Point2D currentMousePoint = e.GetPosition(DraggableHeader).ToPoint2D();

            if (
                currentMousePoint
                .Minus(_startMousePoint!)
                .ToAbs()
                .Less(PointHelper.MinimumDragDistance).Any)
            {
                return;
            }

            //var positionInScreen = this.PointToScreen(e.GetPosition())

            _afterLoaded = false;
            e.Pointer.Capture(null);

            DraggableHeader.PointerMoved -= OnPointerMoved;

            _customWindow?.Close();

            _customWindow = new CustomWindow()
            {
                Width = 600,
                Height = 600,
                Classes = { "PlainCustomWindow" }
            };

            DraggableHeader.PointerMoved += AnotherDraggableHeaderOnPointerMoved;

            _customWindow.Loaded += CustomWindowOnLoaded;

            _customWindow.Activated += CustomWindowOnActivated;

            _customWindow.Initialized += CustomWindowOnInitialized;

            _customWindow.Opened += CustomWindowOnOpened;

            _customWindow.Closed += CustomWindowOnClosed;


            _customWindow.Show();
        }

        private void AnotherDraggableHeaderOnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!_afterLoaded || _customWindow == null)
            {
                return;
            }

            e.Pointer.Capture(null);

            var headerControl = CustomWindowHeaderControl;
            e.Pointer.Capture(headerControl);

            if (headerControl == null)
                return;
            
            DraggableHeader.PointerMoved -= AnotherDraggableHeaderOnPointerMoved;
            
            //headerControl.PointerPressed += HeaderControlOnPointerPressed;
            //headerControl.PointerEntered += HeaderControlOnPointerEntered;

            headerControl.PointerMoved += CustomWindowOnPointerMoved;
            headerControl.PointerReleased += HeaderControlOnPointerReleased;
        }

        private void HeaderControlOnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            
        }

        private void CustomWindowOnPointerMoved(object? sender, PointerEventArgs e)
        {
            var headerControl = CustomWindowHeaderControl;
            headerControl.PointerMoved -= CustomWindowOnPointerMoved;
            _customWindow.SetDragWindowOnMovePointer(e);
        }

        private void HeaderControlOnPointerEntered(object? sender, PointerEventArgs e)
        {

        }


        private void CustomWindowOnClosed(object? sender, EventArgs e)
        {
            _customWindow = null;
        }

        private void CustomWindowOnOpened(object? sender, EventArgs e)
        {
            
        }

        private void CustomWindowOnInitialized(object? sender, EventArgs e)
        {
            
        }

        private void CustomWindowOnPointerEntered(object? sender, PointerEventArgs e)
        {
            
        }

        private void CustomWindowOnActivated(object? sender, EventArgs e)
        {
            
        }

        private void CustomWindowOnLoaded(object? sender, RoutedEventArgs e)
        {


            _afterLoaded = true;

            //CurrentScreenPointBehavior.Capture(headerControl);

            //headerControl.AddHandler(PointerMovedEvent, HeaderControlOnPointerMoved, RoutingStrategies.Tunnel, true );
        }

        private void HeaderControlOnPointerMoved(object? sender, PointerEventArgs e)
        {
            
        }
    }
}