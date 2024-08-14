using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using HelixToolkit.Wpf.SharpDX;

namespace Slicer14_v2;

public partial class RightSidePanelControl : UserControl
{
    public TreeView TreeView { get; private set; }
    public RightSidePanelControl()
    {
        InitializeComponent();
        DataContextChanged += RightSidePanelControl_DataContextChanged;
        TreeView = treeView; // Assume treeView is the name of your TreeView control in XAML
    }

    private void RightSidePanelControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldViewModel)
        {
            oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        if (e.NewValue is MainViewModel newViewModel)
        {
            newViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }
    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedModel))
        {
            // Just show a message box or write to the console for testing
            var viewModel = (MainViewModel)sender;
            var selectedModel = viewModel.SelectedModel;

            // Find the corresponding TreeViewItem and select it
            if (TreeView != null)
            {
                SelectTreeViewItem(TreeView, selectedModel);
            }
        }
    }
    private void SelectTreeViewItem(TreeView treeView, Element3D selectedModel)
{
    // If selectedModel is null, deselect all items
    if (selectedModel == null)
    {
        foreach (var item in treeView.Items)
        {
            var treeViewItem = (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromItem(item);
            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = false;
                // Deselect nested items as well
                SelectTreeViewItemRecursive(treeViewItem, selectedModel, true);
            }
        }
    }
    else
    {
        // Traverse through the TreeView items and find the matching one
        foreach (var item in treeView.Items)
        {
            if (item is Element3D model && model == selectedModel)
            {
                var treeViewItem = (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromItem(item);
                if (treeViewItem != null)
                {
                    treeViewItem.IsSelected = true;
                    treeViewItem.BringIntoView();
                }
                return;
            }

            if (item is TreeViewItem treeViewItem2)
            {
                if (SelectTreeViewItemRecursive(treeViewItem2, selectedModel))
                {
                    return;
                }
            }
        }
    }
}
    private bool SelectTreeViewItemRecursive(TreeViewItem parentItem, Element3D selectedModel, bool nullItem = false)
{
    foreach (var item in parentItem.Items)
    {
        var treeViewItem = (TreeViewItem)parentItem.ItemContainerGenerator.ContainerFromItem(item);
        if (treeViewItem != null)
        {
            if (nullItem)
            {
                treeViewItem.IsSelected = false;
            }
            else if (item is Element3D model && model == selectedModel)
            {
                treeViewItem.IsSelected = true;
                treeViewItem.BringIntoView();
                return true;
            }

            if (item is TreeViewItem treeViewItem2)
            {
                if (SelectTreeViewItemRecursive(treeViewItem2, selectedModel, nullItem))
                {
                    return true;
                }
            }
        }
    }

    return false;
}
    
    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is Element3D selectedElement)
        {
            // Set the ViewModel's SelectedModel property to the newly selected item in the TreeView
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                viewModel.SelectedModel = selectedElement;
            }
        }
    }

}



