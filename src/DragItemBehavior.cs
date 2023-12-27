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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using NP.Avalonia.Visuals;
using NP.Avalonia.Visuals.Behaviors;
using NP.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NP.Avalonia.UniDock
{
    public abstract class DragItemBehavior<TItem>
        where TItem : Control, IControl
    {
        protected static DragItemBehavior<TItem>? Instance { get; set; }

        protected static void OnIsSetChanged(AvaloniaPropertyChangedEventArgs<bool> change)
        {
            Control itemsContainer = (Control)change.Sender;

            if (change.NewValue.Value)
            {
                itemsContainer.AddHandler
                (
                    Control.PointerPressedEvent,
                    Instance!.Control_PointerPressed!,
                    RoutingStrategies.Bubble,
                    false);
            }
            else
            {
                itemsContainer.RemoveHandler(Control.PointerPressedEvent, Instance!.Control_PointerPressed!);
            }
        }

        //static DragItemBehavior()
        //{
        //    IsSetProperty.Changed.Subscribe(OnIsSetChanged);
        //}

        private Point2D? _startMousePoint;

        private bool _allowDrag = false;

        protected TItem? _startItem;

        protected IDockGroup? _draggedDockGroup;

        protected Func<TItem, IDockGroup> _dockGroupGetter;

        public DragItemBehavior(Func<TItem, IDockGroup> dockGroupGetter)
        {
            _dockGroupGetter = dockGroupGetter;
        }

        private void Control_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            Control itemsContainer = (Control)sender;

            if (!itemsContainer.IsLeftMousePressed(e))
                return;

            _startMousePoint = e.GetPosition(itemsContainer).ToPoint2D();

            _startItem = e.GetControlUnderCurrentMousePosition<TItem>((Control)sender)!;

            if (_startItem == null)
            {
                return;
            }

            _draggedDockGroup = _dockGroupGetter.Invoke(_startItem);

            if (!_draggedDockGroup.CanFloat)
            {
                return;
            }
            
            CurrentScreenPointBehavior.Capture(itemsContainer);

            if (CurrentScreenPointBehavior.CapturedControl != itemsContainer)
                return;

            itemsContainer.AddHandler
            (
                Control.PointerReleasedEvent,
                ClearHandlers!,
                RoutingStrategies.Bubble,
                false);

            itemsContainer.AddHandler
            (
                Control.PointerMovedEvent,
                OnDragPointerMoved!,
                RoutingStrategies.Bubble,
                false);

            _allowDrag = false;
        }

        private void ClearHandlers(object sender, PointerEventArgs e)
        {
            ClearHandlers(sender);
        }

        private void ClearHandlers(object sender)
        {
            Control control = (Control)sender;

            control.RemoveHandler(Control.PointerReleasedEvent, ClearHandlers!);
            control.RemoveHandler(Control.PointerMovedEvent, OnDragPointerMoved!);
        }

        protected abstract bool MoveItemWithinContainer(Control itemsContainer, PointerEventArgs e);

        protected virtual async void OnDragPointerMoved(object sender, PointerEventArgs e)
        {
            if (_startItem == null)
            {
                return;
            }

            Control itemsContainer = (Control)sender;

            DockManager dockManager = _draggedDockGroup!.TheDockManager!;

            Point2D currentPoint = e.GetPosition(itemsContainer).ToPoint2D();

            if (currentPoint.Minus(_startMousePoint).ToAbs().GreaterOrEqual(PointHelper.MinimumDragDistance).Any)
            {
                _allowDrag = true;
            }

            if (!_allowDrag)
                return;

            bool allDone = MoveItemWithinContainer(itemsContainer, e);

            if (allDone)
            {
                return;
            }


            IDockGroup? parentItem = _draggedDockGroup.DockParent;
            IDockGroup topDockGroup = _draggedDockGroup.GetDockGroupRoot();

            Window parentWindow = parentItem.GetVisualAncestors().OfType<Window>().First();

            FloatingWindow? floatingWindow = parentWindow as FloatingWindow;

            FloatingWindow dockWindow;
            try
            {
                CurrentScreenPointBehavior.ReleaseCapture();

                floatingWindow?.SetCloseIsNotAllowed();

                Window ownerWindow = DockAttachedProperties.GetDockChildWindowOwner(parentWindow);

                DockStaticEvents.FirePossibleDockChangeHappenedInsideEvent(topDockGroup);

                // create the window
                dockWindow = dockManager.FloatingWindowFactory.CreateFloatingWindow();

                dockWindow.ProducingUserDefinedWindowGroup = 
                    _draggedDockGroup.ProducingUserDefinedWindowGroup;

                dockWindow.SetMovePtr();

                DockAttachedProperties.SetTheDockManager(dockWindow, dockManager);

                DockAttachedProperties.SetDockChildWindowOwner(dockWindow, ownerWindow);

                // remove from the current items
                _draggedDockGroup?.RemoveItselfFromParent();

                parentItem?.Simplify();

                _draggedDockGroup!.CleanSelfOnRemove();

                dockWindow.TheDockGroup.GroupOnlyById = 
                    _draggedDockGroup.GroupOnlyById;

                dockWindow.Width = _draggedDockGroup.FloatingSize.X;
                dockWindow.Height = _draggedDockGroup.FloatingSize.Y;
                dockWindow.TheDockGroup.DockChildren.Add(_draggedDockGroup!);
 
                ClearHandlers(sender);
            }
            finally
            {
                floatingWindow?.ResetIsCloseAllowed();

                floatingWindow?.CloseIfAllowed(false);

                if (floatingWindow != null && !floatingWindow.IsVisible)
                {
                    floatingWindow.TheDockManager = null;
                }

                void CleanUpFloatingWindow(FloatingWindow? win)
                {
                    win?.CloseIfAllowed();
                }

                void CleanUp()
                {
                    CleanUpFloatingWindow(floatingWindow);
                    CurrentScreenPointBehavior.PointerReleasedEvent -= CleanUp;
                }

                CurrentScreenPointBehavior.PointerReleasedEvent += CleanUp;
            }

            dockWindow.ShowDockWindow();
        }
    }
}
