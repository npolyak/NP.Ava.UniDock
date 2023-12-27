using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace NP.Avalonia.UniDock
{
    public interface IRecyclingTemplateContainer
    {
        IRecyclingDataTemplate? RecyclingDataTemplate { get; set; }

        Control? OldChild { get; set; }
    }
}
