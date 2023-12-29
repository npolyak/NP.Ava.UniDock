using System;

namespace NP.Ava.UniDock
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
