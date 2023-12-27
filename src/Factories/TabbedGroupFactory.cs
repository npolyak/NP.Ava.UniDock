namespace NP.Avalonia.UniDock.Factories
{
    public class TabbedGroupFactory : ITabbedGroupFactory
    {
        public TabbedDockGroup Create()
        {
            return new TabbedDockGroup();
        }
    }
}
