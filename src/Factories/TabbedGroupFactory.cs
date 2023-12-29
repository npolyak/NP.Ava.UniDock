namespace NP.Ava.UniDock.Factories
{
    public class TabbedGroupFactory : ITabbedGroupFactory
    {
        public TabbedDockGroup Create()
        {
            return new TabbedDockGroup();
        }
    }
}
