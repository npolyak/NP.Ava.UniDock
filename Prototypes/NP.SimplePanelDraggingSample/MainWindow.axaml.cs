using System;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using NP.Ava.Visuals;
using NP.Ava.Visuals.Controls;
using NP.Utilities;

namespace NP.SimplePanelDraggingSample
{
    public partial class MainWindow : CustomWindow
    {
        private Point2D<double>? _startMousePoint;

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

            var runtimeId = RuntimeInformation.RuntimeIdentifier;

            DraggableHeader.PointerPressed += OnPointerPressed;
        }

        private void OnPointerPressed(object? sender, PointerEventArgs e)
        {
            _startMousePoint = e.GetPosition(DraggableHeader).ToPoint2D();

            e.Pointer.Capture(DraggableHeader);

            DraggableHeader.PointerMoved += OnPointerMoved;

        }
        
        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            Point2D<double> currentMousePoint = e.GetPosition(DraggableHeader).ToPoint2D();

            if ( currentMousePoint
                    .Minus(_startMousePoint!)
                    .ToAbs()
                    .Less(PointHelper.MinimumDragDistance).Any )
            {
                return;
            }

            //var positionInScreen = this.PointToScreen(e.GetPosition())
            
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
            
            _customWindow.Closed += CustomWindowOnClosed;


            _customWindow.Show();
        }

        private void AnotherDraggableHeaderOnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_customWindow?.IsLoaded != true)
            {
                return;
            }

            e.Pointer.Capture(null);

            var headerControl = CustomWindowHeaderControl;
            e.Pointer.Capture(headerControl);

            if (headerControl == null)
                return;
            
            DraggableHeader.PointerMoved -= AnotherDraggableHeaderOnPointerMoved;

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


        private void CustomWindowOnClosed(object? sender, EventArgs e)
        {
            _customWindow = null;
        }
    }
}