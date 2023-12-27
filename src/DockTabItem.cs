using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using System;

namespace NP.Avalonia.UniDock
{
    public class DockTabItem : TabItem, IStyleable
    {
        #region IsActive Styled Avalonia Property
        public bool IsActive
        {
            get { return GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<DockTabItem, bool>
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
            AvaloniaProperty.Register<DockTabItem, bool>
            (
                nameof(IsFullyActive)
            );
        #endregion IsFullyActive Styled Avalonia Property

        public DockTabItem()
        {
            this.AddHandler(PointerPressedEvent, OnPointerPressedFired, RoutingStrategies.Bubble, true);
        }

        private void OnPointerPressedFired(object? sender, PointerPressedEventArgs e)
        {
            (this.Content as IActiveItem<DockItem>)?.MakeActive();
        }
    }
}
