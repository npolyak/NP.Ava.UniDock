using Avalonia.Controls;
using NP.Ava.UniDock;
using NP.Ava.UniDock.Factories;

namespace NP.ViewModelSample
{
    public class MyCustomFloatingWindowFactory : IFloatingWindowFactory
    {
        public virtual FloatingWindow CreateFloatingWindow()
        {
            // create the window

            FloatingWindow dockWindow = new FloatingWindow();

            dockWindow.Classes.AddRange(new[] { "PlainFloatingWindow", "MyFloatingWindow" });
            dockWindow.TitleClasses = "WindowTitle";

            return dockWindow;
        }
    }
}
