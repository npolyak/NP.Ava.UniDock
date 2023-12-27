using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace NP.Avalonia.UniDock
{
    public class DockSeparator : GridSplitter
    {
        #region TheOrientation Styled Avalonia Property
        public Orientation TheOrientation
        {
            get { return GetValue(TheOrientationProperty); }
            set { SetValue(TheOrientationProperty, value); }
        }

        public static readonly StyledProperty<Orientation> TheOrientationProperty =
            AvaloniaProperty.Register<DockSeparator, Orientation>
            (
                nameof(TheOrientation)
            );
        #endregion TheOrientation Styled Avalonia Property

        public DockSeparator()
        {
            ShowsPreview = true;
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            // do not cancel resize on lost focus
            //base.OnLostFocus(e);
        }

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            base.OnLostFocus(e);
        }
    }
}
