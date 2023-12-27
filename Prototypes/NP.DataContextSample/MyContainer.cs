using NP.Avalonia.UniDock;
using NP.Avalonia.UniDock.Factories;
using NP.Avalonia.UniDockService;
using NP.DependencyInjection.Interfaces;
using NP.IoCy;

namespace NP.DataContextSample
{
    public static class MyContainer
    {
        public static IDependencyInjectionContainer<object> TheContainer { get; }

        public static DockManager TheDockManager { get; } = new DockManager();

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
