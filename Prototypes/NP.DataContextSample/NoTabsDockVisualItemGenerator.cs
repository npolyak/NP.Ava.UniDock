using Avalonia.Controls;
using NP.Ava.UniDock;
using NP.Ava.UniDock.Factories;

namespace NP.DataContextSample;

public class NoTabsDockVisualItemGenerator : IDockVisualItemGenerator
{
    public Control Generate(IDockGroup dockObj)
    {
        Control result;

        if (dockObj is DockItem dockItem)
        {
            dockItem.AllowCenterDocking = false;

            result = new DockItemPresenter 
            { 
                DockContext = dockItem 
            };
        }
        else
        {
            result = (Control)dockObj;
        }

        return result;
    }
}
