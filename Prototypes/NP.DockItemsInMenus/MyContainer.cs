using NP.Avalonia.UniDock.Factories;
using NP.DependencyInjection.Interfaces;
using NP.IoCy;

namespace NP.DockItemsInMenus
{
    public static class MyContainer
    {
        public static IDependencyInjectionContainer<object> TheContainer { get; }

        static MyContainer()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<IFloatingWindowFactory, MyCustomFloatingWindowFactory>();

            TheContainer = containerBuilder.Build();
        }
    }
}
