using Avalonia.Controls;
using System;

namespace NP.Ava.UniDock.Factories
{
    public class StackGroupFactory : IStackGroupFactory
    {
        public StackDockGroup Create()
        {
            return new StackDockGroup();
        }
    }
}
