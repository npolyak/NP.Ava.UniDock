using Avalonia.Controls;

namespace NP.Ava.UniDock
{
    public interface IDockManagerContainer
    {
        DockManager? TheDockManager { get; set; }
    }
}
