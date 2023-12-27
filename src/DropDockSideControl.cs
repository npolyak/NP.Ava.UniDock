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
using System;

namespace NP.Avalonia.UniDock
{
    public class DropDockSideControl : TemplatedControl
    {
        public DockKind SelectDockSide { get; set; }

        private IDisposable? _disposableSubscription = null;
        public DropDockSideControl()
        {
            _disposableSubscription =
                 DockAttachedProperties.DockSideProperty.Changed.Subscribe(OnDockSideChanged);
        }

        private void OnDockSideChanged(AvaloniaPropertyChangedEventArgs<DockKind?> dockSideChange)
        {
            if (dockSideChange.Sender != this)
                return;

            IsSelected = dockSideChange.NewValue.Value == this.SelectDockSide;
        }

        #region IsSelected Direct Avalonia Property
        public static readonly DirectProperty<DropDockSideControl, bool> IsSelectedProperty =
            AvaloniaProperty.RegisterDirect<DropDockSideControl, bool>
            (
                nameof(IsSelected),
                o => o.IsSelected,
                (o, v) => o.IsSelected = v
            );

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            private set
            {
                SetAndRaise(IsSelectedProperty, ref _isSelected, value);
            }
        }
        #endregion IsSelected Direct Avalonia Property
    }
}
