using Avalonia.Controls;
using NP.Ava.UniDock;
using NP.Ava.UniDock.Factories;

namespace NP.ComplexViewModelSaveRestoreSample
{
    public class MyCustomFloatingWindowFactory : IFloatingWindowFactory
    {
        public virtual FloatingWindow CreateFloatingWindow()
        {
            // create the window

            FloatingWindow dockWindow = new FloatingWindow();

            dockWindow.Classes.Add("PlainFloatingWindow");
            dockWindow.Classes.Add("MyFloatingWindow");

            dockWindow.TitleClasses = "WindowTitle";

            return dockWindow;
        }
    }
}
