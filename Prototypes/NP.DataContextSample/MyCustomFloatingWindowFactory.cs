using Avalonia.Controls;
using NP.Avalonia.UniDock;
using NP.Avalonia.UniDock.Factories;

namespace NP.DataContextSample
{
    public class MyCustomFloatingWindowFactory : IFloatingWindowFactory
    {
        public virtual FloatingWindow CreateFloatingWindow()
        {
            // create the window

            FloatingWindow dockWindow = new FloatingWindow();

            dockWindow.Classes = new Classes("PlainFloatingWindow", "MyFloatingWindow");
            dockWindow.TitleClasses = "WindowTitle";
            dockWindow.IsDockWindow = true;

            return dockWindow;
        }
    }
}
