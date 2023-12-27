using Avalonia.Controls;
using System;

namespace NP.Avalonia.UniDock.Factories
{
    public class StackGroupFactory : IStackGroupFactory
    {
        public StackDockGroup Create()
        {
            return new StackDockGroup();
        }
    }
}
