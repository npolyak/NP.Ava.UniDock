using NP.Avalonia.UniDock.Factories;
using NP.IoCy;
using NP.DependencyInjection.Interfaces;

namespace NP.StableFloatingWindows
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
