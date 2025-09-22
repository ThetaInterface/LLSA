using System.Linq;
using Avalonia.Controls;
using Avalonia.VisualTree;
using LLSA.Views;

namespace LLSA.Utilities;

public static class UtilitieManager
{
    public static void ClearAllTextBoxes(MainWindow window)
    {
        foreach (var textBox in window.GetVisualDescendants().OfType<TextBox>())
        {
            textBox.Text = string.Empty;
            textBox.Clear();
        }
    }
}