using NP.Ava.UniDock;
using NP.Ava.UniDock.Factories;
using NP.Ava.UniDockService;
using NP.DependencyInjection.Interfaces;
using NP.IoCy;

namespace NP.DataContextSample
{
    public static class MyContainer
    {
        public static IDependencyInjectionContainer<object> TheContainer { get; }

        public static DockManager TheDockManager { get; } = new DockManager()
        {
            AllowTabDocking = false, // testing
        };

        static MyContainer()
        {
            var containerBuilder = new ContainerBuilder();

            TheDockManager.IsInEditableState = true;

            containerBuilder.RegisterType<IFloatingWindowFactory, MyCustomFloatingWindowFactory>();

            containerBuilder.RegisterSingletonInstance<IUniDockService>(TheDockManager);

            TheContainer = containerBuilder.Build();
        }
    }
}
