using Avalonia.Layout;

namespace NP.Ava.UniDock.Factories
{
    public interface IDockSeparatorFactory
    {
        bool ResizePreview { get; set; }

        DockSeparator GetDockSeparator(Orientation orientation);
    }
}
