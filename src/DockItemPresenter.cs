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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using System;

namespace NP.Avalonia.UniDock
{
    public class DockItemPresenter : TemplatedControl
    {
        static DockItemPresenter()
        {
            DockContextProperty.Changed.AddClassHandler<DockItemPresenter>((x,e) => x.OnDockContextChanged(e));
        }

        #region DockContext Styled Avalonia Property
        public DockItem? DockContext
        {
            get { return GetValue(DockContextProperty); }
            set { SetValue(DockContextProperty, value); }
        }

        public static readonly StyledProperty<DockItem?> DockContextProperty =
            AvaloniaProperty.Register<DockItemPresenter, DockItem?>
            (
                nameof(DockContext)
            );
        #endregion DockContext Styled Avalonia Property

        private void OnDockContextChanged(AvaloniaPropertyChangedEventArgs e)
        {
            DockItem? oldDockItem = (DockItem?)e.OldValue;

            if (oldDockItem != null)
            {
                oldDockItem.TheVisual = null;
            }

            DockItem? newDockItem = (DockItem?)e.NewValue;

            if (newDockItem != null)
            {
                newDockItem.TheVisual = this;
            }
        }


        public DockItemPresenter()
        {
            this.AddHandler(PointerPressedEvent, OnPointerPressedFired, RoutingStrategies.Bubble, true);

            (this as IVisual).DetachedFromVisualTree += DockItemPresenter_DetachedFromVisualTree;
        }

        private void DockItemPresenter_DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            this.RemoveHandler(PointerPressedEvent, OnPointerPressedFired);
        }

        private void OnPointerPressedFired(object? sender, PointerPressedEventArgs e)
        {
            (this.DockContext as IActiveItem<DockItem>)?.MakeActive();
        }


        #region HeaderBackground Styled Avalonia Property
        public IBrush HeaderBackground
        {
            get { return GetValue(HeaderBackgroundProperty); }
            set { SetValue(HeaderBackgroundProperty, value); }
        }

        public static readonly StyledProperty<IBrush> HeaderBackgroundProperty =
            AvaloniaProperty.Register<DockItemPresenter, IBrush>
            (
                nameof(HeaderBackground)
            );
        #endregion HeaderBackground Styled Avalonia Property


        #region HeaderForeground Styled Avalonia Property
        public IBrush HeaderForeground
        {
            get { return GetValue(HeaderForegroundProperty); }
            set { SetValue(HeaderForegroundProperty, value); }
        }

        public static readonly StyledProperty<IBrush> HeaderForegroundProperty =
            AvaloniaProperty.Register<DockItemPresenter, IBrush>
            (
                nameof(HeaderForeground)
            );
        #endregion HeaderForeground Styled Avalonia Property


        #region IsActive Styled Avalonia Property
        public bool IsActive
        {
            get { return GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<DockItemPresenter, bool>
            (
                nameof(IsActive)
            );
        #endregion IsActive Styled Avalonia Property


        #region IsFullyActive Styled Avalonia Property
        public bool IsFullyActive
        {
            get { return GetValue(IsFullyActiveProperty); }
            set { SetValue(IsFullyActiveProperty, value); }
        }

        public static readonly StyledProperty<bool> IsFullyActiveProperty =
            AvaloniaProperty.Register<DockItemPresenter, bool>
            (
                nameof(IsFullyActive)
            );
        #endregion IsFullyActive Styled Avalonia Property


        #region ShowHeader Styled Avalonia Property
        public bool ShowHeader
        {
            get { return GetValue(ShowHeaderProperty); }
            set { SetValue(ShowHeaderProperty, value); }
        }

        public static readonly AttachedProperty<bool> ShowHeaderProperty =
            DockAttachedProperties.ShowHeaderProperty.AddOwner<DockItemPresenter>();
        #endregion ShowHeader Styled Avalonia Property
    }
}
