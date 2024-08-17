using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;

namespace Slicer14_v2;

public class Selection
{
    public ObservableCollection<Element3D> SelectedObjects { get; private set; }
    public GroupModel3D groupModel3D { get; private set; }
    public BoundingBox CustomBounds { get; private set; }

    public Selection()
    {
        SelectedObjects = new ObservableCollection<Element3D>();
        groupModel3D = new GroupModel3D();
    }

    public void AddObject(Element3D obj)
    {
        if (!SelectedObjects.Contains(obj))
        {
            SelectedObjects.Add(obj);
        }
        groupModel3D.Children.Add(obj);
        
        UpdateCustomBounds();
        Console.WriteLine($"BoundingBox {CustomBounds.Height} Count:{groupModel3D.Children.Count} Is initialized:{groupModel3D.IsInitialized} " +
                          $"{CustomBounds.Minimum.X} {CustomBounds.Minimum.Y} {CustomBounds.Minimum.Z}");
    }

    public void RemoveObject(Element3D obj)
    {
        if (SelectedObjects.Contains(obj))
        {
            if (obj is MeshGeometryModel3D meshModel)
            {
                groupModel3D.Children.Remove(obj);
                UpdateCustomBounds();
                meshModel.PostEffects = String.Empty;
            }
            SelectedObjects.Remove(obj);
        }
    }
    public void Clear()
    {
        foreach (var model in SelectedObjects)
        {
            if (model is MeshGeometryModel3D meshModel)
            {
                groupModel3D.Children.Remove(model);
                meshModel.PostEffects = string.Empty;
            }
        }

        UpdateCustomBounds();
        SelectedObjects.Clear();
    }

    private void UpdateCustomBounds()
    {
        if (!groupModel3D.Children.Any())
        {
            CustomBounds = new BoundingBox();
            return;
        }

        var firstBounds = groupModel3D.Children[0].Bounds;
        CustomBounds = new BoundingBox(firstBounds.Minimum, firstBounds.Maximum);

        foreach (var child in groupModel3D.Children)
        {
            CustomBounds = BoundingBox.Merge(CustomBounds, child.Bounds);
        }
    }
    public void HandleSelection(MeshGeometryModel3D meshModel, bool isShiftPressed)
    {
        if (SelectedObjects.Contains(meshModel))
        {
            Console.WriteLine("Removing model");
            RemoveObject(meshModel);
        }
        else
        {
            meshModel.PostEffects = "border[color:#00FFDE]";
            if (isShiftPressed)
            {
                AddObject(meshModel);
            }
            else
            {
                Clear();
                AddObject(meshModel);
            }
        }
    }
}