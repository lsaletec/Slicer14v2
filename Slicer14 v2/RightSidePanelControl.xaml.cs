using System.Windows;
using System.Windows.Controls;
using HelixToolkit.Wpf.SharpDX;

namespace Slicer14_v2;

public partial class RightSidePanelControl : UserControl
{
    public RightSidePanelControl()
    {
        InitializeComponent();
    }
    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.SelectedModel = e.NewValue as Element3D;
        }
    }
}