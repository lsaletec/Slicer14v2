using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;

namespace Slicer14_v2;

public class Selection
{
    public ObservableCollection<Element3D> SelectedObjects { get; private set; }
    public GroupModel3D groupModel3D { get; private set; }
    public MeshGeometryModel3D selectionCenter { get; private set;}
    public BoundingBox CustomBounds { get; private set; }

    public Selection()
    {
        SelectedObjects = new ObservableCollection<Element3D>();
        groupModel3D = new GroupModel3D();
        selectionCenter= CreateSelectionCenterCube();
    }
    private MeshGeometryModel3D CreateSelectionCenterCube()
    {
        var meshBuilder = new MeshBuilder();
        meshBuilder.AddBox(new Vector3(0, 0, 0), 1, 1, 1); // Creates a 1x1x1 cube centered at the origin

        var geometry = meshBuilder.ToMeshGeometry3D();

        return new MeshGeometryModel3D
        {
            Geometry = geometry,
            Material = new PhongMaterial { DiffuseColor = Colors.Red.ToColor4() }
        };
    }
    public void AddObject(Element3D obj)
    {
        if (!SelectedObjects.Contains(obj))
        {
            SelectedObjects.Add(obj);
        }
        groupModel3D.Children.Add(obj);
        
        UpdateCustomBounds();
        UpdateSelectionCenter();
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
                UpdateSelectionCenter();
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
        UpdateSelectionCenter();
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
    private void UpdateSelectionCenter()
    {
        if (groupModel3D.Children.Count > 0)
        {
            var center = CustomBounds.Center();

            // Update the position of the selectionCenter to the center of the bounding box
            selectionCenter.Transform = new TranslateTransform3D(center.X, center.Y, center.Z);
        }
        else
        {
            selectionCenter.Transform = new TranslateTransform3D(0, 0, 0);
        }
    }
    
    public void ApplyTransformToSelectedObjects(Transform3D transform)
    {
        foreach (var obj in SelectedObjects)
        {
            if (obj is MeshGeometryModel3D meshModel)
            {
                var transformGroup = new Transform3DGroup();
                transformGroup.Children.Add(meshModel.Transform);
                transformGroup.Children.Add(transform);
                meshModel.Transform = transformGroup;
            }
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