using Avalonia.Layout;

namespace NP.Avalonia.UniDock.Factories
{
    public interface IDockSeparatorFactory
    {
        bool ResizePreview { get; set; }

        DockSeparator GetDockSeparator(Orientation orientation);
    }
}
