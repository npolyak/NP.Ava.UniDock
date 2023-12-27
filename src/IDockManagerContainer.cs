using Avalonia.Controls;

namespace NP.Avalonia.UniDock
{
    public interface IDockManagerContainer
    {
        DockManager? TheDockManager { get; set; }
    }
}
