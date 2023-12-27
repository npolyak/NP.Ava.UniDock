using System;

namespace NP.Avalonia.UniDock
{
    public static class DockStaticEvents
    {
        public static event Action<IDockGroup>? PossibleDockChangeHappenedInsideEvent;

        public static void FirePossibleDockChangeHappenedInsideEvent(IDockGroup dockGroup)
        {
            PossibleDockChangeHappenedInsideEvent?.Invoke(dockGroup);
        }
    }
}
