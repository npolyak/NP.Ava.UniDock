using Avalonia.Controls;
using NP.Ava.UniDock;
using NP.Ava.UniDock.Factories;

namespace NP.GroupHeaderSample
{
    public class MyCustomFloatingWindowFactory : IFloatingWindowFactory
    {
        public virtual FloatingWindow CreateFloatingWindow()
        {
            // create the window

            FloatingWindow dockWindow = new FloatingWindow();

            dockWindow.Classes.AddRange(new[] { "PlainFloatingWindow", "MyFloatingWindow" });
            dockWindow.TitleClasses = "WindowTitle";
            dockWindow.IsDockWindow = true;

            return dockWindow;
        }
    }
}
