using NP.Ava.UniDock;
using NP.Ava.UniDock.Factories;
using NP.UniDockService;
using NP.DependencyInjection.Interfaces;
using NP.IoCy;

namespace NP.ComplexViewModelSaveRestoreSample
{
    public static class MyContainer
    {
        public static IDependencyInjectionContainer<object> TheContainer { get; }

        public static DockManager TheDockManager { get; } = new DockManager();

        static MyContainer()
        {
            var containerBuilder = new ContainerBuilder();


            containerBuilder.RegisterSingletonType<IFloatingWindowFactory, MyCustomFloatingWindowFactory>();
            containerBuilder.RegisterSingletonInstance<DockManager>(TheDockManager);
            //TheContainer.MapSingleton<IUniDockService, DockManager>(TheDockManager, null, true);

            TheContainer = containerBuilder.Build();
        }
    }
}
