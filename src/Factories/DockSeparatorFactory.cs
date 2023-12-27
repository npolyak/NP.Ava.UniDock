using Avalonia.Controls;
using Avalonia.Layout;

namespace NP.Avalonia.UniDock.Factories
{
    public class DockSeparatorFactory : IDockSeparatorFactory
    {
        public bool ResizePreview { get; set; } = false;

        public DockSeparator GetDockSeparator(Orientation orientation)
        {
            return new DockSeparator() { ShowsPreview = ResizePreview };
        }
    }
}
