using Avalonia;
using Avalonia.Controls;

namespace NP.Ava.UniDock.Factories
{
    public class FloatingWindowFactory : IFloatingWindowFactory
    {
        public virtual FloatingWindow CreateFloatingWindow()
        {
            // create the window

            FloatingWindow dockWindow = new FloatingWindow();

            dockWindow.Classes.Add("PlainFloatingWindow");
            dockWindow.TitleClasses = "WindowTitle";

            dockWindow.IsDockWindow = true;

            return dockWindow;
        }
    }
}
