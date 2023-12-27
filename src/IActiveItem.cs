using System;

namespace NP.Avalonia.UniDock
{
    public interface IActiveItem<T>
        where T : IActiveItem<T>
    {
        bool IsActive { get; set; }

        event Action<T>? IsActiveChanged;

        void MakeActive()
        {
            IsActive = true;
        }
    }
}
